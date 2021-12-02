using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDKBase.Editor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Editor;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation;
using VRC.SDKBase.Validation.Performance.Stats;
using VRCStation = VRC.SDK3.Avatars.Components.VRCStation;
using VRC.SDK3.Validation;

[assembly: VRCSdkControlPanelBuilder(typeof(VRCSdkControlPanelAvatarBuilder3A))]

namespace VRC.SDK3.Editor
{
    public class VRCSdkControlPanelAvatarBuilder3A : VRCSdkControlPanelAvatarBuilder
    {
        public override void ValidateFeatures(VRC_AvatarDescriptor avatar, Animator anim, AvatarPerformanceStats perfStats)
        {
            //Create avatar debug hashset
            VRCAvatarDescriptor avatarSDK3 = avatar as VRCAvatarDescriptor;
            if (avatarSDK3 != null)
            {
                avatarSDK3.animationHashSet.Clear();

                foreach (VRCAvatarDescriptor.CustomAnimLayer animLayer in avatarSDK3.baseAnimationLayers)
                {
                    AnimatorController controller = animLayer.animatorController as AnimatorController;
                    if (controller != null)
                    {
                        foreach (AnimatorControllerLayer layer in controller.layers)
                        {
                            ProcessStateMachine(layer.stateMachine, "");
                            void ProcessStateMachine(AnimatorStateMachine stateMachine, string prefix)
                            {
                                //Update prefix
                                prefix = prefix + stateMachine.name + ".";

                                //States
                                foreach (var state in stateMachine.states)
                                {
                                    VRCAvatarDescriptor.DebugHash hash = new VRCAvatarDescriptor.DebugHash();
                                    string fullName = prefix + state.state.name;
                                    hash.hash = Animator.StringToHash(fullName);
                                    hash.name = fullName.Remove(0, layer.stateMachine.name.Length + 1);
                                    avatarSDK3.animationHashSet.Add(hash);
                                }

                                //Sub State Machines
                                foreach (var subMachine in stateMachine.stateMachines)
                                    ProcessStateMachine(subMachine.stateMachine, prefix);
                            }
                        }
                    }
                }
            }

            //Validate Playable Layers
            if (avatarSDK3 != null && avatarSDK3.customizeAnimationLayers)
            {
                VRCAvatarDescriptor.CustomAnimLayer gestureLayer = avatarSDK3.baseAnimationLayers[2];
                if (anim != null
                    && anim.isHuman
                    && gestureLayer.animatorController != null
                    && gestureLayer.type == VRCAvatarDescriptor.AnimLayerType.Gesture
                    && !gestureLayer.isDefault)
                {
                    AnimatorController controller = gestureLayer.animatorController as AnimatorController;
                    if (controller != null && controller.layers[0].avatarMask == null)
                        _builder.OnGUIError(avatar, "Gesture Layer needs valid mask on first animator layer",
                            delegate { OpenAnimatorControllerWindow(controller); }, null);
                }
            }

            //Expression menu images
            if (avatarSDK3 != null)
            {
                bool ValidateTexture(Texture2D texture)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null)
                        return true;
                    TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();

                    //Max texture size
                    if ((texture.width > MAX_ACTION_TEXTURE_SIZE || texture.height > MAX_ACTION_TEXTURE_SIZE) &&
                        settings.maxTextureSize > MAX_ACTION_TEXTURE_SIZE)
                        return false;

                    //Compression
                    if (settings.textureCompression == TextureImporterCompression.Uncompressed)
                        return false;

                    //Success
                    return true;
                }

                void FixTexture(Texture2D texture)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null)
                        return;
                    TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();

                    //Max texture size
                    if (texture.width > MAX_ACTION_TEXTURE_SIZE || texture.height > MAX_ACTION_TEXTURE_SIZE)
                        settings.maxTextureSize = Math.Min(settings.maxTextureSize, MAX_ACTION_TEXTURE_SIZE);

                    //Compression
                    if (settings.textureCompression == TextureImporterCompression.Uncompressed)
                        settings.textureCompression = TextureImporterCompression.Compressed;

