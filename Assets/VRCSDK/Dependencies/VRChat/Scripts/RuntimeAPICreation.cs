using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VRC.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRCSDK2
{
#if UNITY_EDITOR

    public class RuntimeAPICreation : MonoBehaviour
    {
        public VRC.Core.PipelineManager pipelineManager;

        protected bool forceNewFileCreation = false;
        protected bool useFileApi = false;

        protected bool isUploading = false;
        protected float uploadProgress = 0f;
        protected string uploadMessage;
        protected string uploadTitle;

        protected string uploadVrcPath;
        protected string uploadUnityPackagePath;

        protected string cloudFrontAssetUrl;
        protected string cloudFrontImageUrl;
        protected string cloudFrontUnityPackageUrl;

        protected CameraImageCapture imageCapture;

        private bool cancelRequested = false;

        public static bool publishingToCommunityLabs = false;

        private Dictionary<string, string> mRetryState = new Dictionary<string, string>();

        protected bool isUpdate { get { return pipelineManager.completedSDKPipeline; } }

        protected void Start()
        {
            if (!Application.isEditor || !Application.isPlaying)
                return;

            PipelineSaver ps = GameObject.FindObjectOfType<PipelineSaver>();
            pipelineManager = ps.gameObject.GetComponent<PipelineManager>();

            imageCapture = GetComponent<CameraImageCapture>();
            imageCapture.shotCamera = GameObject.Find("VRCCam").GetComponent<Camera>();

            LoadUploadRetryStateFromCache();

            forceNewFileCreation = UnityEditor.EditorPrefs.GetBool("forceNewFileCreation", true);
            useFileApi = UnityEditor.EditorPrefs.GetBool("useFileApi", false);

            API.SetOnlineMode(true);
        }

        protected void Update()
        {
            if (isUploading)
            {
                bool cancelled = UnityEditor.EditorUtility.DisplayCancelableProgressBar(uploadTitle, uploadMessage, uploadProgress);
                if (cancelled)
                {
                    cancelRequested = true;
                }

                if (EditorApplication.isPaused)
                    EditorApplication.isPaused = false;

            }
        }

        protected void LoadUploadRetryStateFromCache()
        {
            try
            {
                string json = File.ReadAllText(GetUploadRetryStateFilePath());
                mRetryState = VRC.Tools.ObjDictToStringDict(VRC.Tools.JsonDecode(json) as Dictionary<string, object>);

                Debug.LogFormat("<color=yellow> loaded retry state: {0}</color>", json);
            }
            catch (Exception)
            {
                // normal case
                return;
            }

            Debug.Log("Loaded upload retry state from: " + GetUploadRetryStateFilePath());
        }

        protected void SaveUploadRetryState(string key, string val)
        {
            if (string.IsNullOrEmpty(val))
                return;
            mRetryState[key] = val;
            SaveUploadRetryState();
        }

        protected void SaveUploadRetryState()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GetUploadRetryStateFilePath()));
                string json = VRC.Tools.JsonEncode(mRetryState);
                File.WriteAllText(GetUploadRetryStateFilePath(), json);

                Debug.LogFormat("<color=yellow> wrote retry state: {0}</color>", json);
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't save upload retry state: " + GetUploadRetryStateFilePath() + "\n" + e.Message);
                return;
            }

            Debug.Log("Saved upload retry state to: " + GetUploadRetryStateFilePath());
        }

        protected void ClearUploadRetryState()
        {
            try
            {
                if (!File.Exists(GetUploadRetryStateFilePath()))
                    return;

                File.Delete(GetUploadRetryStateFilePath());
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't delete upload retry state: " + GetUploadRetryStateFilePath() + "\n" + e.Message);
                return;
            }

            Debug.Log("Cleared upload retry state at: " + GetUploadRetryStateFilePath());
        }

        protected string GetUploadRetryStateFilePath()
        {
            string id = UnityEditor.AssetDatabase.AssetPathToGUID(SceneManager.GetActiveScene().path);
            return Path.Combine(VRC.Tools.GetTempFolderPath(id), "upload_retry.dat");
        }

        protected string GetUploadRetryStateValue(string key)
        {
            return mRetryState.ContainsKey(key) ? mRetryState[key] : "";
        }

        protected virtual void DisplayUpdateCompletedDialog(string contentUrl=null)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("VRChat SDK", "Update Complete! Launch VRChat to see your uploaded content." + (null==contentUrl ? "" : "\n\nManage content at: " + contentUrl ), (null == contentUrl) ? "Okay" : CommunityLabsConstants.MANAGE_WORLD_IN_BROWSER_STRING, (null == contentUrl) ? "" : "Done" ))
            {
                if (null!=contentUrl)
                {
                    Application.OpenURL(contentUrl);
                }
            }
        }

        protected void OnSDKPipelineComplete(string contentUrl=null)
        {
            VRC.Core.Logger.Log("OnSDKPipelineComplete", DebugLevel.All);
            isUploading = false;
            pipelineManager.completedSDKPipeline = true;
            ClearUploadRetryState();
            UnityEditor.EditorPrefs.SetBool("forceNewFileCreation", false);
            UnityEditor.EditorApplication.isPaused = false;
            UnityEditor.EditorApplication.isPlaying = false;
            UnityEditor.EditorUtility.ClearProgressBar();
            DisplayUpdateCompletedDialog(contentUrl);
        }

        protected void OnSDKPipelineError(string error, string details)
        {
            VRC.Core.Logger.Log("OnSDKPipelineError: " + error + " - " + details, DebugLevel.All);
            isUploading = false;
            pipelineManager.completedSDKPipeline = true;
            UnityEditor.EditorApplication.isPaused = false;
            UnityEditor.EditorApplication.isPlaying = false;
            UnityEditor.EditorUtility.ClearProgressBar();
            if (cancelRequested)
                UnityEditor.EditorUtility.DisplayDialog("VRChat SDK", "The update was cancelled.", "Okay");
            else
                UnityEditor.EditorUtility.DisplayDialog("VRChat SDK", "Error updating content. " + error + "\n" + details, "Okay");
        }

        protected void SetUploadProgress(string title, string message, float progress)
        {
            uploadTitle = title;
            uploadMessage = message;
            uploadProgress = progress;
        }

        protected bool WasCancelRequested(ApiFile apiFile)
        {
            return cancelRequested;
        }

        protected void PrepareUnityPackageForS3(string packagePath, string blueprintId, int version, AssetVersion assetVersion)
        {
            uploadUnityPackagePath = Application.temporaryCachePath + "/" + blueprintId + "_" + version.ToString() + "_" + Application.unityVersion + "_" + assetVersion.ApiVersion + "_" + VRC.Tools.Platform +
                "_" + API.GetServerEnvironmentForApiUrl() + ".unitypackage";
            uploadUnityPackagePath.Trim();
            uploadUnityPackagePath.Replace(' ', '_');

            if (System.IO.File.Exists(uploadUnityPackagePath))
                System.IO.File.Delete(uploadUnityPackagePath);

            System.IO.File.Copy(packagePath, uploadUnityPackagePath);
        }

        protected void PrepareVRCPathForS3(string abPath, string blueprintId, int version, AssetVersion assetVersion)
        {
            uploadVrcPath = Application.temporaryCachePath + "/" + blueprintId + "_" + version.ToString() + "_" + Application.unityVersion + "_" + assetVersion.ApiVersion + "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl() + System.IO.Path.GetExtension(abPath);
            uploadVrcPath.Trim();
            uploadVrcPath.Replace(' ', '_');

            if (System.IO.File.Exists(uploadVrcPath))
                System.IO.File.Delete(uploadVrcPath);

            System.IO.File.Copy(abPath, uploadVrcPath);
        }

        protected IEnumerator UploadFile(string filename, string existingFileUrl, string friendlyFilename, string fileType, Action<string> onSuccess)
        {
            if (string.IsNullOrEmpty(filename))
                yield break;
            VRC.Core.Logger.Log("Uploading " + fileType + "(" + filename + ") ...", DebugLevel.All);
            SetUploadProgress("Uploading " + fileType + "...", "", 0.0f);

            string fileId = GetUploadRetryStateValue(filename);
            if (string.IsNullOrEmpty(fileId))
                fileId = isUpdate ? ApiFile.ParseFileIdFromFileAPIUrl(existingFileUrl) : "";
            string errorStr = "";
            string newFileUrl = "";


            yield return StartCoroutine(ApiFileHelper.Instance.UploadFile(filename, forceNewFileCreation ? "" : fileId, friendlyFilename,
                delegate (ApiFile apiFile, string message)
                {
                    newFileUrl = apiFile.GetFileURL();
                    if (VRC.Core.Logger.DebugLevelIsEnabled(DebugLevel.API))
                        VRC.Core.Logger.Log(fileType + " upload succeeded: " + message + " (" + filename +
                              ") => " + apiFile.ToString(), DebugLevel.API);
                    else
                        VRC.Core.Logger.Log(fileType + " upload succeeded ", DebugLevel.Always);
                },
                delegate (ApiFile apiFile, string error)
                {
                    SaveUploadRetryState(filename, apiFile.id);

                    errorStr = error;
                    Debug.LogError(fileType + " upload failed: " + error + " (" + filename +
                                   ") => " + apiFile.ToString());
                },
                delegate (ApiFile apiFile, string status, string subStatus, float pct)
                {
                    SetUploadProgress("Uploading " + fileType + "...", status + (!string.IsNullOrEmpty(subStatus) ? " (" + subStatus + ")" : ""), pct);
                },
                WasCancelRequested
            ));

            if (!string.IsNullOrEmpty(errorStr))
            {
                OnSDKPipelineError(fileType + " upload failed.", errorStr);
                yield break;
            }

            if (onSuccess != null)
                onSuccess(newFileUrl);
        }

        protected IEnumerator UpdateImage(string existingFileUrl, string friendlyFileName)
        {
            string imagePath = imageCapture.TakePicture();
            
            if (!string.IsNullOrEmpty(imagePath))
            {
                yield return StartCoroutine(UploadFile(imagePath, existingFileUrl, friendlyFileName, "Image",
                    delegate (string fileUrl)
                    {
                        cloudFrontImageUrl = fileUrl;
                    }
                ));
            }
        }

        protected virtual IEnumerator CreateBlueprint()
        {
            throw new NotImplementedException();
        }

        protected virtual IEnumerator UpdateBlueprint()
        {
            throw new NotImplementedException();
        }

        protected bool ValidateNameInput(InputField nameInput)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(nameInput.text))
            {
                isUploading = false;
                UnityEditor.EditorUtility.DisplayDialog("Invalid Input", "Cannot leave the name field empty.", "OK");
                isValid = false;
            }
            return isValid;
        }

        protected bool ValidateAssetBundleBlueprintID(string blueprintID)
        {
            string lastBuiltID = UnityEditor.EditorPrefs.GetString("lastBuiltAssetBundleBlueprintID", "");
            return !string.IsNullOrEmpty(lastBuiltID) && lastBuiltID == blueprintID;
        }
    }
#endif

}
