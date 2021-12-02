using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
using Object = UnityEngine.Object;

namespace VRC.SDKBase.Editor
{
    public class VRCSdkControlPanelAvatarBuilder : IVRCSdkControlPanelBuilder
    {
        protected VRCSdkControlPanel _builder;
        private VRC_AvatarDescriptor[] _avatars;
        private static VRC_AvatarDescriptor _selectedAvatar;
        private Vector2 _avatarListScrollPos;
        private Vector2 _scrollPos;


        protected const int MAX_ACTION_TEXTURE_SIZE = 256;

        private bool showAvatarPerformanceDetails
        {
            get => EditorPrefs.GetBool("VRC.SDKBase_showAvatarPerformanceDetails", false);
            set => EditorPrefs.SetBool("VRC.SDKBase_showAvatarPerformanceDetails",
                value); //Do we ever actually set this?
        }

        private static PropertyInfo _legacyBlendShapeNormalsPropertyInfo;

        private static PropertyInfo LegacyBlendShapeNormalsPropertyInfo
        {
            get
            {
                if (_legacyBlendShapeNormalsPropertyInfo != null)
                {
                    return _legacyBlendShapeNormalsPropertyInfo;
                }

                Type modelImporterType = typeof(ModelImporter);
                _legacyBlendShapeNormalsPropertyInfo = modelImporterType.GetProperty(
                    "legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );

                return _legacyBlendShapeNormalsPropertyInfo;
            }
        }

        public void ShowSettingsOptions()
        {
            EditorGUILayout.BeginVertical(VRCSdkControlPanel.boxGuiStyle);
            GUILayout.Label("Avatar Options", EditorStyles.boldLabel);
            bool prevShowPerfDetails = showAvatarPerformanceDetails;
            bool showPerfDetails =
                EditorGUILayout.ToggleLeft("Show All Avatar Performance Details", prevShowPerfDetails);
            if (showPerfDetails != prevShowPerfDetails)
            {
                showAvatarPerformanceDetails = showPerfDetails;
                _builder.ResetIssues();
            }

            EditorGUILayout.EndVertical();
        }

        public bool IsValidBuilder(out string message)
        {
            FindAvatars();
            message = null;
            if (_avatars != null && _avatars.Length > 0) return true;
#if VRC_SDK_VRCSDK2
            message = "A VRC_SceneDescriptor or VRC_AvatarDescriptor\nis required to build VRChat SDK Content";
#elif VRC_SDK_VRCSDK3
            message = "A VRCSceneDescriptor or VRCAvatarDescriptor\nis required to build VRChat SDK Content";
#endif
            return false;
        }

        public virtual void ShowBuilder()
        {
            if (_avatars.Length > 0)
            {
                if (!_builder.CheckedForIssues)
                {
                    _builder.ResetIssues();
                    foreach (VRC_AvatarDescriptor t in _avatars)
                        OnGUIAvatarCheck(t);
                    _builder.CheckedForIssues = true;
                }

                bool drawList = true;
                if (_avatars.Length == 1)
                {
                    drawList = false;
                    _selectedAvatar = _avatars[0];
                }

                if (drawList)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"),
                                                  GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth),
                                                  GUILayout.MaxHeight(150));
                    _avatarListScrollPos = EditorGUILayout.BeginScrollView(_avatarListScrollPos, false, false);

                    for (int i = 0; i < _avatars.Length; ++i)
                    {
                        VRC_AvatarDescriptor av = _avatars[i];
                        EditorGUILayout.Space();
                        if (_selectedAvatar == av)
                        {
                            if (GUILayout.Button(av.gameObject.name,
                                                 VRCSdkControlPanel.listButtonStyleSelected,
                                                 GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth - 50)))
                                _selectedAvatar = null;
                        }
                        else
                        {
                            if (GUILayout.Button(av.gameObject.name,
                                        ((i & 0x01) > 0)
                                        ? (VRCSdkControlPanel.listButtonStyleOdd)
                                        : (VRCSdkControlPanel.listButtonStyleEven),
                                        GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth - 50)))
                                _selectedAvatar = av;
                        }
                    }

                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginVertical(GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth));
                _builder.OnGUIShowIssues();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();

                if (_selectedAvatar != null)
                {
                    EditorGUILayout.BeginVertical(VRCSdkControlPanel.boxGuiStyle);
                    OnGUIAvatarSettings(_selectedAvatar);
                    EditorGUILayout.EndVertical();

                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
                                                                 false,
                                                                 false,
                                                                 GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth));
                    _builder.OnGUIShowIssues(_selectedAvatar);
                    EditorGUILayout.EndScrollView();

                    GUILayout.FlexibleSpace();

                    OnGUIAvatar(_selectedAvatar);
                }
            }
            else
            {
                EditorGUILayout.Space();
                if (UnityEditor.BuildPipeline.isBuildingPlayer)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Building – Please Wait ...",
                                               VRCSdkControlPanel.titleGuiStyle,
                                               GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth));
                }
                else
                {
#if VRC_SDK_VRCSDK2
                    EditorGUILayout.LabelField(
                            "A VRC_SceneDescriptor or VRC_AvatarDescriptor\nis required to build VRChat SDK Content",
                            VRCSdkControlPanel.titleGuiStyle, GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth));