                    //Set & Reimport
                    importer.SetPlatformTextureSettings(settings);
                    AssetDatabase.ImportAsset(path);
                }

                //Find all textures
                List<Texture2D> textures = new List<Texture2D>();
                List<VRCExpressionsMenu> menuStack = new List<VRCExpressionsMenu>();
                FindTextures(avatarSDK3.expressionsMenu);

                void FindTextures(VRCExpressionsMenu menu)
                {
                    if (menu == null || menuStack.Contains(menu)) //Prevent recursive menu searching
                        return;
                    menuStack.Add(menu);

                    //Check controls
                    foreach (VRCExpressionsMenu.Control control in menu.controls)
                    {
                        AddTexture(control.icon);
                        if (control.labels != null)
                        {
                            foreach (VRCExpressionsMenu.Control.Label label in control.labels)
                                AddTexture(label.icon);
                        }

                        if (control.subMenu != null)
                            FindTextures(control.subMenu);
                    }

                    void AddTexture(Texture2D texture)
                    {
                        if (texture != null)
                            textures.Add(texture);
                    }
                }

                //Validate
                bool isValid = true;
                foreach (Texture2D texture in textures)
                {
                    if (!ValidateTexture(texture))
                        isValid = false;
                }

                if (!isValid)
                    _builder.OnGUIError(avatar, "Images used for Actions & Moods are too large.",
                        delegate { Selection.activeObject = avatar.gameObject; }, FixTextures);

                //Fix
                void FixTextures()
                {
                    foreach (Texture2D texture in textures)
                        FixTexture(texture);
                }
            }

            //Expression menu parameters
            if (avatarSDK3 != null)
            {
                //Check for expression menu/parameters object
                if (avatarSDK3.expressionsMenu != null || avatarSDK3.expressionParameters != null)
                {
                    //Menu
                    if (avatarSDK3.expressionsMenu == null)
                        _builder.OnGUIError(avatar, "VRCExpressionsMenu object reference is missing.",
                            delegate { Selection.activeObject = avatarSDK3; }, null);

                    //Parameters
                    if (avatarSDK3.expressionParameters == null)
                        _builder.OnGUIError(avatar, "VRCExpressionParameters object reference is missing.",
                            delegate { Selection.activeObject = avatarSDK3; }, null);
                }

                //Check if parameters is valid
                if (avatarSDK3.expressionParameters != null && avatarSDK3.expressionParameters.CalcTotalCost() > VRCExpressionParameters.MAX_PARAMETER_COST)
                {
                    _builder.OnGUIError(avatar, "VRCExpressionParameters has too many parameters defined.",
                        delegate { Selection.activeObject = avatarSDK3.expressionParameters; }, null);
                }

                //Find all existing parameters
                if (avatarSDK3.expressionsMenu != null && avatarSDK3.expressionParameters != null)
                {
                    List<VRCExpressionsMenu> menuStack = new List<VRCExpressionsMenu>();
                    List<string> parameters = new List<string>();
                    List<VRCExpressionsMenu> selects = new List<VRCExpressionsMenu>();
                    FindParameters(avatarSDK3.expressionsMenu);

                    void FindParameters(VRCExpressionsMenu menu)
                    {
                        if (menu == null || menuStack.Contains(menu)) //Prevent recursive menu searching
                            return;
                        menuStack.Add(menu);

                        //Check controls
                        foreach (VRCExpressionsMenu.Control control in menu.controls)
                        {
                            AddParameter(control.parameter);
                            if (control.subParameters != null)
                            {
                                foreach (VRCExpressionsMenu.Control.Parameter subParameter in control.subParameters)
                                {
                                    AddParameter(subParameter);
                                }
                            }

                            if (control.subMenu != null)
                                FindParameters(control.subMenu);
                        }

                        void AddParameter(VRCExpressionsMenu.Control.Parameter parameter)
                        {
                            if (parameter != null)
                            {
                                parameters.Add(parameter.name);
                                selects.Add(menu);
                            }
                        }
                    }

                    //Validate parameters
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        string parameter = parameters[i];
                        VRCExpressionsMenu select = selects[i];

                        //Find
                        bool exists = string.IsNullOrEmpty(parameter) || avatarSDK3.expressionParameters.FindParameter(parameter) != null;
                        if (!exists)
                        {
                            _builder.OnGUIError(avatar,
                                "VRCExpressionsMenu uses a parameter that is not defined.\nParameter: " + parameter,
                                delegate { Selection.activeObject = select; }, null);
                        }
                    }

                    //Validate param choices
                    foreach (var menu in menuStack)
                    {
                        foreach (var control in menu.controls)
                        {
                            bool isValid = true;
                            if (control.type == VRCExpressionsMenu.Control.ControlType.FourAxisPuppet)
                            {
                                isValid &= ValidateNonBoolParam(control.subParameters[0].name);
                                isValid &= ValidateNonBoolParam(control.subParameters[1].name);
                                isValid &= ValidateNonBoolParam(control.subParameters[2].name);
                                isValid &= ValidateNonBoolParam(control.subParameters[3].name);
                            }
                            else if (control.type == VRCExpressionsMenu.Control.ControlType.RadialPuppet)
                            {
                                isValid &= ValidateNonBoolParam(control.subParameters[0].name);
                            }
                            else if (control.type == VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet)
                            {
                                isValid &= ValidateNonBoolParam(control.subParameters[0].name);
                                isValid &= ValidateNonBoolParam(control.subParameters[1].name);
                            }
                            if (!isValid)
                            {
                                _builder.OnGUIError(avatar,
                                "VRCExpressionsMenu uses an invalid parameter for a control.\nControl: " + control.name,
                                delegate { Selection.activeObject = menu; }, null);
                            }
                        }

                        bool ValidateNonBoolParam(string name)
                        {
                            VRCExpressionParameters.Parameter param = string.IsNullOrEmpty(name) ? null : avatarSDK3.expressionParameters.FindParameter(name);
                            if (param != null && param.valueType == VRCExpressionParameters.ValueType.Bool)
                                return false;
                            return true;
                        }
                    }
                }
            }

            List<Component> componentsToRemove = AvatarValidation.FindIllegalComponents(avatar.gameObject).ToList();

            // create a list of the PipelineSaver component(s)
            List<Component> toRemoveSilently = new List<Component>();
            foreach (Component c in componentsToRemove)
            {
                if (c.GetType().Name == "PipelineSaver")
                {
                    toRemoveSilently.Add(c);
                }
            }

            // delete PipelineSaver(s) from the list of the Components we will destroy now
            foreach (Component c in toRemoveSilently)
            {
                    componentsToRemove.Remove(c);
            }

            HashSet<string> componentsToRemoveNames = new HashSet<string>();
            List<Component> toRemove = componentsToRemove as List<Component> ?? componentsToRemove;
            foreach (Component c in toRemove)
            {
                if (componentsToRemoveNames.Contains(c.GetType().Name) == false)
                    componentsToRemoveNames.Add(c.GetType().Name);
            }

            if (componentsToRemoveNames.Count > 0)
                _builder.OnGUIError(avatar,
                    "The following component types are found on the Avatar and will be removed by the client: " +
                    string.Join(", ", componentsToRemoveNames.ToArray()),
                    delegate { ShowRestrictedComponents(toRemove); },
                    delegate { FixRestrictedComponents(toRemove); });

            List<AudioSource> audioSources =
                avatar.gameObject.GetComponentsInChildren<AudioSource>(true).ToList();
            if (audioSources.Count > 0)
                _builder.OnGUIWarning(avatar,
                    "Audio sources found on Avatar, they will be adjusted to safe limits, if necessary.",
                    GetAvatarSubSelectAction(avatar, typeof(AudioSource)), null);

            List<VRCStation> stations =
                avatar.gameObject.GetComponentsInChildren<VRCStation>(true).ToList();
            if (stations.Count > 0)
                _builder.OnGUIWarning(avatar, "Stations found on Avatar, they will be adjusted to safe limits, if necessary.",
                    GetAvatarSubSelectAction(avatar, typeof(VRCStation)), null);

            if (VRCSdkControlPanel.HasSubstances(avatar.gameObject))
            {
                _builder.OnGUIWarning(avatar,
                    "This avatar has one or more Substance materials, which is not supported and may break in-game. Please bake your Substances to regular materials.",
                    () => { Selection.objects = VRCSdkControlPanel.GetSubstanceObjects(avatar.gameObject); },
                    null);
            }

            CheckAvatarMeshesForLegacyBlendShapesSetting(avatar);
            CheckAvatarMeshesForMeshReadWriteSetting(avatar);

