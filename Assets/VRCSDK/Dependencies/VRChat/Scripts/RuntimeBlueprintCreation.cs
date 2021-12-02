using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VRC.Core;
using VRC.SDKBase;

namespace VRCSDK2
{
#if UNITY_EDITOR
    public class RuntimeBlueprintCreation : RuntimeAPICreation
    {
        public GameObject waitingPanel;
        public GameObject blueprintPanel;
        public GameObject errorPanel;

        public Text titleText;
        public InputField blueprintName;
        public InputField blueprintDescription;
        public RawImage bpImage;
        public Image liveBpImage;
        public Toggle shouldUpdateImageToggle;
        public Toggle contentSex;
        public Toggle contentViolence;
        public Toggle contentGore;
        public Toggle contentOther;
        public Toggle developerAvatar;
        public Toggle sharePrivate;
        public Toggle sharePublic;
        public Toggle tagFallback;

        public UnityEngine.UI.Button uploadButton;

        private ApiAvatar apiAvatar;

        new void Start()
        {
            if (!Application.isEditor || !Application.isPlaying)
                return;

            base.Start();

            var desc = pipelineManager.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>();
            desc.PositionPortraitCamera(imageCapture.shotCamera.transform);

            Application.runInBackground = true;
            UnityEngine.XR.XRSettings.enabled = false;

            uploadButton.onClick.AddListener(SetupUpload);

            shouldUpdateImageToggle.onValueChanged.AddListener(ToggleUpdateImage);

            Login();
        }

        void LoginErrorCallback(string obj)
        {
            VRC.Core.Logger.LogError("Could not log in - " + obj, DebugLevel.Always);
            blueprintPanel.SetActive(false);
            errorPanel.SetActive(true);
        }

        void Login()
        {
            if (!ApiCredentials.Load())
                LoginErrorCallback("Not logged in");
            else
                APIUser.InitialFetchCurrentUser(
                    delegate (ApiModelContainer<APIUser> c)
                    {
                        pipelineManager.user = c.Model as APIUser;

                        ApiAvatar av = new ApiAvatar() { id = pipelineManager.blueprintId };
                        av.Get(false,
                            (c2) =>
                            {
                                VRC.Core.Logger.Log("<color=magenta>Updating an existing avatar.</color>", DebugLevel.API);
                                apiAvatar = c2.Model as ApiAvatar;
                                pipelineManager.completedSDKPipeline = !string.IsNullOrEmpty(apiAvatar.authorId);
                                SetupUI();
                            },
                            (c2) =>
                            {
                                VRC.Core.Logger.Log("<color=magenta>Creating a new avatar.</color>", DebugLevel.API);
                                apiAvatar = new ApiAvatar();
                                apiAvatar.id = pipelineManager.blueprintId;
                                pipelineManager.completedSDKPipeline = !string.IsNullOrEmpty(apiAvatar.authorId);
                                SetupUI();
                            });
                    }, (c) => {
                        LoginErrorCallback(c.Error);
                    });
        }

