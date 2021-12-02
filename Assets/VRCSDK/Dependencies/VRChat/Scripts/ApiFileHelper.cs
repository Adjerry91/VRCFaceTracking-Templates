#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;

namespace VRC.Core
{
    public class ApiFileHelper : MonoBehaviour
    {
        private readonly int kMultipartUploadChunkSize = 100 * 1024 * 1024; // 100 MB
        private readonly int SERVER_PROCESSING_WAIT_TIMEOUT_CHUNK_SIZE = 50 * 1024 * 1024;
        private readonly float SERVER_PROCESSING_WAIT_TIMEOUT_PER_CHUNK_SIZE = 120.0f;
        private readonly float SERVER_PROCESSING_MAX_WAIT_TIMEOUT = 600.0f;
        private readonly float SERVER_PROCESSING_INITIAL_RETRY_TIME = 2.0f;
        private readonly float SERVER_PROCESSING_MAX_RETRY_TIME = 10.0f;

        private static bool EnableDeltaCompression = false;

        private readonly Regex[] kUnityPackageAssetNameFilters = new Regex[]
        {
            new Regex(@"/LightingData\.asset$"),                    // lightmap base asset
            new Regex(@"/Lightmap-.*(\.png|\.exr)$"),               // lightmaps
            new Regex(@"/ReflectionProbe-.*(\.exr|\.png)$"),        // reflection probes
            new Regex(@"/Editor/Data/UnityExtensions/")             // anything that looks like part of the Unity installation
        };

        public delegate void OnFileOpSuccess(ApiFile apiFile, string message);
        public delegate void OnFileOpError(ApiFile apiFile, string error);
        public delegate void OnFileOpProgress(ApiFile apiFile, string status, string subStatus, float pct);
        public delegate bool FileOpCancelQuery(ApiFile apiFile);

        public static ApiFileHelper Instance
        {
            get
            {
                CheckInstance();
                return mInstance;
            }
        }

        private static ApiFileHelper mInstance = null;
        const float kPostWriteDelay = 0.75f;

        public enum FileOpResult
        {
            Success,
            Unchanged
        }

        public static string GetMimeTypeFromExtension(string extension)
        {
            if (extension == ".vrcw")
                    return "application/x-world";
            if (extension == ".vrca")
                    return "application/x-avatar";
            if (extension == ".dll")
                    return "application/x-msdownload";
            if (extension == ".unitypackage")
                return "application/gzip";
            if (extension == ".gz")
                    return "application/gzip";
            if (extension == ".jpg")
                    return "image/jpg";
            if (extension == ".png")
                    return "image/png";
            if (extension == ".sig")
                    return "application/x-rsync-signature";
            if (extension == ".delta")
                    return "application/x-rsync-delta";

            Debug.LogWarning("Unknown file extension for mime-type: " + extension);
            return "application/octet-stream";
        }

        public static bool IsGZipCompressed(string filename)
        {
            return GetMimeTypeFromExtension(Path.GetExtension(filename)) == "application/gzip";
        }

        public IEnumerator UploadFile(string filename, string existingFileId, string friendlyName,
            OnFileOpSuccess onSuccess, OnFileOpError onError, OnFileOpProgress onProgress, FileOpCancelQuery cancelQuery)
        {
            VRC.Core.Logger.Log("UploadFile: filename: " + filename + ", file id: " +
                       (!string.IsNullOrEmpty(existingFileId) ? existingFileId : "<new>") + ", name: " + friendlyName, DebugLevel.All);

            // init remote config
            if (!ConfigManager.RemoteConfig.IsInitialized())
            {
                bool done = false;
                ConfigManager.RemoteConfig.Init(
                    delegate () { done = true; },
                    delegate () { done = true; }
                );

                while (!done)
                    yield return null;

                if (!ConfigManager.RemoteConfig.IsInitialized())
                {
                    Error(onError, null, "Failed to fetch configuration.");
                    yield break;
                }
            }

            // configure delta compression
            {
                EnableDeltaCompression = ConfigManager.RemoteConfig.GetBool("sdkEnableDeltaCompression", false);
            }

            // validate input file
            Progress(onProgress, null, "Checking file...");

            if (string.IsNullOrEmpty(filename))
            {
                Error(onError, null, "Upload filename is empty!");
                yield break;
            }

            if (!System.IO.Path.HasExtension(filename))
            {
                Error(onError, null, "Upload filename must have an extension: " + filename);
                yield break;
            }

            string whyNot;
            if (!VRC.Tools.FileCanRead(filename, out whyNot))
            {
                Error(onError, null, "Could not read file to upload!", filename + "\n" + whyNot);
                yield break;
            }

            // get or create ApiFile
            Progress(onProgress, null, string.IsNullOrEmpty(existingFileId) ? "Creating file record..." : "Getting file record...");

            bool wait = true;
            bool wasError = false;
            bool worthRetry = false;
            string errorStr = "";

            if (string.IsNullOrEmpty(friendlyName))
                friendlyName = filename;

            string extension = System.IO.Path.GetExtension(filename);
            string mimeType = GetMimeTypeFromExtension(extension);

            ApiFile apiFile = null;

            System.Action<ApiContainer> fileSuccess = (ApiContainer c) =>
            {
                apiFile = c.Model as ApiFile;
                wait = false;
            };

            System.Action<ApiContainer> fileFailure = (ApiContainer c) =>
            {
                errorStr = c.Error;
                wait = false;

                if (c.Code == 400)
                    worthRetry = true;
            };

            while (true)
            {
                apiFile = null;
                wait = true;
                worthRetry = false;
                errorStr = "";

                if (string.IsNullOrEmpty(existingFileId))
                    ApiFile.Create(friendlyName, mimeType, extension, fileSuccess, fileFailure);
                else
                    API.Fetch<ApiFile>(existingFileId, fileSuccess, fileFailure);

                while (wait)
                {
                    if (apiFile != null && CheckCancelled(cancelQuery, onError, apiFile))
                        yield break;

                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    if (errorStr.Contains("File not found"))
                    {
                        Debug.LogError("Couldn't find file record: " + existingFileId + ", creating new file record");

                        existingFileId = "";
                        continue;
                    }

                    string msg = string.IsNullOrEmpty(existingFileId) ? "Failed to create file record." : "Failed to get file record.";
                    Error(onError, null, msg, errorStr);

                    if (!worthRetry)
                        yield break;
                }

                if (!worthRetry)
                    break;
                else
                    yield return new WaitForSecondsRealtime(kPostWriteDelay);
            }

            if (apiFile == null)
                yield break;

            LogApiFileStatus(apiFile, false, true);

            while (apiFile.HasQueuedOperation(EnableDeltaCompression))
            {
                wait = true;

                apiFile.DeleteLatestVersion((c) => wait = false, (c) => wait = false);

                while (wait)
                {
                    if (apiFile != null && CheckCancelled(cancelQuery, onError, apiFile))
                        yield break;

                    yield return null;
                }
            }

            // delay to let write get through servers
            yield return new WaitForSecondsRealtime(kPostWriteDelay);

            LogApiFileStatus(apiFile, false);

            // check for server side errors from last upload
            if (apiFile.IsInErrorState())
            {
                Debug.LogWarning("ApiFile: " + apiFile.id + ": server failed to process last uploaded, deleting failed version");

                while (true)
                {
                    // delete previous failed version
                    Progress(onProgress, apiFile, "Preparing file for upload...", "Cleaning up previous version");

                    wait = true;
                    errorStr = "";
                    worthRetry = false;

                    apiFile.DeleteLatestVersion(fileSuccess, fileFailure);

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onError, null))
                        {
                            yield break;
                        }

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        Error(onError, apiFile, "Failed to delete previous failed version!", errorStr);
                        if (!worthRetry)
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }
                    }