#if UNITY_ANDROID
        IEnumerable<Shader> illegalShaders = AvatarValidation.FindIllegalShaders(avatar.gameObject);
        foreach (Shader s in illegalShaders)
        {
            _builder.OnGUIError(avatar, "Avatar uses unsupported shader '" + s.name + "'. You can only use the shaders provided in 'VRChat/Mobile' for Quest avatars.", delegate () { Selection.activeObject
 = avatar.gameObject; }, null);
        }
#endif

            foreach (AvatarPerformanceCategory perfCategory in Enum.GetValues(typeof(AvatarPerformanceCategory)))
            {
                if (perfCategory == AvatarPerformanceCategory.Overall ||
                    perfCategory == AvatarPerformanceCategory.PolyCount ||
                    perfCategory == AvatarPerformanceCategory.AABB ||
                    perfCategory == AvatarPerformanceCategory.AvatarPerformanceCategoryCount)
                {
                    continue;
                }

                Action show = null;

                switch (perfCategory)
                {
                    case AvatarPerformanceCategory.AnimatorCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Animator));
                        break;
                    case AvatarPerformanceCategory.AudioSourceCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(AudioSource));
                        break;
                    case AvatarPerformanceCategory.BoneCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(SkinnedMeshRenderer));
                        break;
                    case AvatarPerformanceCategory.ClothCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Cloth));
                        break;
                    case AvatarPerformanceCategory.ClothMaxVertices:
                        show = GetAvatarSubSelectAction(avatar, typeof(Cloth));
                        break;
                    case AvatarPerformanceCategory.LightCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Light));
                        break;
                    case AvatarPerformanceCategory.LineRendererCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(LineRenderer));
                        break;
                    case AvatarPerformanceCategory.MaterialCount:
                        show = GetAvatarSubSelectAction(avatar,
                            new[] {typeof(MeshRenderer), typeof(SkinnedMeshRenderer)});
                        break;
                    case AvatarPerformanceCategory.MeshCount:
                        show = GetAvatarSubSelectAction(avatar,
                            new[] {typeof(MeshRenderer), typeof(SkinnedMeshRenderer)});
                        break;
                    case AvatarPerformanceCategory.ParticleCollisionEnabled:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.ParticleMaxMeshPolyCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.ParticleSystemCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.ParticleTotalCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.ParticleTrailsEnabled:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.PhysicsColliderCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Collider));
                        break;
                    case AvatarPerformanceCategory.PhysicsRigidbodyCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Rigidbody));
                        break;
                    case AvatarPerformanceCategory.PolyCount:
                        show = GetAvatarSubSelectAction(avatar,
                            new[] {typeof(MeshRenderer), typeof(SkinnedMeshRenderer)});
                        break;
                    case AvatarPerformanceCategory.SkinnedMeshCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(SkinnedMeshRenderer));
                        break;
                    case AvatarPerformanceCategory.TrailRendererCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(TrailRenderer));
                        break;
                }

                // we can only show these buttons if DynamicBone is installed

                Type dynamicBoneType = typeof(AvatarValidation).Assembly.GetType("DynamicBone");
                Type dynamicBoneColliderType = typeof(AvatarValidation).Assembly.GetType("DynamicBoneCollider");
                if ((dynamicBoneType != null) && (dynamicBoneColliderType != null))
                {
                    switch (perfCategory)
                    {
                        case AvatarPerformanceCategory.DynamicBoneColliderCount:
                            show = GetAvatarSubSelectAction(avatar, dynamicBoneColliderType);
                            break;
                        case AvatarPerformanceCategory.DynamicBoneCollisionCheckCount:
                            show = GetAvatarSubSelectAction(avatar, dynamicBoneColliderType);
                            break;
                        case AvatarPerformanceCategory.DynamicBoneComponentCount:
                            show = GetAvatarSubSelectAction(avatar, dynamicBoneType);
                            break;
                        case AvatarPerformanceCategory.DynamicBoneSimulatedBoneCount:
                            show = GetAvatarSubSelectAction(avatar, dynamicBoneType);
                            break;
                    }
                }

                OnGUIPerformanceInfo(avatar, perfStats, perfCategory, show, null);
            }

            _builder.OnGUILink(avatar, "Avatar Optimization Tips", VRCSdkControlPanel.AVATAR_OPTIMIZATION_TIPS_URL);

        }

        public override void OnGUIAvatar(VRC_AvatarDescriptor avatar)
        {
            EditorGUILayout.BeginVertical(VRCSdkControlPanel.boxGuiStyle);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            EditorGUILayout.Space();

            GUI.enabled = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                           EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64) &&
                          (_builder.NoGuiErrorsOrIssues() || Core.APIUser.CurrentUser.developerType ==
                              Core.APIUser.DeveloperType.Internal);

            GUILayout.Label("Offline Testing", VRCSdkControlPanel.infoGuiStyle);
            if (GUI.enabled)
            {
            GUILayout.Label(
                "Before uploading your avatar you may build and test it in the VRChat client. Other users will not able to see the test avatar.",
                VRCSdkControlPanel.infoGuiStyle);
            }
            else
            {
                GUILayout.Label(
                        "(Not available for Android build target)",
                        VRCSdkControlPanel.infoGuiStyle);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.Space();

            if (GUILayout.Button("Build & Test"))
            {
                if (Core.APIUser.CurrentUser.canPublishAvatars)
                {
                    VRC_SdkBuilder.ExportAndTestAvatarBlueprint(avatar.gameObject);

                    EditorUtility.DisplayDialog("VRChat SDK", "Test Avatar Built", "OK");
                }
                else
                {
                    VRCSdkControlPanel.ShowContentPublishPermissionsDialog();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

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

                        VRC_SdkBuilder.shouldBuildUnityPackage = VRCSdkControlPanel.FutureProofPublishEnabled;
                        VRC_SdkBuilder.ExportAndUploadAvatarBlueprint(avatar.gameObject);

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
    }
}
