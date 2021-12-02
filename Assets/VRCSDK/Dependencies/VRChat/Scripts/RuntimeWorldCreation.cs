#define COMMUNITY_LABS_SDK
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using VRC.Core;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRCSDK2
{
#if UNITY_EDITOR
    public class RuntimeWorldCreation : RuntimeAPICreation
    {
        public GameObject waitingPanel;
        public GameObject blueprintPanel;
        public GameObject errorPanel;

        public Text titleText;
        public InputField blueprintName;
        public InputField blueprintDescription;
        public InputField worldCapacity;
        public RawImage bpImage;
        public Image liveBpImage;
        public Toggle shouldUpdateImageToggle;
        public Toggle releasePublic;
        public Toggle contentNsfw;

        public Toggle contentSex;
        public Toggle contentViolence;
        public Toggle contentGore;
        public Toggle contentOther;

        public Toggle contentFeatured;
        public Toggle contentSDKExample;

        public Image showInWorldsMenuGroup;
        public Toggle showInActiveWorlds;
        public Toggle showInPopularWorlds;
        public Toggle showInNewWorlds;

        public InputField userTags;

        public UnityEngine.UI.Button uploadButton;

        public UnityEngine.UI.Button openCommunityLabsDocsButton;

        public GameObject publishToCommunityLabsPanel;

        private Toggle publishToCommLabsToggle;

        private ApiWorld worldRecord;

        private const int MAX_USER_TAGS_FOR_WORLD = 5;
        private const int MAX_CHARACTERS_ALLOWED_IN_USER_TAG = 20;
        List<String> customTags;

        public static bool IsCurrentWorldInCommunityLabs = false;
        public static bool IsCurrentWorldUploaded = false;
        public static bool IsCurrentWorldPubliclyPublished = false;
        public static bool HasExceededPublishLimit = false;

        new void Start()
        {
            if (!Application.isEditor || !Application.isPlaying)
                return;

            base.Start();

            IsCurrentWorldInCommunityLabs = false;
            IsCurrentWorldUploaded = false;
            IsCurrentWorldPubliclyPublished = false;


            var desc = pipelineManager.GetComponent<VRC.SDKBase.VRC_SceneDescriptor>();
            desc.PositionPortraitCamera(imageCapture.shotCamera.transform);

            Application.runInBackground = true;
            UnityEngine.XR.XRSettings.enabled = false;

            uploadButton.onClick.AddListener(SetupUpload);

            openCommunityLabsDocsButton.onClick.AddListener(OpenCommunityLabsDocumentation);

            shouldUpdateImageToggle.onValueChanged.AddListener(ToggleUpdateImage);

            releasePublic.gameObject.SetActive(false);

            System.Action<string> onError = (err) => {
                VRC.Core.Logger.LogError("Could not authenticate - " + err, DebugLevel.Always);
                blueprintPanel.SetActive(false);
                errorPanel.SetActive(true);
            };

            if (!ApiCredentials.Load())
                onError("Not logged in");
            else
                APIUser.InitialFetchCurrentUser(
                    delegate (ApiModelContainer<APIUser> c)
                    {
                        UserLoggedInCallback(c.Model as APIUser);
                    },
                    delegate (ApiModelContainer<APIUser> c)
                    {
                        onError(c.Error);
                    }
                );

#if !COMMUNITY_LABS_SDK
            publishToCommunityLabsPanel.gameObject.SetActive(false);
#endif
        }

        void UserLoggedInCallback(APIUser user)
        {
            pipelineManager.user = user;

            ApiWorld model = new ApiWorld();
            model.id = pipelineManager.blueprintId;
            model.Fetch(null,
                (c) =>
                {
                    VRC.Core.Logger.Log("<color=magenta>Updating an existing world.</color>", DebugLevel.All);
                    worldRecord = c.Model as ApiWorld;
                    pipelineManager.completedSDKPipeline = !string.IsNullOrEmpty(worldRecord.authorId);
                    GetUserUploadInformationAndSetupUI(model.id);
                },
                (c) =>
                {
                    VRC.Core.Logger.Log("<color=magenta>World record not found, creating a new world.</color>", DebugLevel.All);
                    worldRecord = new ApiWorld { capacity = 16 };
                    pipelineManager.completedSDKPipeline = false;
                    worldRecord.id = pipelineManager.blueprintId;
                    GetUserUploadInformationAndSetupUI(model.id);
                });
        }

        void CheckWorldStatus(string worldId, Action onCheckComplete)
        {
            // check if world has been previously uploaded, and if world is in community labs
            ApiWorld.FetchUploadedWorlds(
                delegate (IEnumerable<ApiWorld> worlds)
                {
                    ApiWorld selectedWorld = worlds.FirstOrDefault(w => w.id == worldId);
                    if (null!=selectedWorld)
                    {
                        IsCurrentWorldInCommunityLabs = selectedWorld.IsCommunityLabsWorld;
                        IsCurrentWorldPubliclyPublished = selectedWorld.IsPublicPublishedWorld;
                        IsCurrentWorldUploaded = true;
                    }
                    if (onCheckComplete != null) onCheckComplete();

                },
                delegate (string err)
                {
                    IsCurrentWorldInCommunityLabs = false;
                    IsCurrentWorldUploaded = false;
                    IsCurrentWorldPubliclyPublished = false;
                    Debug.Log("CheckWorldStatus error:" + err);
                    if (onCheckComplete != null) onCheckComplete();
                }
                );
        }


        void GetUserUploadInformationAndSetupUI(string worldId)
        {
            CheckWorldStatus(worldId, delegate()
                {
                    bool hasSufficientTrustLevelToPublishToCommunityLabs = APIUser.CurrentUser.hasKnownTrustLevel;
                    APIUser.FetchPublishWorldsInformation(
                        (c) =>
                        {
                            try
                            {
                                Dictionary<string, object> publish = c as Dictionary<string, object>;
                                if (publish["canPublish"] is bool)
                                {
                                    HasExceededPublishLimit = !(bool)(publish["canPublish"]);
                                }
                                else
                                    HasExceededPublishLimit = true;
                            }
                            catch (Exception)
                            {
                                HasExceededPublishLimit = true;
                            }

                            if(Application.isPlaying)
                            {
                                SetupUI(hasSufficientTrustLevelToPublishToCommunityLabs, HasExceededPublishLimit);
                            }
                        },
                        (c) =>
                        {
                            if(Application.isPlaying)
                            {
                                SetupUI(hasSufficientTrustLevelToPublishToCommunityLabs, HasExceededPublishLimit);
                            }
                        }
                    );
                }
            );
        }

        void SetupUI(bool hasEnoughTrustToPublishToCL = false, bool hasExceededWeeklyPublishLimit = false)
        {
#if COMMUNITY_LABS_SDK
            // do not display community labs panel if updating an existing CL world or updating a public world
            publishToCommunityLabsPanel.gameObject.SetActive(!IsCurrentWorldUploaded);
#endif

            if (!ValidateAssetBundleBlueprintID(worldRecord.id))
            {
                blueprintPanel.SetActive(false);
                errorPanel.SetActive(true);
                OnSDKPipelineError("The asset bundle is out of date.  Please rebuild the scene using 'New Build'.", "The blueprint ID in the scene does not match the id in the asset bundle.");
                return;
            }

            contentFeatured.gameObject.SetActive(APIUser.CurrentUser.hasSuperPowers);
            contentSDKExample.gameObject.SetActive(APIUser.CurrentUser.hasSuperPowers);

            if (APIUser.Exists(pipelineManager.user))
            {
                waitingPanel.SetActive(false);
                blueprintPanel.SetActive(true);
                errorPanel.SetActive(false);

                if (string.IsNullOrEmpty(worldRecord.authorId) || worldRecord.authorId == pipelineManager.user.id)
                {
                    titleText.text = "Configure World";
                    blueprintName.text = worldRecord.name;
                    worldCapacity.text = worldRecord.capacity.ToString();
                    contentSex.isOn = worldRecord.tags.Contains("content_sex");
                    contentViolence.isOn = worldRecord.tags.Contains("content_violence");
                    contentGore.isOn = worldRecord.tags.Contains("content_gore");
                    contentOther.isOn = worldRecord.tags.Contains("content_other");
                    shouldUpdateImageToggle.interactable = isUpdate;
                    shouldUpdateImageToggle.isOn = !isUpdate;
                    liveBpImage.enabled = !isUpdate;
                    bpImage.enabled = isUpdate;

                    if (!APIUser.CurrentUser.hasSuperPowers)
                    {
                        releasePublic.gameObject.SetActive(false);
                        releasePublic.isOn = false;
                        releasePublic.interactable = false;

                        contentFeatured.isOn = contentSDKExample.isOn = false;
                    }
                    else
                    {
                        contentFeatured.isOn = worldRecord.tags.Contains("content_featured");
                        contentSDKExample.isOn = worldRecord.tags.Contains("content_sdk_example");

                        releasePublic.isOn = worldRecord.releaseStatus == "public";
                        releasePublic.gameObject.SetActive(true);
                    }

                    // "show in worlds menu"
                    if (APIUser.CurrentUser.hasSuperPowers)
                    {
                        showInWorldsMenuGroup.gameObject.SetActive(true);
                        showInActiveWorlds.isOn = !worldRecord.tags.Contains("admin_hide_active");
                        showInPopularWorlds.isOn = !worldRecord.tags.Contains("admin_hide_popular");
                        showInNewWorlds.isOn = !worldRecord.tags.Contains("admin_hide_new");
                    }
                    else
                    {
                        showInWorldsMenuGroup.gameObject.SetActive(false);
                    }

                    blueprintDescription.text = worldRecord.description;

                    userTags.text = "";
                    foreach (var tag in worldRecord.publicTags)
                    {
                        userTags.text = userTags.text + tag.Replace("author_tag_", "");
                        userTags.text = userTags.text + " ";
                    }

                    ImageDownloader.DownloadImage(worldRecord.imageUrl, 0, obj => bpImage.texture = obj, null);
                }
                else // user does not own world id associated with descriptor
                {
                    Debug.LogErrorFormat("{0} is not an owner of {1}", worldRecord.authorId, pipelineManager.user.id);
                    blueprintPanel.SetActive(false);
                    errorPanel.SetActive(true);
                }
            }
            else
            {
                waitingPanel.SetActive(true);
                blueprintPanel.SetActive(false);
                errorPanel.SetActive(false);

                if (!APIUser.CurrentUser.hasSuperPowers)
                {
                    releasePublic.gameObject.SetActive(false);
                    releasePublic.isOn = false;
                    releasePublic.interactable = false;
                }
                else
                {
                    releasePublic.gameObject.SetActive(true);
                    releasePublic.isOn = false;
                }
            }

            // set up publish to Community Labs checkbox and text
            int worldsPublishedThisWeek = hasExceededWeeklyPublishLimit ? 1 : 0;
            int maximumWorldsAllowedToPublishPerWeek = 1;
            publishToCommLabsToggle = publishToCommunityLabsPanel.GetComponentInChildren<Toggle>();

            if (null != publishToCommLabsToggle)
            {
                // disable publishing to CL checkbox if not enough trust or exceeded publish limit 
                publishToCommLabsToggle.interactable = hasEnoughTrustToPublishToCL && !hasExceededWeeklyPublishLimit;

                Text publishText = publishToCommLabsToggle.gameObject.GetComponentInChildren<Text>();
                if (null != publishText)
                {
                    if (!hasEnoughTrustToPublishToCL)
                    {
                        publishText.text = "Not enough Trust to Publish to Community Labs";
                    }
                    else
                    {
                        if (hasExceededWeeklyPublishLimit)
                        {
                            publishText.text = "Publish limit for Community Labs Exceeded\n" + "(" + worldsPublishedThisWeek + "/" + maximumWorldsAllowedToPublishPerWeek + " Published this week)";
                        }
                        else
                        {
                            publishText.text = "Publish to Community Labs\n" + "(" + worldsPublishedThisWeek + "/" + maximumWorldsAllowedToPublishPerWeek + " Published this week)";
                        }
                    }
                }
            }
        }

        public void SetupUpload()
        {
            if (!ParseUserTags())
                return;

            publishingToCommunityLabs = (publishToCommLabsToggle != null) && (publishToCommLabsToggle.isActiveAndEnabled) && (publishToCommLabsToggle.isOn);

            uploadTitle = "Preparing For Upload";
            isUploading = true;

            string abPath = UnityEditor.EditorPrefs.GetString("currentBuildingAssetBundlePath");


            string unityPackagePath = UnityEditor.EditorPrefs.GetString("VRC_exportedUnityPackagePath");

            UnityEditor.EditorPrefs.SetBool("VRCSDK2_scene_changed", true);
            UnityEditor.EditorPrefs.SetInt("VRCSDK2_capacity", System.Convert.ToInt16(worldCapacity.text));
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_sex", contentSex.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_violence", contentViolence.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_gore", contentGore.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_other", contentOther.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_release_public", releasePublic.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_featured", contentFeatured.isOn);
            UnityEditor.EditorPrefs.SetBool("VRCSDK2_content_sdk_example", contentSDKExample.isOn);

            if (string.IsNullOrEmpty(worldRecord.id))
            {
                pipelineManager.AssignId();
                worldRecord.id = pipelineManager.blueprintId;
            }

            string blueprintId = worldRecord.id;
            int version = Mathf.Max(1, worldRecord.version + 1);
            PrepareVRCPathForS3(abPath, blueprintId, version, ApiWorld.VERSION);

            if (!string.IsNullOrEmpty(unityPackagePath) && System.IO.File.Exists(unityPackagePath))
            {
                VRC.Core.Logger.Log("Found unity package path. Preparing to upload!", DebugLevel.All);
                PrepareUnityPackageForS3(unityPackagePath, blueprintId, version, ApiWorld.VERSION);
            }

            StartCoroutine(UploadNew());
        }

        void OnUploadedWorld()
        {
            const string devUrl = "https://dev-api.vrchat.cloud";
            const string releaseUrl = "https://vrchat.com";
            
            string uploadedWorldURL = (API.IsDevApi() ? devUrl : releaseUrl) + "/home/world/" + pipelineManager.blueprintId;
            OnSDKPipelineComplete(uploadedWorldURL);
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
                yield return StartCoroutine(UploadFile(uploadUnityPackagePath, isUpdate ? worldRecord.unityPackageUrl : "", GetFriendlyWorldFileName("Unity package"), "Unity package",
                    delegate (string fileUrl)
                    {
                        cloudFrontUnityPackageUrl = fileUrl;
                    }
                ));
            }

            // upload asset bundle
            if (!string.IsNullOrEmpty(uploadVrcPath))
            {
                yield return StartCoroutine(UploadFile(uploadVrcPath, isUpdate ? worldRecord.assetUrl : "", GetFriendlyWorldFileName("Asset bundle"), "Asset bundle",
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

            if (publishingToCommunityLabs)
            {
                ApiWorld.PublishWorldToCommunityLabs(pipelineManager.blueprintId,
                    (world) => OnUploadedWorld(),
                    (err) =>
                    {
                        Debug.LogError("PublishWorldToCommunityLabs error:" + err);
                        OnUploadedWorld();
                    }
                );
            }
            else
            {
                OnUploadedWorld();
            }
        }

        private string GetFriendlyWorldFileName(string type)
        {
            return "World - " + blueprintName.text + " - " + type + " - " + Application.unityVersion + "_" + ApiWorld.VERSION.ApiVersion +
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
                if (contentFeatured.isOn)
                    tags.Add("content_featured");
                if (contentSDKExample.isOn)
                    tags.Add("content_sdk_example");
                if(releasePublic.isOn)
                    tags.Add("admin_approved");
            }

            // "show in worlds menu"
            if (APIUser.CurrentUser.hasSuperPowers)
            {
                if (!showInActiveWorlds.isOn)
                    tags.Add("admin_hide_active");
                if (!showInPopularWorlds.isOn)
                    tags.Add("admin_hide_popular");
                if (!showInNewWorlds.isOn)
                    tags.Add("admin_hide_new");
            }

            // add any author tags
            foreach (var word in customTags)
            {
                // add all custom tags with "author_tag_" prefix
                tags.Add("author_tag_" + word);
            }

            return tags;
        }

        bool ParseUserTags()
        {
            bool validTags = true;
            customTags = new List<string>();
            char[] delimiterChars = { ' ', ',', '.', ':', '\t', '\n', '"', '#' };

            // split user tags into individual words
            string[] words = userTags.text.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                customTags.Add(word.ToLower());
            }

            // check that number of tags is within tag limit
            if (words.Count() > MAX_USER_TAGS_FOR_WORLD)
            {
                validTags = false;
                UnityEditor.EditorUtility.DisplayDialog("Tags are limited to a maximum of " + MAX_USER_TAGS_FOR_WORLD + " per world.", "Please remove excess tags before uploading!", "OK");
            }
            else
            {
                // check that no tags exceed maximum tag length
                int maximumTagLength = 0;
                foreach (string item in words)
                {
                    if (item.Length > maximumTagLength)
                    {
                        maximumTagLength = item.Length;
                    }
                }

                if (maximumTagLength > MAX_CHARACTERS_ALLOWED_IN_USER_TAG)
                {
                    validTags = false;
                    UnityEditor.EditorUtility.DisplayDialog("Tags are limited to a maximum of " + MAX_CHARACTERS_ALLOWED_IN_USER_TAG + " characters per tag.", "One or more of your tags exceeds the maximum " + MAX_CHARACTERS_ALLOWED_IN_USER_TAG + " character limit.\n\n" + "Please shorten tags before uploading!", "OK");
                }
                else
                {
                    // make sure tags are all alphanumeric
                    foreach (var word in words)
                    {
                        if (!word.All(char.IsLetterOrDigit))
                        {
                            validTags = false;
                            UnityEditor.EditorUtility.DisplayDialog("Tags should consist of alphanumeric characters only.", "Please remove any non-alphanumeric characters from tags before uploading!", "OK");
                        }
                    }
                }
            }

            return validTags;
        }

        protected override IEnumerator CreateBlueprint()
        {
            yield return StartCoroutine(UpdateImage(isUpdate ? worldRecord.imageUrl : "", GetFriendlyWorldFileName("Image")));

            SetUploadProgress("Saving Blueprint to user", "Almost finished!!", 0.0f);
            ApiWorld world = new ApiWorld
            {
                id = worldRecord.id,
                authorName = pipelineManager.user.displayName,
                authorId = pipelineManager.user.id,
                name = blueprintName.text,
                imageUrl = cloudFrontImageUrl,
                assetUrl = cloudFrontAssetUrl,
                unityPackageUrl = cloudFrontUnityPackageUrl,
                description = blueprintDescription.text,
                tags = BuildTags(),
                releaseStatus = (releasePublic.isOn) ? ("public") : ("private"),
                capacity = System.Convert.ToInt16(worldCapacity.text),
                occupants = 0,
                shouldAddToAuthor = true
            };

            if (APIUser.CurrentUser.hasSuperPowers)
                world.isCurated = contentFeatured.isOn || contentSDKExample.isOn;
            else
                world.isCurated = false;

            bool doneUploading = false;
            world.Post(
                (c) =>
                {
                    ApiWorld savedBP = (ApiWorld)c.Model;
                    pipelineManager.blueprintId = savedBP.id;
                    UnityEditor.EditorPrefs.SetString("blueprintID-" + pipelineManager.GetInstanceID().ToString(), savedBP.id);
                    VRC.Core.Logger.Log("Setting blueprintID on pipeline manager and editor prefs", DebugLevel.All);
                    doneUploading = true;
                },
                (c) => { doneUploading = true; Debug.LogError(c.Error); });

            while (!doneUploading)
                yield return null;
        }

        protected override IEnumerator UpdateBlueprint()
        {
            bool doneUploading = false;

            worldRecord.name = blueprintName.text;
            worldRecord.description = blueprintDescription.text;
            worldRecord.capacity = System.Convert.ToInt16(worldCapacity.text);
            worldRecord.assetUrl = cloudFrontAssetUrl;
            worldRecord.tags = BuildTags();
            worldRecord.releaseStatus = (releasePublic.isOn) ? ("public") : ("private");
            worldRecord.unityPackageUrl = cloudFrontUnityPackageUrl;
            worldRecord.isCurated = contentFeatured.isOn || contentSDKExample.isOn;

            if (shouldUpdateImageToggle.isOn)
            {
                yield return StartCoroutine(UpdateImage(isUpdate ? worldRecord.imageUrl : "", GetFriendlyWorldFileName("Image")));

                worldRecord.imageUrl = cloudFrontImageUrl;
            }

            SetUploadProgress("Saving Blueprint", "Almost finished!!", 0.0f);
            worldRecord.Save((c) => doneUploading = true, (c) => { doneUploading = true; Debug.LogError(c.Error); });

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
                ImageDownloader.DownloadImage(worldRecord.imageUrl, 0, obj => bpImage.texture = obj, null);
            }
        }

        protected override void DisplayUpdateCompletedDialog(string contentUrl = null)
        {
#if UNITY_EDITOR
#if COMMUNITY_LABS_SDK
            if (null != contentUrl)
            {
                CheckWorldStatus(pipelineManager.blueprintId, delegate ()
                {
                    ContentUploadedDialog window = (ContentUploadedDialog)EditorWindow.GetWindow(typeof(ContentUploadedDialog), true, "VRCSDK", true);
                    window.setContentURL(contentUrl);
                    window.Show();
                    // refresh UI based on uploaded world
                    GetUserUploadInformationAndSetupUI(pipelineManager.blueprintId);
                }
                );
            }
            else
#endif
                base.DisplayUpdateCompletedDialog(contentUrl);
#endif
        }

        private void OpenCommunityLabsDocumentation()
        {
            Application.OpenURL(CommunityLabsConstants.COMMUNITY_LABS_DOCUMENTATION_URL);
        }

        void OnDestroy()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
            UnityEditor.EditorPrefs.DeleteKey("currentBuildingAssetBundlePath");
        }
    }
#endif
}