        void SetupUI()
        {
            if (!ValidateAssetBundleBlueprintID(apiAvatar.id))
            {
                blueprintPanel.SetActive(false);
                errorPanel.SetActive(true);
                OnSDKPipelineError("The asset bundle is out of date.  Please rebuild the scene using 'New Build'.", "The blueprint ID in the scene does not match the id in the asset bundle.");
                return;
            }

            if (APIUser.Exists(pipelineManager.user))
            {
                waitingPanel.SetActive(false);
                blueprintPanel.SetActive(true);
                errorPanel.SetActive(false);

                if (isUpdate)
                {
                    // bp update
                    if (apiAvatar.authorId == pipelineManager.user.id)
                    {
                        titleText.text = "Update Avatar";
                        // apiAvatar = pipelineManager.user.GetBlueprint(pipelineManager.blueprintId) as ApiAvatar;
                        blueprintName.text = apiAvatar.name;
                        contentSex.isOn = apiAvatar.tags.Contains("content_sex");
                        contentViolence.isOn = apiAvatar.tags.Contains("content_violence");
                        contentGore.isOn = apiAvatar.tags.Contains("content_gore");
                        contentOther.isOn = apiAvatar.tags.Contains("content_other");
                        developerAvatar.isOn = apiAvatar.tags.Contains("developer");
                        sharePrivate.isOn = apiAvatar.releaseStatus.Contains("private");
                        sharePublic.isOn = apiAvatar.releaseStatus.Contains("public");

                        tagFallback.isOn = apiAvatar.tags.Contains("author_quest_fallback");
                        tagFallback.transform.parent.gameObject.SetActive(true);

                        switch (pipelineManager.fallbackStatus)
                        {
                            case PipelineManager.FallbackStatus.Valid:
#if UNITY_ANDROID
                                tagFallback.interactable = true;
                                tagFallback.GetComponentInChildren<Text>().text = "Use for Fallback";
#else
                                tagFallback.interactable = false;
                                tagFallback.GetComponentInChildren<Text>().text = "Use for Fallback (change only with Android upload)";
#endif
                                break;
                            case PipelineManager.FallbackStatus.InvalidPerformance:
                            case PipelineManager.FallbackStatus.InvalidRig:
                                tagFallback.isOn = false; // need to remove tag on this upload, the updated version is not up-to-spec
                                tagFallback.interactable = false;
                                tagFallback.GetComponentInChildren<Text>().text = "Use for Fallback (avatar not valid, tag will be cleared)";
                                break;
                        }

                        blueprintDescription.text = apiAvatar.description;
                        shouldUpdateImageToggle.interactable = true;
                        shouldUpdateImageToggle.isOn = false;
                        liveBpImage.enabled = false;
                        bpImage.enabled = true;

                        ImageDownloader.DownloadImage(apiAvatar.imageUrl, 0, (Texture2D obj) => bpImage.texture = obj, null);
                    }
                    else // user does not own apiAvatar id associated with descriptor
                    {
                        Debug.LogErrorFormat("{0} is not an owner of {1}", apiAvatar.authorId, pipelineManager.user.id);
                        blueprintPanel.SetActive(false);
                        errorPanel.SetActive(true);
                    }
                }
                else
                {
                    titleText.text = "New Avatar";
                    shouldUpdateImageToggle.interactable = false;
                    shouldUpdateImageToggle.isOn = true;
                    liveBpImage.enabled = true;
                    bpImage.enabled = false;
                    tagFallback.isOn = false;
                    
                    // Janky fix for an avatar's blueprint image not showing up the very first time you press publish in a project until you resize the window
                    // can remove if we fix the underlying issue or move publishing out of Play Mode
                    string firstTimeResize = $"{Application.identifier}-firstTimeResize";
                    if (!PlayerPrefs.HasKey(firstTimeResize))
                    {
                        GameViewMethods.ResizeGameView();
                        PlayerPrefs.SetInt(firstTimeResize, 1);
                    }

                    tagFallback.transform.parent.gameObject.SetActive(true);
                    switch (pipelineManager.fallbackStatus)
                    {
                        case PipelineManager.FallbackStatus.Valid:
#if UNITY_ANDROID
                            tagFallback.interactable = true;
                            tagFallback.GetComponentInChildren<Text>().text = "Use for Fallback";
#else
                            tagFallback.interactable = false;
                            tagFallback.GetComponentInChildren<Text>().text = "Use for Fallback (change only with Android upload)";
#endif
                            break;
                        case PipelineManager.FallbackStatus.InvalidPerformance:
                        case PipelineManager.FallbackStatus.InvalidRig:
                            tagFallback.transform.parent.gameObject.SetActive(true);
                            tagFallback.interactable = false;
                            tagFallback.GetComponentInChildren<Text>().text = "Use for Fallback (avatar not valid, tag will be cleared)";
                            break;
                    }
                }
            }
            else
            {
                waitingPanel.SetActive(true);
                blueprintPanel.SetActive(false);
                errorPanel.SetActive(false);
            }

            if (APIUser.CurrentUser != null && APIUser.CurrentUser.hasSuperPowers)
                developerAvatar.gameObject.SetActive(true);
            else
                developerAvatar.gameObject.SetActive(false);
        }

        public void SetupUpload()
        {
            uploadTitle = "Preparing For Upload";
            isUploading = true;

            string abPath = UnityEditor.EditorPrefs.GetString("currentBuildingAssetBundlePath");

            string unityPackagePath = UnityEditor.EditorPrefs.GetString("VRC_exportedUnityPackagePath");

            UnityEditor.EditorPrefs.SetBool("VRCSDK2_scene_changed", true);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_sex", contentSex.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_violence", contentViolence.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_gore", contentGore.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_other", contentOther.isOn);

            if (string.IsNullOrEmpty(apiAvatar.id))
            {
                pipelineManager.AssignId();
                apiAvatar.id = pipelineManager.blueprintId;
            }

            string avatarId = apiAvatar.id;
            int version = isUpdate ? apiAvatar.version + 1 : 1;
            PrepareVRCPathForS3(abPath, avatarId, version, ApiAvatar.VERSION);

            if (!string.IsNullOrEmpty(unityPackagePath) && System.IO.File.Exists(unityPackagePath))
            {
                VRC.Core.Logger.Log("Found unity package path. Preparing to upload!", DebugLevel.All);
                PrepareUnityPackageForS3(unityPackagePath, avatarId, version, ApiAvatar.VERSION);
            }

            StartCoroutine(UploadNew());
        }