                    if (worthRetry)
                        yield return new WaitForSecondsRealtime(kPostWriteDelay);
                    else
                        break;
                }
            }

            // delay to let write get through servers
            yield return new WaitForSecondsRealtime(kPostWriteDelay);

            LogApiFileStatus(apiFile, false);

            // verify previous file op is complete
            if (apiFile.HasQueuedOperation(EnableDeltaCompression))
            {
                Error(onError, apiFile, "A previous upload is still being processed. Please try again later.");
                yield break;
            }

            if (wasError)
                yield break;

            LogApiFileStatus(apiFile, false);

            // generate md5 and check if file has changed
            Progress(onProgress, apiFile, "Preparing file for upload...", "Generating file hash");

            string fileMD5Base64 = "";
            wait = true;
            errorStr = "";
            VRC.Tools.FileMD5(filename,
                delegate (byte[] md5Bytes)
                {
                    fileMD5Base64 = Convert.ToBase64String(md5Bytes);
                    wait = false;
                },
                delegate (string error)
                {
                    errorStr = filename + "\n" + error;
                    wait = false;
                }
            );

            while (wait)
            {
                if (CheckCancelled(cancelQuery, onError, apiFile))
                {
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }
                yield return null;
            }

            if (!string.IsNullOrEmpty(errorStr))
            {
                Error(onError, apiFile, "Failed to generate MD5 hash for upload file.", errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            LogApiFileStatus(apiFile, false);

            // check if file has been changed
            Progress(onProgress, apiFile, "Preparing file for upload...", "Checking for changes");

            bool isPreviousUploadRetry = false;
            if (apiFile.HasExistingOrPendingVersion())
            {
                // uploading the same file?
                if (string.Compare(fileMD5Base64, apiFile.GetFileMD5(apiFile.GetLatestVersionNumber())) == 0)
                {
                    // the previous operation completed successfully?
                    if (!apiFile.IsWaitingForUpload())
                    {
                        Success(onSuccess, apiFile, "The file to upload is unchanged.");
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }
                    else
                    {
                        isPreviousUploadRetry = true;

                        Debug.Log("Retrying previous upload");
                    }
                }
                else
                {
                    // the file has been modified
                    if (apiFile.IsWaitingForUpload())
                    {
                        // previous upload failed, and the file is changed
                        while (true)
                        {
                            // delete previous failed version
                            Progress(onProgress, apiFile, "Preparing file for upload...", "Cleaning up previous version");

                            wait = true;
                            worthRetry = false;
                            errorStr = "";

                            apiFile.DeleteLatestVersion(fileSuccess, fileFailure);

                            while (wait)
                            {
                                if (CheckCancelled(cancelQuery, onError, apiFile))
                                {
                                    yield break;
                                }
                                yield return null;
                            }

                            if (!string.IsNullOrEmpty(errorStr))
                            {
                                Error(onError, apiFile, "Failed to delete previous incomplete version!", errorStr);
                                if (!worthRetry)
                                {
                                    CleanupTempFiles(apiFile.id);
                                    yield break;
                                }
                            }

                            // delay to let write get through servers
                            yield return new WaitForSecondsRealtime(kPostWriteDelay);

                            if (!worthRetry)
                                break;
                        }
                    }
                }
            }

            LogApiFileStatus(apiFile, false);

            // generate signature for new file

            Progress(onProgress, apiFile, "Preparing file for upload...", "Generating signature");

            string signatureFilename = VRC.Tools.GetTempFileName(".sig", out errorStr, apiFile.id);
            if (string.IsNullOrEmpty(signatureFilename))
            {
                Error(onError, apiFile, "Failed to generate file signature!", "Failed to create temp file: \n" + errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            wasError = false;
            yield return StartCoroutine(CreateFileSignatureInternal(filename, signatureFilename,
                delegate ()
                {
                    // success!
                },
                delegate (string error)
                {
                    Error(onError, apiFile, "Failed to generate file signature!", error);
                    CleanupTempFiles(apiFile.id);
                    wasError = true;
                })
            );

            if (wasError)
                yield break;

            LogApiFileStatus(apiFile, false);

            // generate signature md5 and file size
            Progress(onProgress, apiFile, "Preparing file for upload...", "Generating signature hash");

            string sigMD5Base64 = "";
            wait = true;
            errorStr = "";
            VRC.Tools.FileMD5(signatureFilename,
                delegate (byte[] md5Bytes)
                {
                    sigMD5Base64 = Convert.ToBase64String(md5Bytes);
                    wait = false;
                },
                delegate (string error)
                {
                    errorStr = signatureFilename + "\n" + error;
                    wait = false;
                }
            );

            while (wait)
            {
                if (CheckCancelled(cancelQuery, onError, apiFile))
                {
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }
                yield return null;
            }

            if (!string.IsNullOrEmpty(errorStr))
            {
                Error(onError, apiFile, "Failed to generate MD5 hash for signature file.", errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            long sigFileSize = 0;
            if (!VRC.Tools.GetFileSize(signatureFilename, out sigFileSize, out errorStr))
            {
                Error(onError, apiFile, "Failed to generate file signature!", "Couldn't get file size:\n" + errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            LogApiFileStatus(apiFile, false);

            // download previous version signature (if exists)
            string existingFileSignaturePath = null;
            if (EnableDeltaCompression && apiFile.HasExistingVersion())
            {
                Progress(onProgress, apiFile, "Preparing file for upload...", "Downloading previous version signature");

                wait = true;
                errorStr = "";
                apiFile.DownloadSignature(
                    delegate (byte[] data)
                    {
                        // save to temp file
                        existingFileSignaturePath = VRC.Tools.GetTempFileName(".sig", out errorStr, apiFile.id);
                        if (string.IsNullOrEmpty(existingFileSignaturePath))
                        {
                            errorStr = "Failed to create temp file: \n" + errorStr;
                            wait = false;
                        }
                        else
                        {
                            try
                            {
                                File.WriteAllBytes(existingFileSignaturePath, data);
                            }
                            catch (Exception e)
                            {
                                existingFileSignaturePath = null;
                                errorStr = "Failed to write signature temp file:\n" + e.Message;
                            }
                            wait = false;
                        }
                    },
                    delegate (string error)
                    {
                        errorStr = error;
                        wait = false;
                    },
                    delegate (long downloaded, long length)
                    {
                        Progress(onProgress, apiFile, "Preparing file for upload...", "Downloading previous version signature", Tools.DivideSafe(downloaded, length));
                    }
                );

                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onError, apiFile))
                    {
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    Error(onError, apiFile, "Failed to download previous file version signature.", errorStr);
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }
            }

            LogApiFileStatus(apiFile, false);

            // create delta if needed
            string deltaFilename = null;

            if (EnableDeltaCompression && !string.IsNullOrEmpty(existingFileSignaturePath))
            {
                Progress(onProgress, apiFile, "Preparing file for upload...", "Creating file delta");

                deltaFilename = VRC.Tools.GetTempFileName(".delta", out errorStr, apiFile.id);
                if (string.IsNullOrEmpty(deltaFilename))
                {
                    Error(onError, apiFile, "Failed to create file delta for upload.", "Failed to create temp file: \n" + errorStr);
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }

                wasError = false;
                yield return StartCoroutine(CreateFileDeltaInternal(filename, existingFileSignaturePath, deltaFilename,
                    delegate ()
                    {
                        // success!
                    },
                    delegate (string error)
                    {
                        Error(onError, apiFile, "Failed to create file delta for upload.", error);
                        CleanupTempFiles(apiFile.id);
                        wasError = true;
                    })
                );

                if (wasError)
                    yield break;
            }

            // upload smaller of delta and new file
            long fullFileSize = 0;
            long deltaFileSize = 0;
            if (!VRC.Tools.GetFileSize(filename, out fullFileSize, out errorStr) ||
                (!string.IsNullOrEmpty(deltaFilename) && !VRC.Tools.GetFileSize(deltaFilename, out deltaFileSize, out errorStr)))
            {
                Error(onError, apiFile, "Failed to create file delta for upload.", "Couldn't get file size: " + errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            bool uploadDeltaFile = EnableDeltaCompression && deltaFileSize > 0 && deltaFileSize < fullFileSize;
            if (EnableDeltaCompression)
                VRC.Core.Logger.Log("Delta size " + deltaFileSize + " (" + ((float)deltaFileSize / (float)fullFileSize) + " %), full file size " + fullFileSize + ", uploading " + (uploadDeltaFile ? " DELTA" : " FULL FILE"), DebugLevel.All);
            else
                VRC.Core.Logger.Log("Delta compression disabled, uploading FULL FILE, size " + fullFileSize, DebugLevel.All);

            LogApiFileStatus(apiFile, uploadDeltaFile);

            string deltaMD5Base64 = "";
            if (uploadDeltaFile)
            {
                Progress(onProgress, apiFile, "Preparing file for upload...", "Generating file delta hash");

                wait = true;
                errorStr = "";
                VRC.Tools.FileMD5(deltaFilename,
                    delegate (byte[] md5Bytes)
                    {
                        deltaMD5Base64 = Convert.ToBase64String(md5Bytes);
                        wait = false;
                    },
                    delegate (string error)
                    {
                        errorStr = error;
                        wait = false;
                    }
                );

                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onError, apiFile))
                    {
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    Error(onError, apiFile, "Failed to generate file delta hash.", errorStr);
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }
            }

            // validate existing pending version info, if this is a retry
            bool versionAlreadyExists = false;

            LogApiFileStatus(apiFile, uploadDeltaFile);

            if (isPreviousUploadRetry)
            {
                bool isValid = true;

                ApiFile.Version v = apiFile.GetVersion(apiFile.GetLatestVersionNumber());
                if (v != null)
                {
                    if (uploadDeltaFile)
                    {
                        isValid = deltaFileSize == v.delta.sizeInBytes &&
                            deltaMD5Base64.CompareTo(v.delta.md5) == 0 &&
                            sigFileSize == v.signature.sizeInBytes &&
                            sigMD5Base64.CompareTo(v.signature.md5) == 0;
                    }
                    else
                    {
                        isValid = fullFileSize == v.file.sizeInBytes &&
                            fileMD5Base64.CompareTo(v.file.md5) == 0 &&
                            sigFileSize == v.signature.sizeInBytes &&
                            sigMD5Base64.CompareTo(v.signature.md5) == 0;
                    }
                }
                else
                {
                    isValid = false;
                }

                if (isValid)
                {
                    versionAlreadyExists = true;

                    Debug.Log("Using existing version record");
                }
                else
                {
                    // delete previous invalid version
                    Progress(onProgress, apiFile, "Preparing file for upload...", "Cleaning up previous version");

                    while (true)
                    {
                        wait = true;
                        errorStr = "";
                        worthRetry = false;

                        apiFile.DeleteLatestVersion(fileSuccess, fileFailure);

                        while (wait)
                        {
                            if (CheckCancelled(cancelQuery, onError, null))
                            {
                                yield break;
                            }
                            yield return null;
                        }

                        if (!string.IsNullOrEmpty(errorStr))
                        {
                            Error(onError, apiFile, "Failed to delete previous incomplete version!", errorStr);
                            if (!worthRetry)
                            {
                                CleanupTempFiles(apiFile.id);
                                yield break;
                            }
                        }

                        // delay to let write get through servers
                        yield return new WaitForSecondsRealtime(kPostWriteDelay);

                        if (!worthRetry)
                            break;
                    }
                }
            }

            LogApiFileStatus(apiFile, uploadDeltaFile);

            // create new version of file
            if (!versionAlreadyExists)
            {
                while (true)
                {
                    Progress(onProgress, apiFile, "Creating file version record...");

                    wait = true;
                    errorStr = "";
                    worthRetry = false;

                    if (uploadDeltaFile)
                        // delta file
                        apiFile.CreateNewVersion(ApiFile.Version.FileType.Delta, deltaMD5Base64, deltaFileSize, sigMD5Base64, sigFileSize, fileSuccess, fileFailure);
                    else
                        // full file
                        apiFile.CreateNewVersion(ApiFile.Version.FileType.Full, fileMD5Base64, fullFileSize, sigMD5Base64, sigFileSize, fileSuccess, fileFailure);

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onError, apiFile))
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        Error(onError, apiFile, "Failed to create file version record.", errorStr);
                        if (!worthRetry)
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }
                    }

                    // delay to let write get through servers
                    yield return new WaitForSecondsRealtime(kPostWriteDelay);

                    if (!worthRetry)
                        break;
                }
            }

            // upload components

            LogApiFileStatus(apiFile, uploadDeltaFile);

            // upload delta
            if (uploadDeltaFile)
            {
                if (apiFile.GetLatestVersion().delta.status == ApiFile.Status.Waiting)
                {
                    Progress(onProgress, apiFile, "Uploading file delta...");

                    wasError = false;
                    yield return StartCoroutine(UploadFileComponentInternal(apiFile,
                        ApiFile.Version.FileDescriptor.Type.delta, deltaFilename, deltaMD5Base64, deltaFileSize,
                        delegate (ApiFile file)
                        {
                            Debug.Log("Successfully uploaded file delta.");
                            apiFile = file;
                        },
                        delegate (string error)
                        {
                            Error(onError, apiFile, "Failed to upload file delta.", error);
                            CleanupTempFiles(apiFile.id);
                            wasError = true;
                        },
                        delegate (long downloaded, long length)
                        {
                            Progress(onProgress, apiFile, "Uploading file delta...", "", Tools.DivideSafe(downloaded, length));
                        },
                        cancelQuery)
                    );

                    if (wasError)
                        yield break;
                }
            }
            // upload file
            else
            {
                if (apiFile.GetLatestVersion().file.status == ApiFile.Status.Waiting)
                {
                    Progress(onProgress, apiFile, "Uploading file...");

                    wasError = false;
                    yield return StartCoroutine(UploadFileComponentInternal(apiFile,
                        ApiFile.Version.FileDescriptor.Type.file, filename, fileMD5Base64, fullFileSize,
                        delegate (ApiFile file)
                        {
                            VRC.Core.Logger.Log("Successfully uploaded file.", DebugLevel.All);
                            apiFile = file;
                        },
                        delegate (string error)
                        {
                            Error(onError, apiFile, "Failed to upload file.", error);
                            CleanupTempFiles(apiFile.id);
                            wasError = true;
                        },
                        delegate (long downloaded, long length)
                        {
                            Progress(onProgress, apiFile, "Uploading file...", "", Tools.DivideSafe(downloaded, length));
                        },
                        cancelQuery)
                    );

                    if (wasError)
                        yield break;
                }
            }

            LogApiFileStatus(apiFile, uploadDeltaFile);

            // upload signature
            if (apiFile.GetLatestVersion().signature.status == ApiFile.Status.Waiting)
            {
                Progress(onProgress, apiFile, "Uploading file signature...");

                wasError = false;
                yield return StartCoroutine(UploadFileComponentInternal(apiFile,
                    ApiFile.Version.FileDescriptor.Type.signature, signatureFilename, sigMD5Base64, sigFileSize,
                    delegate (ApiFile file)
                    {
                        VRC.Core.Logger.Log("Successfully uploaded file signature.", DebugLevel.All);
                        apiFile = file;
                    },
                    delegate (string error)
                    {
                        Error(onError, apiFile, "Failed to upload file signature.", error);
                        CleanupTempFiles(apiFile.id);
                        wasError = true;
                    },
                    delegate (long downloaded, long length)
                    {
                        Progress(onProgress, apiFile, "Uploading file signature...", "", Tools.DivideSafe(downloaded, length));
                    },
                    cancelQuery)
                );

                if (wasError)
                    yield break;
            }

            LogApiFileStatus(apiFile, uploadDeltaFile);

            // Validate file records queued or complete
            Progress(onProgress, apiFile, "Validating upload...");

            bool isUploadComplete = (uploadDeltaFile
                ? apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), ApiFile.Version.FileDescriptor.Type.delta).status == ApiFile.Status.Complete
                : apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), ApiFile.Version.FileDescriptor.Type.file).status == ApiFile.Status.Complete);
            isUploadComplete = isUploadComplete &&
                               apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), ApiFile.Version.FileDescriptor.Type.signature).status == ApiFile.Status.Complete;

            if (!isUploadComplete)
            {
                Error(onError, apiFile, "Failed to upload file.", "Record status is not 'complete'");
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            bool isServerOpQueuedOrComplete = (uploadDeltaFile
                ? apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), ApiFile.Version.FileDescriptor.Type.file).status != ApiFile.Status.Waiting
                : apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), ApiFile.Version.FileDescriptor.Type.delta).status != ApiFile.Status.Waiting);

            if (!isServerOpQueuedOrComplete)
            {
                Error(onError, apiFile, "Failed to upload file.", "Record is still in 'waiting' status");
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            LogApiFileStatus(apiFile, uploadDeltaFile);

            // wait for server processing to complete
            Progress(onProgress, apiFile, "Processing upload...");
            float checkDelay = SERVER_PROCESSING_INITIAL_RETRY_TIME;
            float maxDelay = SERVER_PROCESSING_MAX_RETRY_TIME;
            float timeout = GetServerProcessingWaitTimeoutForDataSize(apiFile.GetLatestVersion().file.sizeInBytes);
            double initialStartTime = Time.realtimeSinceStartup;
            double startTime = initialStartTime;
            while (apiFile.HasQueuedOperation(uploadDeltaFile))
            {
                // wait before polling again
                Progress(onProgress, apiFile, "Processing upload...", "Checking status in " + Mathf.CeilToInt(checkDelay) + " seconds");

                while (Time.realtimeSinceStartup - startTime < checkDelay)
                {
                    if (CheckCancelled(cancelQuery, onError, apiFile))
                    {
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }

                    if (Time.realtimeSinceStartup - initialStartTime > timeout)
                    {
                        LogApiFileStatus(apiFile, uploadDeltaFile);

                        Error(onError, apiFile, "Timed out waiting for upload processing to complete.");
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }

                    yield return null;
                }

                while (true)
                {
                    // check status
                    Progress(onProgress, apiFile, "Processing upload...", "Checking status...");

                    wait = true;
                    worthRetry = false;
                    errorStr = "";
                    API.Fetch<ApiFile>(apiFile.id, fileSuccess, fileFailure);

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onError, apiFile))
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        Error(onError, apiFile, "Checking upload status failed.", errorStr);
                        if (!worthRetry)
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }
                    }

                    if (!worthRetry)
                        break;
                }

                checkDelay = Mathf.Min(checkDelay * 2, maxDelay);
                startTime = Time.realtimeSinceStartup;
            }

            // cleanup and wait for it to finish
            yield return StartCoroutine(CleanupTempFilesInternal(apiFile.id));

            Success(onSuccess, apiFile, "Upload complete!");
        }

        private static void LogApiFileStatus(ApiFile apiFile, bool checkDelta, bool logSuccess = false)
        {
            if (apiFile == null || !apiFile.IsInitialized)
            {
                Debug.LogFormat("<color=yellow>apiFile not initialized</color>");
            }
            else if (apiFile.IsInErrorState())
            {
                Debug.LogFormat("<color=yellow>ApiFile {0} is in an error state.</color>", apiFile.name);
            }
            else if (logSuccess)
                VRC.Core.Logger.Log("< color = yellow > Processing { 3}: { 0}, { 1}, { 2}</ color > " +
                           (apiFile.IsWaitingForUpload() ? "waiting for upload" : "upload complete") +
                           (apiFile.HasExistingOrPendingVersion() ? "has existing or pending version" : "no previous version") +
                           (apiFile.IsLatestVersionQueued(checkDelta) ? "latest version queued" : "latest version not queued") +
                           apiFile.name, DebugLevel.All);

            if (apiFile != null && apiFile.IsInitialized && logSuccess)
            {
                var apiFields = apiFile.ExtractApiFields();
                if (apiFields != null)
                    VRC.Core.Logger.Log("<color=yellow>{0}</color>" + VRC.Tools.JsonEncode(apiFields), DebugLevel.All);
            }
        }

        public IEnumerator CreateFileSignatureInternal(string filename, string outputSignatureFilename, Action onSuccess, Action<string> onError)
        {
            VRC.Core.Logger.Log("CreateFileSignature: " + filename + " => " + outputSignatureFilename, DebugLevel.All);

            yield return null;

            Stream inStream = null;
            FileStream outStream = null;
            byte[] buf = new byte[64 * 1024];
            IAsyncResult asyncRead = null;
            IAsyncResult asyncWrite = null;

            try
            {
                inStream = librsync.net.Librsync.ComputeSignature(File.OpenRead(filename));
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't open input file: " + e.Message);
                yield break;
            }

            try
            {
                outStream = File.Open(outputSignatureFilename, FileMode.Create, FileAccess.Write);
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't create output file: " + e.Message);
                yield break;
            }

            while (true)
            {
                try
                {
                    asyncRead = inStream.BeginRead(buf, 0, buf.Length, null, null);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                while (!asyncRead.IsCompleted)
                    yield return null;

                int read = 0;
                try
                {
                    read = inStream.EndRead(asyncRead);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                if (read <= 0)
                    break;

                try
                {
                    asyncWrite = outStream.BeginWrite(buf, 0, read, null, null);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't write file: " + e.Message);
                    yield break;
                }

                while (!asyncWrite.IsCompleted)
                    yield return null;

                try
                {
                    outStream.EndWrite(asyncWrite);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't write file: " + e.Message);
                    yield break;
                }
            }

            inStream.Close();
            outStream.Close();

            yield return null;

            if (onSuccess != null)
                onSuccess();
        }

        public IEnumerator CreateFileDeltaInternal(string newFilename, string existingFileSignaturePath, string outputDeltaFilename, Action onSuccess, Action<string> onError)
        {
            Debug.Log("CreateFileDelta: " + newFilename + " (delta) " + existingFileSignaturePath + " => " + outputDeltaFilename);

            yield return null;

            Stream inStream = null;
            FileStream outStream = null;
            byte[] buf = new byte[64 * 1024];
            IAsyncResult asyncRead = null;
            IAsyncResult asyncWrite = null;

            try
            {
                inStream = librsync.net.Librsync.ComputeDelta(File.OpenRead(existingFileSignaturePath), File.OpenRead(newFilename));
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't open input file: " + e.Message);
                yield break;
            }

            try
            {
                outStream = File.Open(outputDeltaFilename, FileMode.Create, FileAccess.Write);
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't create output file: " + e.Message);
                yield break;
            }

            while (true)
            {
                try
                {
                    asyncRead = inStream.BeginRead(buf, 0, buf.Length, null, null);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                while (!asyncRead.IsCompleted)
                    yield return null;

                int read = 0;
                try
                {
                    read = inStream.EndRead(asyncRead);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                if (read <= 0)
                    break;

                try
                {
                    asyncWrite = outStream.BeginWrite(buf, 0, read, null, null);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't write file: " + e.Message);
                    yield break;
                }

                while (!asyncWrite.IsCompleted)
                    yield return null;

                try
                {
                    outStream.EndWrite(asyncWrite);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't write file: " + e.Message);
                    yield break;
                }
            }

            inStream.Close();
            outStream.Close();

            yield return null;

            if (onSuccess != null)
                onSuccess();
        }

        protected static void Success(OnFileOpSuccess onSuccess, ApiFile apiFile, string message)
        {
            if (apiFile == null)
                apiFile = new ApiFile();

            VRC.Core.Logger.Log("ApiFile " + apiFile.ToStringBrief() + ": Operation Succeeded!", DebugLevel.All);
            if (onSuccess != null)
                onSuccess(apiFile, message);
        }

        protected static void Error(OnFileOpError onError, ApiFile apiFile, string error, string moreInfo = "")
        {
            if (apiFile == null)
                apiFile = new ApiFile();

            Debug.LogError("ApiFile " + apiFile.ToStringBrief() + ": Error: " + error + "\n" + moreInfo);
            if (onError != null)
                onError(apiFile, error);
        }

        protected static void Progress(OnFileOpProgress onProgress, ApiFile apiFile, string status, string subStatus = "", float pct = 0.0f)
        {
            if (apiFile == null)
                apiFile = new ApiFile();

            if (onProgress != null)
                onProgress(apiFile, status, subStatus, pct);
        }

        protected static bool CheckCancelled(FileOpCancelQuery cancelQuery, OnFileOpError onError, ApiFile apiFile)
        {
            if (apiFile == null)
            {
                Debug.LogError("apiFile was null");
                return true;
            }

            if (cancelQuery != null && cancelQuery(apiFile))
            {
                Debug.Log("ApiFile " + apiFile.ToStringBrief() + ": Operation cancelled");
                if (onError != null)
                    onError(apiFile, "Cancelled by user.");
                return true;
            }

            return false;
        }

        protected static void CleanupTempFiles(string subFolderName)
        {
            Instance.StartCoroutine(Instance.CleanupTempFilesInternal(subFolderName));
        }

        protected IEnumerator CleanupTempFilesInternal(string subFolderName)
        {
            if (!string.IsNullOrEmpty(subFolderName))
            {
                string folder = VRC.Tools.GetTempFolderPath(subFolderName);

                while (Directory.Exists(folder))
                {
                    try
                    {
                        if (Directory.Exists(folder))
                            Directory.Delete(folder, true);
                    }
                    catch (System.Exception)
                    {
                    }

                    yield return null;
                }
            }
        }

        private static void CheckInstance()
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject("ApiFileHelper");
                mInstance = go.AddComponent<ApiFileHelper>();

                try
                {
                    GameObject.DontDestroyOnLoad(go);
                }
                catch
                {
                }
            }
        }

        private float GetServerProcessingWaitTimeoutForDataSize(int size)
        {
            float timeoutMultiplier = Mathf.Ceil((float)size / (float)SERVER_PROCESSING_WAIT_TIMEOUT_CHUNK_SIZE);
            return Mathf.Clamp(timeoutMultiplier * SERVER_PROCESSING_WAIT_TIMEOUT_PER_CHUNK_SIZE, SERVER_PROCESSING_WAIT_TIMEOUT_PER_CHUNK_SIZE, SERVER_PROCESSING_MAX_WAIT_TIMEOUT);
        }

        private bool UploadFileComponentValidateFileDesc(ApiFile apiFile, string filename, string md5Base64, long fileSize, ApiFile.Version.FileDescriptor fileDesc, Action<ApiFile> onSuccess, Action<string> onError)
        {
            if (fileDesc.status != ApiFile.Status.Waiting)
            {
                // nothing to do (might be a retry)
                Debug.Log("UploadFileComponent: (file record not in waiting status, done)");
                if (onSuccess != null)
                    onSuccess(apiFile);
                return false;
            }

            if (fileSize != fileDesc.sizeInBytes)
            {
                if (onError != null)
                    onError("File size does not match version descriptor");
                return false;
            }
            if (string.Compare(md5Base64, fileDesc.md5) != 0)
            {
                if (onError != null)
                    onError("File MD5 does not match version descriptor");
                return false;
            }

            // make sure file is right size
            long tempSize = 0;
            string errorStr = "";
            if (!VRC.Tools.GetFileSize(filename, out tempSize, out errorStr))
            {
                if (onError != null)
                    onError("Couldn't get file size");
                return false;
            }
            if (tempSize != fileSize)
            {
                if (onError != null)
                    onError("File size does not match input size");
                return false;
            }

            return true;
        }

        private IEnumerator UploadFileComponentDoSimpleUpload(ApiFile apiFile,
            ApiFile.Version.FileDescriptor.Type fileDescriptorType,
            string filename,
            string md5Base64,
            long fileSize,
            Action<ApiFile> onSuccess,
            Action<string> onError,
            Action<long, long> onProgress,
            FileOpCancelQuery cancelQuery)
        {
            OnFileOpError onCancelFunc = delegate (ApiFile file, string s)
            {
                if (onError != null)
                    onError(s);
            };

            string uploadUrl = "";
            while (true)
            {
                bool wait = true;
                string errorStr = "";
                bool worthRetry = false;

                apiFile.StartSimpleUpload(fileDescriptorType,
                    (c) =>
                    {
                        uploadUrl = (c as ApiDictContainer).ResponseDictionary["url"] as string;
                        wait = false;
                    },
                    (c) =>
                    {
                        errorStr = "Failed to start upload: " + c.Error;
                        wait = false;
                        if (c.Code == 400)
                            worthRetry = true;
                    });

                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    if (onError != null)
                        onError(errorStr);
                    if (!worthRetry)
                        yield break;
                }

                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                if (!worthRetry)
                    break;
            }

            // PUT file
            {
                bool wait = true;
                string errorStr = "";

                VRC.HttpRequest req = ApiFile.PutSimpleFileToURL(uploadUrl, filename, GetMimeTypeFromExtension(Path.GetExtension(filename)), md5Base64, true,
                    delegate ()
                    {
                        wait = false;
                    },
                    delegate (string error)
                    {
                        errorStr = "Failed to upload file: " + error;
                        wait = false;
                    },
                    delegate (long uploaded, long length)
                    {
                        if (onProgress != null)
                            onProgress(uploaded, length);
                    }
                );

                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        if (req != null)
                            req.Abort();
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    if (onError != null)
                        onError(errorStr);
                    yield break;
                }
            }

            // finish upload
            while (true)
            {
                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                bool wait = true;
                string errorStr = "";
                bool worthRetry = false;

                apiFile.FinishUpload(fileDescriptorType, null,
                    (c) =>
                    {
                        apiFile = c.Model as ApiFile;
                        wait = false;
                    },
                    (c) =>
                    {
                        errorStr = "Failed to finish upload: " + c.Error;
                        wait = false;
                        if (c.Code == 400)
                            worthRetry = false;
                    });

                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    if (onError != null)
                        onError(errorStr);
                    if (!worthRetry)
                        yield break;
                }

                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                if (!worthRetry)
                    break;
            }

        }

        private IEnumerator UploadFileComponentDoMultipartUpload(ApiFile apiFile,
            ApiFile.Version.FileDescriptor.Type fileDescriptorType,
            string filename,
            string md5Base64,
            long fileSize,
            Action<ApiFile> onSuccess,
            Action<string> onError,
            Action<long, long> onProgress,
            FileOpCancelQuery cancelQuery)
        {
            FileStream fs = null;
            OnFileOpError onCancelFunc = delegate (ApiFile file, string s)
            {
                if (fs != null)
                    fs.Close();
                if (onError != null)
                    onError(s);
            };

            // query multipart upload status.
            // we might be resuming a previous upload
            ApiFile.UploadStatus uploadStatus = null;
            {
                while (true)
                {
                    bool wait = true;
                    string errorStr = "";
                    bool worthRetry = false;

                    apiFile.GetUploadStatus(apiFile.GetLatestVersionNumber(), fileDescriptorType,
                       (c) =>
                       {
                           uploadStatus = c.Model as ApiFile.UploadStatus;
                           wait = false;

                           VRC.Core.Logger.Log("Found existing multipart upload status (next part = " + uploadStatus.nextPartNumber + ")", DebugLevel.All);
                       },
                       (c) =>
                       {
                           errorStr = "Failed to query multipart upload status: " + c.Error;
                           wait = false;
                           if (c.Code == 400)
                               worthRetry = true;
                       });

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                        {
                            yield break;
                        }
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        if (onError != null)
                            onError(errorStr);
                        if (!worthRetry)
                            yield break;
                    }

                    if (!worthRetry)
                        break;
                }
            }

            // split file into chunks
            try
            {
                fs = File.OpenRead(filename);
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't open file: " + e.Message);
                yield break;
            }

            byte[] buffer = new byte[kMultipartUploadChunkSize * 2];

            long totalBytesUploaded = 0;
            List<string> etags = new List<string>();
            if (uploadStatus != null)
                etags = uploadStatus.etags.ToList();

            int numParts = Mathf.Max(1, Mathf.FloorToInt((float)fs.Length / (float)kMultipartUploadChunkSize));
            for (int partNumber = 1; partNumber <= numParts; partNumber++)
            {
                // read chunk
                int bytesToRead = partNumber < numParts ? kMultipartUploadChunkSize : (int)(fs.Length - fs.Position);
                int bytesRead = 0;
                try
                {
                    bytesRead = fs.Read(buffer, 0, bytesToRead);
                }
                catch (Exception e)
                {
                    fs.Close();
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                if (bytesRead != bytesToRead)
                {
                    fs.Close();
                    if (onError != null)
                        onError("Couldn't read file: read incorrect number of bytes from stream");
                    yield break;
                }

                // check if this part has been upload already
                // NOTE: uploadStatus.nextPartNumber == number of parts already uploaded
                if (uploadStatus != null && partNumber <= uploadStatus.nextPartNumber)
                {
                    totalBytesUploaded += bytesRead;
                    continue;
                }

                // start upload
                string uploadUrl = "";

                while (true)
                {
                    bool wait = true;
                    string errorStr = "";
                    bool worthRetry = false;

                    apiFile.StartMultipartUpload(fileDescriptorType, partNumber,
                        (c) =>
                        {
                            uploadUrl = (c as ApiDictContainer).ResponseDictionary["url"] as string;
                            wait = false;
                        },
                        (c) =>
                        {
                            errorStr = "Failed to start part upload: " + c.Error;
                            wait = false;
                        });

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                        {
                            yield break;
                        }
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        fs.Close();
                        if (onError != null)
                            onError(errorStr);
                        if (!worthRetry)
                            yield break;
                    }

                    // delay to let write get through servers
                    yield return new WaitForSecondsRealtime(kPostWriteDelay);

                    if (!worthRetry)
                        break;
                }

                // PUT file part
                {
                    bool wait = true;
                    string errorStr = "";

                    VRC.HttpRequest req = ApiFile.PutMultipartDataToURL(uploadUrl, buffer, bytesRead, GetMimeTypeFromExtension(Path.GetExtension(filename)), true,
                        delegate (string etag)
                        {
                            if (!string.IsNullOrEmpty(etag))
                                etags.Add(etag);
                            totalBytesUploaded += bytesRead;
                            wait = false;
                        },
                        delegate (string error)
                        {
                            errorStr = "Failed to upload data: " + error;
                            wait = false;
                        },
                        delegate (long uploaded, long length)
                        {
                            if (onProgress != null)
                                onProgress(totalBytesUploaded + uploaded, fileSize);
                        }
                    );

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                        {
                            if (req != null)
                                req.Abort();
                            yield break;
                        }
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        fs.Close();
                        if (onError != null)
                            onError(errorStr);
                        yield break;
                    }
                }
            }

            // finish upload
            while (true)
            {
                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                bool wait = true;
                string errorStr = "";
                bool worthRetry = false;

                apiFile.FinishUpload(fileDescriptorType, etags,
                    (c) =>
                    {
                        apiFile = c.Model as ApiFile;
                        wait = false;
                    },
                    (c) =>
                    {
                        errorStr = "Failed to finish upload: " + c.Error;
                        wait = false;
                        if (c.Code == 400)
                            worthRetry = true;
                    });

                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    fs.Close();
                    if (onError != null)
                        onError(errorStr);
                    if (!worthRetry)
                        yield break;
                }

                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                if (!worthRetry)
                    break;
            }

            fs.Close();
        }

        private IEnumerator UploadFileComponentVerifyRecord(ApiFile apiFile,
            ApiFile.Version.FileDescriptor.Type fileDescriptorType,
            string filename,
            string md5Base64,
            long fileSize,
            ApiFile.Version.FileDescriptor fileDesc,
            Action<ApiFile> onSuccess,
            Action<string> onError,
            Action<long, long> onProgress,
            FileOpCancelQuery cancelQuery)
        {
            OnFileOpError onCancelFunc = delegate (ApiFile file, string s)
            {
                if (onError != null)
                    onError(s);
            };

            float initialStartTime = Time.realtimeSinceStartup;
            float startTime = initialStartTime;
            float timeout = GetServerProcessingWaitTimeoutForDataSize(fileDesc.sizeInBytes);
            float waitDelay = SERVER_PROCESSING_INITIAL_RETRY_TIME;
            float maxDelay = SERVER_PROCESSING_MAX_RETRY_TIME;

            while (true)
            {
                if (apiFile == null)
                {
                    if (onError != null)
                        onError("ApiFile is null");
                    yield break;
                }

                var desc = apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), fileDescriptorType);
                if (desc == null)
                {
                    if (onError != null)
                        onError("File descriptor is null ('" + fileDescriptorType + "')");
                    yield break;
                }

                if (desc.status != ApiFile.Status.Waiting)
                {
                    // upload completed or is processing
                    break;
                }

                // wait for next poll
                while (Time.realtimeSinceStartup - startTime < waitDelay)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        yield break;
                    }

                    if (Time.realtimeSinceStartup - initialStartTime > timeout)
                    {
                        if (onError != null)
                            onError("Couldn't verify upload status: Timed out wait for server processing");
                        yield break;
                    }

                    yield return null;
                }


                while (true)
                {
                    bool wait = true;
                    string errorStr = "";
                    bool worthRetry = false;

                    apiFile.Refresh(
                        (c) =>
                        {
                            wait = false;
                        },
                        (c) =>
                        {
                            errorStr = "Couldn't verify upload status: " + c.Error;
                            wait = false;
                            if (c.Code == 400)
                                worthRetry = true;
                        });

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                        {
                            yield break;
                        }

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        if (onError != null)
                            onError(errorStr);
                        if (!worthRetry)
                            yield break;
                    }

                    if (!worthRetry)
                        break;
                }

                waitDelay = Mathf.Min(waitDelay * 2, maxDelay);
                startTime = Time.realtimeSinceStartup;
            }

            if (onSuccess != null)
                onSuccess(apiFile);
        }

        private IEnumerator UploadFileComponentInternal(ApiFile apiFile,
            ApiFile.Version.FileDescriptor.Type fileDescriptorType,
            string filename,
            string md5Base64,
            long fileSize,
            Action<ApiFile> onSuccess,
            Action<string> onError,
            Action<long, long> onProgress,
            FileOpCancelQuery cancelQuery)
        {
            VRC.Core.Logger.Log("UploadFileComponent: " + fileDescriptorType + " (" + apiFile.id + "): " + filename, DebugLevel.All);
            ApiFile.Version.FileDescriptor fileDesc = apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), fileDescriptorType);

            if (!UploadFileComponentValidateFileDesc(apiFile, filename, md5Base64, fileSize, fileDesc, onSuccess, onError))
                yield break;

            switch (fileDesc.category)
            {
                case ApiFile.Category.Simple:
                    yield return UploadFileComponentDoSimpleUpload(apiFile, fileDescriptorType, filename, md5Base64, fileSize, onSuccess, onError, onProgress, cancelQuery);
                    break;
                case ApiFile.Category.Multipart:
                    yield return UploadFileComponentDoMultipartUpload(apiFile, fileDescriptorType, filename, md5Base64, fileSize, onSuccess, onError, onProgress, cancelQuery);
                    break;
                default:
                    if (onError != null)
                        onError("Unknown file category type: " + fileDesc.category);
                    yield break;
            }

            yield return UploadFileComponentVerifyRecord(apiFile, fileDescriptorType, filename, md5Base64, fileSize, fileDesc, onSuccess, onError, onProgress, cancelQuery);
        }
    }
}

#endif