#elif VRC_SDK_VRCSDK3
                    EditorGUILayout.LabelField(
                            "A VRCSceneDescriptor or VRCAvatarDescriptor\nis required to build VRChat SDK Content",
                            VRCSdkControlPanel.titleGuiStyle, GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth));
#else
                    EditorGUILayout.LabelField("A SceneDescriptor or AvatarDescriptor\nis required to build VRChat SDK Content", VRCSdkControlPanel.titleGuiStyle, GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth));
#endif
                }
            }
        }

        public void RegisterBuilder(VRCSdkControlPanel baseBuilder)
        {
            _builder = baseBuilder;
        }

        public void SelectAllComponents()
        {
            List<Object> show = new List<Object>(Selection.objects);
            foreach (VRC_AvatarDescriptor a in _avatars)
                show.Add(a.gameObject);
            Selection.objects = show.ToArray();
        }

        private void FindAvatars()
        {
            List<VRC_AvatarDescriptor> allAvatars = Tools.FindSceneObjectsOfTypeAll<VRC_AvatarDescriptor>().ToList();
            // Select only the active avatars
            VRC_AvatarDescriptor[] newAvatars =
                allAvatars.Where(av => null != av && av.gameObject.activeInHierarchy).ToArray();

            if (_avatars != null)
            {
                foreach (VRC_AvatarDescriptor a in newAvatars)
                    if (_avatars.Contains(a) == false)
                        _builder.CheckedForIssues = false;
            }

            _avatars = newAvatars;
        }

        public virtual void OnGUIAvatarCheck(VRC_AvatarDescriptor avatar)
        {
            string vrcFilePath = UnityWebRequest.UnEscapeURL(EditorPrefs.GetString("currentBuildingAssetBundlePath"));
            if (!string.IsNullOrEmpty(vrcFilePath) &&
                ValidationHelpers.CheckIfAssetBundleFileTooLarge(ContentType.Avatar, vrcFilePath, out int fileSize))
            {
                _builder.OnGUIWarning(avatar,
                    ValidationHelpers.GetAssetBundleOverSizeLimitMessageSDKWarning(ContentType.Avatar, fileSize),
                    delegate { Selection.activeObject = avatar.gameObject; }, null);
            }

            AvatarPerformanceStats perfStats = new AvatarPerformanceStats();
            AvatarPerformance.CalculatePerformanceStats(avatar.Name, avatar.gameObject, perfStats);

            OnGUIPerformanceInfo(avatar, perfStats, AvatarPerformanceCategory.Overall,
                GetAvatarSubSelectAction(avatar, typeof(VRC_AvatarDescriptor)), null);
            OnGUIPerformanceInfo(avatar, perfStats, AvatarPerformanceCategory.PolyCount,
                GetAvatarSubSelectAction(avatar, new[] {typeof(MeshRenderer), typeof(SkinnedMeshRenderer)}), null);
            OnGUIPerformanceInfo(avatar, perfStats, AvatarPerformanceCategory.AABB,
                GetAvatarSubSelectAction(avatar, typeof(VRC_AvatarDescriptor)), null);

            if (avatar.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape &&
                avatar.VisemeSkinnedMesh == null)
                _builder.OnGUIError(avatar, "This avatar uses Visemes but the Face Mesh is not specified.",
                    delegate { Selection.activeObject = avatar.gameObject; }, null);

            if (ShaderKeywordsUtility.DetectCustomShaderKeywords(avatar))
                _builder.OnGUIWarning(avatar,
                    "A Material on this avatar has custom shader keywords. Please consider optimizing it using the Shader Keywords Utility.",
                    () => { Selection.activeObject = avatar.gameObject; },
                    () =>
                    {
                        EditorApplication.ExecuteMenuItem("VRChat SDK/Utilities/Avatar Shader Keywords Utility");
                    });

            VerifyAvatarMipMapStreaming(avatar);

            Animator anim = avatar.GetComponent<Animator>();
            if (anim == null)
            {
                _builder.OnGUIWarning(avatar,
                    "This avatar does not contain an Animator, and will not animate in VRChat.",
                    delegate { Selection.activeObject = avatar.gameObject; }, null);
            }
            else if (anim.isHuman == false)
            {
                _builder.OnGUIWarning(avatar,
                    "This avatar is not imported as a humanoid rig and will not play VRChat's provided animation set.",
                    delegate { Selection.activeObject = avatar.gameObject; }, null);
            }
            else if (avatar.gameObject.activeInHierarchy == false)
            {
                _builder.OnGUIError(avatar, "Your avatar is disabled in the scene hierarchy!",
                    delegate { Selection.activeObject = avatar.gameObject; }, null);
            }
            else
            {
                Transform lFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                Transform rFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                if ((lFoot == null) || (rFoot == null))
                    _builder.OnGUIError(avatar, "Your avatar is humanoid, but its feet aren't specified!",
                        delegate { Selection.activeObject = avatar.gameObject; }, null);
                if (lFoot != null && rFoot != null)
                {
                    Vector3 footPos = lFoot.position - avatar.transform.position;
                    if (footPos.y < 0)
                        _builder.OnGUIWarning(avatar,
                            "Avatar feet are beneath the avatar's origin (the floor). That's probably not what you want.",
                            delegate
                            {
                                List<Object> gos = new List<Object> {rFoot.gameObject, lFoot.gameObject};
                                Selection.objects = gos.ToArray();
                            }, null);
                }

                Transform lShoulder = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                Transform rShoulder = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                if (lShoulder == null || rShoulder == null)
                    _builder.OnGUIError(avatar, "Your avatar is humanoid, but its upper arms aren't specified!",
                        delegate { Selection.activeObject = avatar.gameObject; }, null);
                if (lShoulder != null && rShoulder != null)
                {
                    Vector3 shoulderPosition = lShoulder.position - avatar.transform.position;
                    if (shoulderPosition.y < 0.2f)
                        _builder.OnGUIError(avatar, "This avatar is too short. The minimum is 20cm shoulder height.",
                            delegate { Selection.activeObject = avatar.gameObject; }, null);
                    else if (shoulderPosition.y < 1.0f)
                        _builder.OnGUIWarning(avatar, "This avatar is shorter than average.",
                            delegate { Selection.activeObject = avatar.gameObject; }, null);
                    else if (shoulderPosition.y > 5.0f)
                        _builder.OnGUIWarning(avatar, "This avatar is too tall. The maximum is 5m shoulder height.",
                            delegate { Selection.activeObject = avatar.gameObject; }, null);
                    else if (shoulderPosition.y > 2.5f)
                        _builder.OnGUIWarning(avatar, "This avatar is taller than average.",
                            delegate { Selection.activeObject = avatar.gameObject; }, null);
                }

                if (AnalyzeIK(avatar, anim) == false)
                    _builder.OnGUILink(avatar, "See Avatar Rig Requirements for more information.",
                        VRCSdkControlPanel.AVATAR_RIG_REQUIREMENTS_URL);
            }

            ValidateFeatures(avatar, anim, perfStats);

            Core.PipelineManager pm = avatar.GetComponent<Core.PipelineManager>();

            PerformanceRating rating = perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall);
            if (_builder.NoGuiErrors())
            {
                if (!anim.isHuman)
                {
                    if (pm != null) pm.fallbackStatus = Core.PipelineManager.FallbackStatus.InvalidRig;
                    _builder.OnGUIInformation(avatar, "This avatar does not have a humanoid rig, so it can not be used as a custom fallback.");
                }
                else if (rating > PerformanceRating.Good)
                {
                    if (pm != null) pm.fallbackStatus = Core.PipelineManager.FallbackStatus.InvalidPerformance;
                    _builder.OnGUIInformation(avatar, "This avatar does not have an overall rating of Good or better, so it can not be used as a custom fallback. See the link below for details on Avatar Optimization.");
                }
                else
                {
                    if (pm != null) pm.fallbackStatus = Core.PipelineManager.FallbackStatus.Valid;
                    _builder.OnGUIInformation(avatar, "This avatar can be used as a custom fallback. This avatar must be uploaded for every supported platform to be valid for fallback selection.");
                    if (perfStats.animatorCount.HasValue && perfStats.animatorCount.Value > 1)
                        _builder.OnGUIInformation(avatar, "This avatar uses additional animators, they will be disabled when used as a fallback.");
                }

                // additional messages for Poor and Very Poor Avatars
#if UNITY_ANDROID
                if (rating > PerformanceRating.Poor)
                    _builder.OnGUIInformation(avatar, "This avatar will be blocked by default due to performance. Your fallback will be shown instead.");
                else if (rating > PerformanceRating.Medium)
                    _builder.OnGUIInformation(avatar, "Other users may choose to block this avatar due to performance. Your fallback will be shown instead.");
#else
                if (rating > PerformanceRating.Medium)
                    _builder.OnGUIInformation(avatar, "Other users may choose to block this avatar due to performance. Your fallback will be shown instead.");
#endif
            }
            else
            {
                // shouldn't matter because we can't hit upload button
                if (pm != null) pm.fallbackStatus = Core.PipelineManager.FallbackStatus.InvalidPlatform;
            }
        }

        public virtual void ValidateFeatures(VRC_AvatarDescriptor avatar, Animator anim, AvatarPerformanceStats perfStats)
        {
            // stub, used in SDK3A for Expression Menu, etc.
        }

        protected void OnGUIPerformanceInfo(VRC_AvatarDescriptor avatar, AvatarPerformanceStats perfStats,
            AvatarPerformanceCategory perfCategory, Action show, Action fix)
        {
            PerformanceRating rating = perfStats.GetPerformanceRatingForCategory(perfCategory);
            SDKPerformanceDisplay.GetSDKPerformanceInfoText(perfStats, perfCategory, out string text,
                out PerformanceInfoDisplayLevel displayLevel);

            switch (displayLevel)
            {
                case PerformanceInfoDisplayLevel.None:
                {
                    break;
                }
                case PerformanceInfoDisplayLevel.Verbose:
                {
                    if (showAvatarPerformanceDetails)
                    {
                        _builder.OnGUIStat(avatar, text, rating, show, fix);
                    }

                    break;
                }
                case PerformanceInfoDisplayLevel.Info:
                {
                    _builder.OnGUIStat(avatar, text, rating, show, fix);
                    break;
                }
                case PerformanceInfoDisplayLevel.Warning:
                {
                    _builder.OnGUIStat(avatar, text, rating, show, fix);
                    break;
                }
                case PerformanceInfoDisplayLevel.Error:
                {
                    _builder.OnGUIStat(avatar, text, rating, show, fix);
                    _builder.OnGUIError(avatar, text, delegate { Selection.activeObject = avatar.gameObject; }, null);
                    break;
                }
                default:
                {
                    _builder.OnGUIError(avatar, "Unknown performance display level.",
                        delegate { Selection.activeObject = avatar.gameObject; }, null);
                    break;
                }
            }
        }

        public virtual void OnGUIAvatar(VRC_AvatarDescriptor avatar)
        {
            EditorGUILayout.BeginVertical(VRCSdkControlPanel.boxGuiStyle);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            EditorGUILayout.Space();

            GUILayout.Label("Online Publishing", VRCSdkControlPanel.infoGuiStyle);
            GUILayout.Label(
                "In order for other people to see your avatar in VRChat it must be built and published to our game servers.",
                VRCSdkControlPanel.infoGuiStyle);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.Space();

            GUI.enabled = _builder.NoGuiErrorsOrIssues() ||
                          Core.APIUser.CurrentUser.developerType == Core.APIUser.DeveloperType.Internal;
            if (GUILayout.Button(VRCSdkControlPanel.GetBuildAndPublishButtonString()))
            {
                bool buildBlocked = !VRCBuildPipelineCallbacks.OnVRCSDKBuildRequested(VRCSDKRequestedBuildType.Avatar);
                if (!buildBlocked)
                {
                    if (Core.APIUser.CurrentUser.canPublishAvatars)
                    {
                        EnvConfig.FogSettings originalFogSettings = EnvConfig.GetFogSettings();
                        EnvConfig.SetFogSettings(
                            new EnvConfig.FogSettings(EnvConfig.FogSettings.FogStrippingMode.Custom, true, true, true));

#if UNITY_ANDROID
                        EditorPrefs.SetBool("VRC.SDKBase_StripAllShaders", true);
#else
                        EditorPrefs.SetBool("VRC.SDKBase_StripAllShaders", false);
#endif

#if VRC_SDK_VRCSDK2
                        VRC_SdkBuilder.shouldBuildUnityPackage = VRCSdkControlPanel.FutureProofPublishEnabled;
                        VRC_SdkBuilder.ExportAndUploadAvatarBlueprint(avatar.gameObject);
#endif

                        EnvConfig.SetFogSettings(originalFogSettings);

                        // this seems to workaround a Unity bug that is clearing the formatting of two levels of Layout
                        // when we call the upload functions
                        return;
                    }
                    else
                    {
                        VRCSdkControlPanel.ShowContentPublishPermissionsDialog();
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUI.enabled = true;
        }

        private static void OnGUIAvatarSettings(VRC_AvatarDescriptor avatar)
        {
            EditorGUILayout.BeginVertical(VRCSdkControlPanel.boxGuiStyle, GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth));

            string name = "Unpublished Avatar - " + avatar.gameObject.name;
            if (avatar.apiAvatar != null)
                name = (avatar.apiAvatar as Core.ApiAvatar)?.name;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(name, VRCSdkControlPanel.titleGuiStyle);

            Core.PipelineManager pm = avatar.GetComponent<Core.PipelineManager>();
            if (pm != null && !string.IsNullOrEmpty(pm.blueprintId))
            {
                if (avatar.apiAvatar == null)
                {
                    Core.ApiAvatar av = Core.API.FromCacheOrNew<Core.ApiAvatar>(pm.blueprintId);
                    av.Fetch(
                        c => avatar.apiAvatar = c.Model as Core.ApiAvatar,
                        c =>
                        {
                            if (c.Code == 404)
                            {
                                Core.Logger.Log(
                                    $"Could not load avatar {pm.blueprintId} because it didn't exist.",
                                    Core.DebugLevel.API);
                                Core.ApiCache.Invalidate<Core.ApiWorld>(pm.blueprintId);
                            }
                            else
                                Debug.LogErrorFormat("Could not load avatar {0} because {1}", pm.blueprintId, c.Error);
                        });
                    avatar.apiAvatar = av;
                }
            }

            if (avatar.apiAvatar != null)
            {
                Core.ApiAvatar a = (avatar.apiAvatar as Core.ApiAvatar);
                DrawContentInfoForAvatar(a);
                VRCSdkControlPanel.DrawContentPlatformSupport(a);
            }

            VRCSdkControlPanel.DrawBuildTargetSwitcher();
            EditorGUILayout.EndVertical();
        }

        private static void DrawContentInfoForAvatar(Core.ApiAvatar a)
        {
            VRCSdkControlPanel.DrawContentInfo(a.name, a.version.ToString(), a.description, null, a.releaseStatus,
                a.tags);
        }

        protected static Action GetAvatarSubSelectAction(Component avatar, Type[] types)
        {
            return () =>
            {
                List<Object> gos = new List<Object>();
                foreach (Type t in types)
                {
                    Component[] components = avatar.GetComponentsInChildren(t, true);
                    foreach (Component c in components)
                        gos.Add(c.gameObject);
                }

                Selection.objects = gos.Count > 0 ? gos.ToArray() : new Object[] {avatar.gameObject};
            };
        }

        protected static Action GetAvatarSubSelectAction(Component avatar, Type type)
        {
            List<Type> t = new List<Type> {type};
            return GetAvatarSubSelectAction(avatar, t.ToArray());
        }

        private void VerifyAvatarMipMapStreaming(Component avatar)
        {
            List<Object> badTextures = new List<Object>();
            foreach (Renderer r in avatar.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (!m)
                        continue;
                    int[] texIDs = m.GetTexturePropertyNameIDs();
                    if (texIDs == null)
                        continue;
                    foreach (int i in texIDs)
                    {
                        Texture t = m.GetTexture(i);
                        if (!t)
                            continue;
                        string path = AssetDatabase.GetAssetPath(t);
                        if (string.IsNullOrEmpty(path))
                            continue;
                        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (importer != null && importer.mipmapEnabled && !importer.streamingMipmaps)
                            badTextures.Add(importer);
                    }
                }
            }

            if (badTextures.Count == 0)
                return;

            _builder.OnGUIError(avatar, "This avatar has mipmapped textures without 'Streaming Mip Maps' enabled.",
                () => { Selection.objects = badTextures.ToArray(); },
                () =>
                {
                    List<string> paths = new List<string>();
                    foreach (Object o in badTextures)
                    {
                        TextureImporter t = (TextureImporter) o;
                        Undo.RecordObject(t, "Set Mip Map Streaming");
                        t.streamingMipmaps = true;
                        t.streamingMipmapsPriority = 0;
                        EditorUtility.SetDirty(t);
                        paths.Add(t.assetPath);
                    }

                    AssetDatabase.ForceReserializeAssets(paths);
                    AssetDatabase.Refresh();
                });
        }

        private bool AnalyzeIK(Object ad, Animator anim)
        {
            bool hasHead;
            bool hasFeet;
            bool hasHands;
            bool hasThreeFingers;
            bool correctSpineHierarchy;
            bool correctLeftArmHierarchy;
            bool correctRightArmHierarchy;
            bool correctLeftLegHierarchy;
            bool correctRightLegHierarchy;

            bool status = true;

            Transform head = anim.GetBoneTransform(HumanBodyBones.Head);
            Transform lFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
            Transform lHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform rHand = anim.GetBoneTransform(HumanBodyBones.RightHand);

            hasHead = null != head;
            hasFeet = (null != lFoot && null != rFoot);
            hasHands = (null != lHand && null != rHand);

            if (!hasHead || !hasFeet || !hasHands)
            {
                _builder.OnGUIError(ad, "Humanoid avatar must have head, hands and feet bones mapped.",
                    delegate { Selection.activeObject = anim.gameObject; }, null);
                return false;
            }

            Transform lThumb = anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
            Transform lIndex = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
            Transform lMiddle = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            Transform rThumb = anim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            Transform rIndex = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
            Transform rMiddle = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

#if VRC_SDK_VRCSDK2
            // Finger test, only for v2
            hasThreeFingers = null != lThumb && null != lIndex && null != lMiddle && null != rThumb && null != rIndex &&
                              null != rMiddle;

            if (!hasThreeFingers)
            {
                // although its only a warning, we return here because the rest
                // of the analysis is for VR IK
                _builder.OnGUIWarning(ad,
                    "Thumb, Index, and Middle finger bones are not mapped, Full-Body IK will be disabled.",
                    delegate { Selection.activeObject = anim.gameObject; }, null);
                status = false;
            }
#endif

            Transform pelvis = anim.GetBoneTransform(HumanBodyBones.Hips);
            Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
            Transform upperChest = anim.GetBoneTransform(HumanBodyBones.UpperChest);
            Transform torso = anim.GetBoneTransform(HumanBodyBones.Spine);

            Transform neck = anim.GetBoneTransform(HumanBodyBones.Neck);
            Transform lClav = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
            Transform rClav = anim.GetBoneTransform(HumanBodyBones.RightShoulder);


            if (null == neck || null == lClav || null == rClav || null == pelvis || null == torso || null == chest)
            {
                string missingElements =
                    ((null == neck) ? "Neck, " : "") +
                    (((null == lClav) || (null == rClav)) ? "Shoulders, " : "") +
                    ((null == pelvis) ? "Pelvis, " : "") +
                    ((null == torso) ? "Spine, " : "") +
                    ((null == chest) ? "Chest, " : "");
                missingElements = missingElements.Remove(missingElements.LastIndexOf(',')) + ".";
                _builder.OnGUIError(ad, "Spine hierarchy missing elements, please map: " + missingElements,
                    delegate { Selection.activeObject = anim.gameObject; }, null);
                return false;
            }

            if (null != upperChest)
                correctSpineHierarchy =
                    lClav.parent == upperChest && rClav.parent == upperChest && neck.parent == upperChest;
            else
                correctSpineHierarchy = lClav.parent == chest && rClav.parent == chest && neck.parent == chest;

            if (!correctSpineHierarchy)
            {
                _builder.OnGUIError(ad,
                    "Spine hierarchy incorrect. Make sure that the parent of both Shoulders and the Neck is the Chest (or UpperChest if set).",
                    delegate
                    {
                        List<Object> gos = new List<Object>
                        {
                            lClav.gameObject,
                            rClav.gameObject,
                            neck.gameObject,
                            null != upperChest ? upperChest.gameObject : chest.gameObject
                        };
                        Selection.objects = gos.ToArray();
                    }, null);
                return false;
            }

            Transform lShoulder = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform lElbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform rShoulder = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform rElbow = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);

            correctLeftArmHierarchy = lShoulder && lElbow && lShoulder.GetChild(0) == lElbow && lHand &&
                                      lElbow.GetChild(0) == lHand;
            correctRightArmHierarchy = rShoulder && rElbow && rShoulder.GetChild(0) == rElbow && rHand &&
                                       rElbow.GetChild(0) == rHand;

            if (!(correctLeftArmHierarchy && correctRightArmHierarchy))
            {
                _builder.OnGUIWarning(ad,
                    "LowerArm is not first child of UpperArm or Hand is not first child of LowerArm: you may have problems with Forearm rotations.",
                    delegate
                    {
                        List<Object> gos = new List<Object>();
                        if (!correctLeftArmHierarchy && lShoulder)
                            gos.Add(lShoulder.gameObject);
                        if (!correctRightArmHierarchy && rShoulder)
                            gos.Add(rShoulder.gameObject);
                        if (gos.Count > 0)
                            Selection.objects = gos.ToArray();
                        else
                            Selection.activeObject = anim.gameObject;
                    }, null);
                status = false;
            }

            Transform lHip = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform lKnee = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Transform rHip = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Transform rKnee = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);

            correctLeftLegHierarchy = lHip && lKnee && lHip.GetChild(0) == lKnee && lKnee.GetChild(0) == lFoot;
            correctRightLegHierarchy = rHip && rKnee && rHip.GetChild(0) == rKnee && rKnee.GetChild(0) == rFoot;

            if (!(correctLeftLegHierarchy && correctRightLegHierarchy))
            {
                _builder.OnGUIWarning(ad,
                    "LowerLeg is not first child of UpperLeg or Foot is not first child of LowerLeg: you may have problems with Shin rotations.",
                    delegate
                    {
                        List<Object> gos = new List<Object>();
                        if (!correctLeftLegHierarchy && lHip)
                            gos.Add(lHip.gameObject);
                        if (!correctRightLegHierarchy && rHip)
                            gos.Add(rHip.gameObject);
                        if (gos.Count > 0)
                            Selection.objects = gos.ToArray();
                        else
                            Selection.activeObject = anim.gameObject;
                    }, null);
                status = false;
            }

            if (!(IsAncestor(pelvis, rFoot) && IsAncestor(pelvis, lFoot) && IsAncestor(pelvis, lHand) &&
                  IsAncestor(pelvis, rHand)))
            {
                _builder.OnGUIWarning(ad,
                    "This avatar has a split hierarchy (Hips bone is not the ancestor of all humanoid bones). IK may not work correctly.",
                    delegate
                    {
                        List<Object> gos = new List<Object> {pelvis.gameObject};
                        if (!IsAncestor(pelvis, rFoot))
                            gos.Add(rFoot.gameObject);
                        if (!IsAncestor(pelvis, lFoot))
                            gos.Add(lFoot.gameObject);
                        if (!IsAncestor(pelvis, lHand))
                            gos.Add(lHand.gameObject);
                        if (!IsAncestor(pelvis, rHand))
                            gos.Add(rHand.gameObject);
                        Selection.objects = gos.ToArray();
                    }, null);
                status = false;
            }

            // if thigh bone rotations diverge from 180 from hip bone rotations, full-body tracking/ik does not work well
            if (!lHip || !rHip) return status;
            {
                Vector3 hipLocalUp = pelvis.InverseTransformVector(Vector3.up);
                Vector3 legLDir = lHip.TransformVector(hipLocalUp);
                Vector3 legRDir = rHip.TransformVector(hipLocalUp);
                float angL = Vector3.Angle(Vector3.up, legLDir);
                float angR = Vector3.Angle(Vector3.up, legRDir);
                if (!(angL < 175f) && !(angR < 175f)) return status;
                string angle = $"{Mathf.Min(angL, angR):F1}";
                _builder.OnGUIWarning(ad,
                    $"The angle between pelvis and thigh bones should be close to 180 degrees (this avatar's angle is {angle}). Your avatar may not work well with full-body IK and Tracking.",
                    delegate
                    {
                        List<Object> gos = new List<Object>();
                        if (angL < 175f)
                            gos.Add(rFoot.gameObject);
                        if (angR < 175f)
                            gos.Add(lFoot.gameObject);
                        Selection.objects = gos.ToArray();
                    }, null);
                status = false;
            }

            return status;
        }

        private static bool IsAncestor(Object ancestor, Transform child)
        {
            bool found = false;
            Transform thisParent = child.parent;
            while (thisParent != null)
            {
                if (thisParent == ancestor)
                {
                    found = true;
                    break;
                }

                thisParent = thisParent.parent;
            }

            return found;
        }

        protected void CheckAvatarMeshesForLegacyBlendShapesSetting(Component avatar)
        {
            if (LegacyBlendShapeNormalsPropertyInfo == null)
            {
                Debug.LogError(
                    "Could not check for legacy blend shape normals because 'legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes' was not found.");
                return;
            }

            // Get all of the meshes used by skinned mesh renderers.
            HashSet<Mesh> avatarMeshes = GetAllMeshesInGameObjectHierarchy(avatar.gameObject);
            HashSet<Mesh> incorrectlyConfiguredMeshes =
                ScanMeshesForIncorrectBlendShapeNormalsSetting(avatarMeshes);
            if (incorrectlyConfiguredMeshes.Count > 0)
            {
                _builder.OnGUIError(
                    avatar,
                    "This avatar contains skinned meshes that were imported with Blendshape Normals set to 'Calculate' but aren't using 'Legacy Blendshape Normals'. This will significantly increase the size of the uploaded avatar. This must be fixed in the mesh import settings before uploading.",
                    null,
                    () => { EnableLegacyBlendShapeNormals(incorrectlyConfiguredMeshes); });
            }
        }

        private static HashSet<Mesh> ScanMeshesForIncorrectBlendShapeNormalsSetting(IEnumerable<Mesh> avatarMeshes)
        {
            HashSet<Mesh> incorrectlyConfiguredMeshes = new HashSet<Mesh>();
            foreach (Mesh avatarMesh in avatarMeshes)
            {
                // Can't get ModelImporter if the model isn't an asset.
                if (!AssetDatabase.Contains(avatarMesh))
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(avatarMesh);
                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if (avatarImporter == null)
                {
                    continue;
                }

                if (avatarImporter.importBlendShapeNormals != ModelImporterNormals.Calculate)
                {
                    continue;
                }

                bool useLegacyBlendShapeNormals = (bool) LegacyBlendShapeNormalsPropertyInfo.GetValue(avatarImporter);
                if (useLegacyBlendShapeNormals)
                {
                    continue;
                }

                if (!incorrectlyConfiguredMeshes.Contains(avatarMesh))
                {
                    incorrectlyConfiguredMeshes.Add(avatarMesh);
                }
            }

            return incorrectlyConfiguredMeshes;
        }

        private static void EnableLegacyBlendShapeNormals(IEnumerable<Mesh> meshesToFix)
        {
            HashSet<string> meshAssetPaths = new HashSet<string>();
            foreach (Mesh meshToFix in meshesToFix)
            {
                // Can't get ModelImporter if the model isn't an asset.
                if (!AssetDatabase.Contains(meshToFix))
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(meshToFix);
                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                if (meshAssetPaths.Contains(meshAssetPath))
                {
                    continue;
                }

                meshAssetPaths.Add(meshAssetPath);
            }

            foreach (string meshAssetPath in meshAssetPaths)
            {
                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if (avatarImporter == null)
                {
                    continue;
                }

                if (avatarImporter.importBlendShapeNormals != ModelImporterNormals.Calculate)
                {
                    continue;
                }

                LegacyBlendShapeNormalsPropertyInfo.SetValue(avatarImporter, true);
                avatarImporter.SaveAndReimport();
            }
        }

        protected void CheckAvatarMeshesForMeshReadWriteSetting(Component avatar)
        {
            // Get all of the meshes used by skinned mesh renderers.
            HashSet<Mesh> avatarMeshes = GetAllMeshesInGameObjectHierarchy(avatar.gameObject);
            HashSet<Mesh> incorrectlyConfiguredMeshes =
                ScanMeshesForDisabledMeshReadWriteSetting(avatarMeshes);
            if (incorrectlyConfiguredMeshes.Count > 0)
            {
                _builder.OnGUIError(
                    avatar,
                    "This avatar contains meshes that were imported with Read/Write disabled. This must be fixed in the mesh import settings before uploading.",
                    null,
                    () => { EnableMeshReadWrite(incorrectlyConfiguredMeshes); });
            }
        }

        private static HashSet<Mesh> ScanMeshesForDisabledMeshReadWriteSetting(IEnumerable<Mesh> avatarMeshes)
        {
            HashSet<Mesh> incorrectlyConfiguredMeshes = new HashSet<Mesh>();
            foreach (Mesh avatarMesh in avatarMeshes)
            {
                // Can't get ModelImporter if the model isn't an asset.
                if (!AssetDatabase.Contains(avatarMesh))
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(avatarMesh);
                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if (avatarImporter == null)
                {
                    continue;
                }

                if (avatarImporter.isReadable)
                {
                    continue;
                }

                if (!incorrectlyConfiguredMeshes.Contains(avatarMesh))
                {
                    incorrectlyConfiguredMeshes.Add(avatarMesh);
                }
            }

            return incorrectlyConfiguredMeshes;
        }

        private static void EnableMeshReadWrite(IEnumerable<Mesh> meshesToFix)
        {
            HashSet<string> meshAssetPaths = new HashSet<string>();
            foreach (Mesh meshToFix in meshesToFix)
            {
                // Can't get ModelImporter if the model isn't an asset.
                if (!AssetDatabase.Contains(meshToFix))
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(meshToFix);
                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                if (meshAssetPaths.Contains(meshAssetPath))
                {
                    continue;
                }

                meshAssetPaths.Add(meshAssetPath);
            }

            foreach (string meshAssetPath in meshAssetPaths)
            {
                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if (avatarImporter == null)
                {
                    continue;
                }

                if (avatarImporter.isReadable)
                {
                    continue;
                }

                avatarImporter.isReadable = true;
                avatarImporter.SaveAndReimport();
            }
        }

        private static HashSet<Mesh> GetAllMeshesInGameObjectHierarchy(GameObject avatar)
        {
            HashSet<Mesh> avatarMeshes = new HashSet<Mesh>();
            foreach (SkinnedMeshRenderer avatarSkinnedMeshRenderer in avatar
                .GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (avatarSkinnedMeshRenderer == null)
                {
                    continue;
                }

                Mesh skinnedMesh = avatarSkinnedMeshRenderer.sharedMesh;
                if (skinnedMesh == null)
                {
                    continue;
                }

                if (avatarMeshes.Contains(skinnedMesh))
                {
                    continue;
                }

                avatarMeshes.Add(skinnedMesh);
            }

            foreach (MeshFilter avatarMeshFilter in avatar.GetComponentsInChildren<MeshFilter>(true))
            {
                if (avatarMeshFilter == null)
                {
                    continue;
                }

                Mesh skinnedMesh = avatarMeshFilter.sharedMesh;
                if (skinnedMesh == null)
                {
                    continue;
                }

                if (avatarMeshes.Contains(skinnedMesh))
                {
                    continue;
                }

                avatarMeshes.Add(skinnedMesh);
            }

            foreach (ParticleSystemRenderer avatarParticleSystemRenderer in avatar
                .GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                if (avatarParticleSystemRenderer == null)
                {
                    continue;
                }

                Mesh[] avatarParticleSystemRendererMeshes = new Mesh[avatarParticleSystemRenderer.meshCount];
                avatarParticleSystemRenderer.GetMeshes(avatarParticleSystemRendererMeshes);
                foreach (Mesh avatarParticleSystemRendererMesh in avatarParticleSystemRendererMeshes)
                {
                    if (avatarParticleSystemRendererMesh == null)
                    {
                        continue;
                    }

                    if (avatarMeshes.Contains(avatarParticleSystemRendererMesh))
                    {
                        continue;
                    }

                    avatarMeshes.Add(avatarParticleSystemRendererMesh);
                }
            }

            return avatarMeshes;
        }

        protected void OpenAnimatorControllerWindow(object animatorController)
        {
            Assembly asm = Assembly.Load("UnityEditor.Graphs");
            Module editorGraphModule = asm.GetModule("UnityEditor.Graphs.dll");
            Type animatorWindowType = editorGraphModule.GetType("UnityEditor.Graphs.AnimatorControllerTool");
            EditorWindow animatorWindow = EditorWindow.GetWindow(animatorWindowType, false, "Animator", false);
            PropertyInfo propInfo = animatorWindowType.GetProperty("animatorController");
            if (propInfo != null) propInfo.SetValue(animatorWindow, animatorController, null);
        }

        protected static void ShowRestrictedComponents(IEnumerable<Component> componentsToRemove)
        {
            List<Object> gos = new List<Object>();
            foreach (Component c in componentsToRemove)
                gos.Add(c.gameObject);
            Selection.objects = gos.ToArray();
        }

        protected static void FixRestrictedComponents(IEnumerable<Component> componentsToRemove)
        {
            if (!(componentsToRemove is List<Component> list)) return;
            for (int v = list.Count - 1; v > -1; v--)
            {
                Object.DestroyImmediate(list[v]);
            }
        }

        public static void SelectAvatar(VRC_AvatarDescriptor avatar)
        {
            if (VRCSdkControlPanel.window != null)
                _selectedAvatar = avatar;
        }


        List<Transform> FindBonesBetween(Transform top, Transform bottom)
        {
            List<Transform> list = new List<Transform>();
            if (top == null || bottom == null) return list;
            Transform bt = top.parent;
            while (bt != bottom && bt != null)
            {
                list.Add(bt);
                bt = bt.parent;
            }

            return list;
        }
    }
}