        IEnumerator UploadNew()
        {
            bool caughtInvalidInput = false;
            if (!ValidateNameInput(blueprintName))
                caughtInvalidInput = true;

            if (caughtInvalidInput)
                yield break;

            VRC.Core.Logger.Log("Starting upload", DebugLevel.Always);

            // upload unity package
            if (!string.IsNullOrEmpty(uploadUnityPackagePath))
            {
                yield return StartCoroutine(UploadFile(uploadUnityPackagePath, isUpdate ? apiAvatar.unityPackageUrl : "", GetFriendlyAvatarFileName("Unity package"), "Unity package",
                    delegate (string fileUrl)
                    {
                        cloudFrontUnityPackageUrl = fileUrl;
                    }
                ));
            }

            // upload asset bundle
            if (!string.IsNullOrEmpty(uploadVrcPath))
            {
                yield return StartCoroutine(UploadFile(uploadVrcPath, isUpdate ? apiAvatar.assetUrl : "", GetFriendlyAvatarFileName("Asset bundle"), "Asset bundle",
                    delegate (string fileUrl)
                    {
                        cloudFrontAssetUrl = fileUrl;
                    }
                ));
            }

            if (isUpdate)
                yield return StartCoroutine(UpdateBlueprint());
            else
                yield return StartCoroutine(CreateBlueprint());

            OnSDKPipelineComplete();
        }

        private string GetFriendlyAvatarFileName(string type)
        {
            return "Avatar - " + blueprintName.text + " - " + type + " - " + Application.unityVersion + "_" + ApiWorld.VERSION.ApiVersion +
                   "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
        }

        List<string> BuildTags()
        {
            var tags = new List<string>();
            if (contentSex.isOn)
                tags.Add("content_sex");
            if (contentViolence.isOn)
                tags.Add("content_violence");
            if (contentGore.isOn)
                tags.Add("content_gore");
            if (contentOther.isOn)
                tags.Add("content_other");

            if (APIUser.CurrentUser.hasSuperPowers)
            {
                if (developerAvatar.isOn)
                    tags.Add("developer");
            }

            if (tagFallback.isOn)
                tags.Add("author_quest_fallback");

            return tags;
        }

        protected override IEnumerator CreateBlueprint()
        {
            yield return StartCoroutine(UpdateImage(isUpdate ? apiAvatar.imageUrl : "", GetFriendlyAvatarFileName("Image")));

            ApiAvatar avatar = new ApiAvatar
            {
                id = pipelineManager.blueprintId,
                authorName = pipelineManager.user.displayName,
                authorId = pipelineManager.user.id,
                name = blueprintName.text,
                imageUrl = cloudFrontImageUrl,
                assetUrl = cloudFrontAssetUrl,
                description = blueprintDescription.text,
                tags = BuildTags(),
                unityPackageUrl = cloudFrontUnityPackageUrl,
                releaseStatus = sharePublic.isOn ? "public" : "private"
            };

            bool doneUploading = false;
            bool wasError = false;

            avatar.Post(
                (c) =>
                {
                    ApiAvatar savedBP = (ApiAvatar)c.Model;
                    pipelineManager.blueprintId = savedBP.id;
                    UnityEditor.EditorPrefs.SetString("blueprintID-" + pipelineManager.GetInstanceID().ToString(), savedBP.id);

                    AnalyticsSDK.AvatarUploaded(savedBP, false);
                    doneUploading = true;
                },
                (c) =>
                {
                    Debug.LogError(c.Error);
                    SetUploadProgress("Saving Avatar", "Error saving blueprint.", 0.0f);
                    doneUploading = true;
                    wasError = true;
                });

            while (!doneUploading)
                yield return null;

            if (wasError)
                yield return new WaitUntil(() => UnityEditor.EditorUtility.DisplayDialog("VRChat SDK", "Error saving blueprint.", "Okay"));
        }

        protected override IEnumerator UpdateBlueprint()
        {
            bool doneUploading = false;

            apiAvatar.name = blueprintName.text;
            apiAvatar.description = blueprintDescription.text;
            apiAvatar.assetUrl = cloudFrontAssetUrl;
            apiAvatar.releaseStatus = sharePublic.isOn ? "public" : "private";
            apiAvatar.tags = BuildTags();
            apiAvatar.unityPackageUrl = cloudFrontUnityPackageUrl;

            if (shouldUpdateImageToggle.isOn)
            {
                yield return StartCoroutine(UpdateImage(isUpdate ? apiAvatar.imageUrl : "", GetFriendlyAvatarFileName("Image")));
                apiAvatar.imageUrl = cloudFrontImageUrl;
            }

            SetUploadProgress("Saving Avatar", "Almost finished!!", 0.8f);
            apiAvatar.Save(
                    (c) => { AnalyticsSDK.AvatarUploaded(apiAvatar, true); doneUploading = true; },
                    (c) => {
                        Debug.LogError(c.Error);
                        SetUploadProgress("Saving Avatar", "Error saving blueprint.", 0.0f);
                        doneUploading = true;
                    });

            while (!doneUploading)
                yield return null;
        }

        void ToggleUpdateImage(bool isOn)
        {
            if (isOn)
            {
                bpImage.enabled = false;
                liveBpImage.enabled = true;
            }
            else
            {
                bpImage.enabled = true;
                liveBpImage.enabled = false;
                ImageDownloader.DownloadImage(apiAvatar.imageUrl, 0, obj => bpImage.texture = obj, null);
            }
        }

        void OnDestroy()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
            UnityEditor.EditorPrefs.DeleteKey("currentBuildingAssetBundlePath");
        }
    }
#endif
}


