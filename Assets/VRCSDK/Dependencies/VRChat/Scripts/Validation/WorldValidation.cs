using System;
using System.Collections.Generic;
using JetBrains.Annotations;
#if TextMeshPro
using TMPro;
#endif
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace VRC.SDKBase.Validation
{
    public static class WorldValidation
    {
        private static readonly Lazy<int> _debugLevel = new Lazy<int>(InitializeLogging);
        private static int DebugLevel => _debugLevel.Value;

        private static int InitializeLogging()
        {
            int hashCode = typeof(WorldValidation).GetHashCode();
            VRC.Core.Logger.DescribeDebugLevel(hashCode, "WorldValidation", VRC.Core.Logger.Color.red);
            VRC.Core.Logger.AddDebugLevel(hashCode);
            return hashCode;
        }

        static string[] ComponentTypeWhiteList = null;

        public enum WhiteListConfiguration
        {
            None,
            VRCSDK2,
            VRCSDK3,
            Unchanged
        }

        static WhiteListConfiguration ComponentTypeWhiteListConfiguration = WhiteListConfiguration.None;

        static readonly string[] ComponentTypeWhiteListCommon = new string[]
        {
            #if UNITY_STANDALONE
            "UnityEngine.Rendering.PostProcessing.PostProcessDebug",
            "UnityEngine.Rendering.PostProcessing.PostProcessLayer",
            "UnityEngine.Rendering.PostProcessing.PostProcessVolume",
            #endif
            "VRC.Core.PipelineManager",
            "UiInputField",
            "VRCProjectSettings",
            "DynamicBone",
            "DynamicBoneCollider",
            "TMPro.TMP_Dropdown",
            "TMPro.TMP_InputField",
            "TMPro.TMP_ScrollbarEventHandler",
            "TMPro.TMP_SelectionCaret",
            "TMPro.TMP_SpriteAnimator",
            "TMPro.TMP_SubMesh",
            "TMPro.TMP_SubMeshUI",
            "TMPro.TMP_Text",
            "TMPro.TextMeshPro",
            "TMPro.TextMeshProUGUI",
            "TMPro.TextContainer",
            "TMPro.TMP_Dropdown+DropdownItem",
            "UnityEngine.EventSystems.EventSystem",
            "UnityEngine.EventSystems.EventTrigger",
            "UnityEngine.EventSystems.UIBehaviour",
            "UnityEngine.EventSystems.BaseInput",
            "UnityEngine.EventSystems.BaseInputModule",
            "UnityEngine.EventSystems.PointerInputModule",
            "UnityEngine.EventSystems.StandaloneInputModule",
            "UnityEngine.EventSystems.TouchInputModule",
            "UnityEngine.EventSystems.BaseRaycaster",
            "UnityEngine.EventSystems.PhysicsRaycaster",
            "UnityEngine.UI.Button",
            "UnityEngine.UI.Dropdown",
            "UnityEngine.UI.Dropdown+DropdownItem",
            "UnityEngine.UI.Graphic",
            "UnityEngine.UI.GraphicRaycaster",
            "UnityEngine.UI.Image",
            "UnityEngine.UI.InputField",
            "UnityEngine.UI.Mask",
            "UnityEngine.UI.MaskableGraphic",
            "UnityEngine.UI.RawImage",
            "UnityEngine.UI.RectMask2D",
            "UnityEngine.UI.Scrollbar",
            "UnityEngine.UI.ScrollRect",
            "UnityEngine.UI.Selectable",
            "UnityEngine.UI.Slider",
            "UnityEngine.UI.Text",
            "UnityEngine.UI.Toggle",
            "UnityEngine.UI.ToggleGroup",
            "UnityEngine.UI.AspectRatioFitter",
            "UnityEngine.UI.CanvasScaler",
            "UnityEngine.UI.ContentSizeFitter",
            "UnityEngine.UI.GridLayoutGroup",
            "UnityEngine.UI.HorizontalLayoutGroup",
            "UnityEngine.UI.HorizontalOrVerticalLayoutGroup",
            "UnityEngine.UI.LayoutElement",
            "UnityEngine.UI.LayoutGroup",
            "UnityEngine.UI.VerticalLayoutGroup",
            "UnityEngine.UI.BaseMeshEffect",
            "UnityEngine.UI.Outline",
            "UnityEngine.UI.PositionAsUV1",
            "UnityEngine.UI.Shadow",
            "OVRLipSync",
            "OVRLipSyncContext",
            "OVRLipSyncContextBase",
            "OVRLipSyncContextCanned",
            "OVRLipSyncContextMorphTarget",
            "OVRLipSyncContextTextureFlip",
            "ONSPReflectionZone",
            "OculusSpatializerUnity",
            "ONSPAmbisonicsNative",
            "ONSPAudioSource",
            "RootMotion.FinalIK.BipedIK",
            "RootMotion.FinalIK.FingerRig",
            "RootMotion.FinalIK.Grounder",
            "RootMotion.FinalIK.GrounderBipedIK",
            "RootMotion.FinalIK.GrounderFBBIK",
            "RootMotion.FinalIK.GrounderIK",
            "RootMotion.FinalIK.GrounderQuadruped",
            "RootMotion.FinalIK.GrounderVRIK",
            "RootMotion.FinalIK.AimIK",
            "RootMotion.FinalIK.CCDIK",
            "RootMotion.FinalIK.FABRIK",
            "RootMotion.FinalIK.FABRIKRoot",
            "RootMotion.FinalIK.FullBodyBipedIK",
            "RootMotion.FinalIK.IK",
            "RootMotion.FinalIK.IKExecutionOrder",
            "RootMotion.FinalIK.LegIK",
            "RootMotion.FinalIK.LimbIK",
            "RootMotion.FinalIK.LookAtIK",
            "RootMotion.FinalIK.TrigonometricIK",
            "RootMotion.FinalIK.VRIK",
            "RootMotion.FinalIK.FBBIKArmBending",
            "RootMotion.FinalIK.FBBIKHeadEffector",
            "RootMotion.FinalIK.TwistRelaxer",
            "RootMotion.FinalIK.InteractionObject",
            "RootMotion.FinalIK.InteractionSystem",
            "RootMotion.FinalIK.InteractionTarget",
            "RootMotion.FinalIK.InteractionTrigger",
            "RootMotion.FinalIK.GenericPoser",
            "RootMotion.FinalIK.HandPoser",
            "RootMotion.FinalIK.Poser",
            "RootMotion.FinalIK.RagdollUtility",
            "RootMotion.FinalIK.RotationLimit",
            "RootMotion.FinalIK.RotationLimitAngle",
            "RootMotion.FinalIK.RotationLimitHinge",
            "RootMotion.FinalIK.RotationLimitPolygonal",
            "RootMotion.FinalIK.RotationLimitSpline",
            "RootMotion.FinalIK.AimPoser",
            "RootMotion.FinalIK.Amplifier",
            "RootMotion.FinalIK.BodyTilt",
            "RootMotion.FinalIK.HitReaction",
            "RootMotion.FinalIK.HitReactionVRIK",
            "RootMotion.FinalIK.Inertia",
            "RootMotion.FinalIK.OffsetModifier",
            "RootMotion.FinalIK.OffsetModifierVRIK",
            "RootMotion.FinalIK.OffsetPose",
            "RootMotion.FinalIK.Recoil",
            "RootMotion.FinalIK.ShoulderRotator",
            "RootMotion.Dynamics.AnimationBlocker",
            "RootMotion.Dynamics.BehaviourBase",
            "RootMotion.Dynamics.BehaviourFall",
            "RootMotion.Dynamics.BehaviourPuppet",
            "RootMotion.Dynamics.JointBreakBroadcaster",
            "RootMotion.Dynamics.MuscleCollisionBroadcaster",
            "RootMotion.Dynamics.PressureSensor",
            "RootMotion.Dynamics.Prop",
            "RootMotion.Dynamics.PropRoot",
            "RootMotion.Dynamics.PuppetMaster",
            "RootMotion.Dynamics.PuppetMasterSettings",
            // TODO: remove these if they are only needed in editor
            "RootMotion.Dynamics.BipedRagdollCreator",
            "RootMotion.Dynamics.RagdollCreator",
            "RootMotion.Dynamics.RagdollEditor",
            //
            "RootMotion.SolverManager",
            "RootMotion.TriggerEventBroadcaster",
            "UnityEngine.WindZone",
            "UnityEngine.Tilemaps.Tilemap",
            "UnityEngine.Tilemaps.TilemapRenderer",
            "UnityEngine.Terrain",
            "UnityEngine.Tree",
            "UnityEngine.SpriteMask",
            "UnityEngine.Grid",
            "UnityEngine.GridLayout",
            "UnityEngine.AudioSource",
            "UnityEngine.AudioReverbZone",
            "UnityEngine.AudioLowPassFilter",
            "UnityEngine.AudioHighPassFilter",
            "UnityEngine.AudioDistortionFilter",
            "UnityEngine.AudioEchoFilter",
            "UnityEngine.AudioChorusFilter",
            "UnityEngine.AudioReverbFilter",
            "UnityEngine.Playables.PlayableDirector",
            "UnityEngine.TerrainCollider",
            "UnityEngine.Canvas",
            "UnityEngine.CanvasGroup",
            "UnityEngine.CanvasRenderer",
            "UnityEngine.TextMesh",
            "UnityEngine.Animator",
            "UnityEngine.AI.NavMeshAgent",
            "UnityEngine.AI.NavMeshObstacle",
            "UnityEngine.AI.OffMeshLink",
            "UnityEngine.Cloth",
            "UnityEngine.WheelCollider",
            "UnityEngine.Rigidbody",
            "UnityEngine.Joint",
            "UnityEngine.HingeJoint",
            "UnityEngine.SpringJoint",
            "UnityEngine.FixedJoint",
            "UnityEngine.CharacterJoint",
            "UnityEngine.ConfigurableJoint",
            "UnityEngine.ConstantForce",
            "UnityEngine.Collider",
            "UnityEngine.BoxCollider",
            "UnityEngine.SphereCollider",
            "UnityEngine.MeshCollider",
            "UnityEngine.CapsuleCollider",
            "UnityEngine.CharacterController",
            "UnityEngine.ParticleSystem",
            "UnityEngine.ParticleSystemRenderer",
            "UnityEngine.BillboardRenderer",
            "UnityEngine.Camera",
            "UnityEngine.FlareLayer",
            "UnityEngine.SkinnedMeshRenderer",
            "UnityEngine.Renderer",
            "UnityEngine.TrailRenderer",
            "UnityEngine.LineRenderer",
            "UnityEngine.GUIElement",
            "UnityEngine.GUILayer",
            "UnityEngine.Light",
            "UnityEngine.LightProbeGroup",
            "UnityEngine.LightProbeProxyVolume",
            "UnityEngine.LODGroup",
            "UnityEngine.ReflectionProbe",
            "UnityEngine.SpriteRenderer",
            "UnityEngine.Transform",
            "UnityEngine.RectTransform",
            "UnityEngine.Rendering.SortingGroup",
            "UnityEngine.Projector",
            "UnityEngine.OcclusionPortal",
            "UnityEngine.OcclusionArea",
            "UnityEngine.LensFlare",
            "UnityEngine.Skybox",
            "UnityEngine.MeshFilter",
            "UnityEngine.Halo",
            "UnityEngine.MeshRenderer",
            "UnityEngine.Collider2D",
            "UnityEngine.Rigidbody2D",
            "UnityEngine.CompositeCollider2D",
            "UnityEngine.ConstantForce2D",
            "UnityEngine.AreaEffector2D",
            "UnityEngine.CapsuleCollider2D",
            "UnityEngine.DistanceJoint2D",
            "UnityEngine.EdgeCollider2D",
            "UnityEngine.Effector2D",
            "UnityEngine.BoxCollider2D",
            "UnityEngine.CircleCollider2D",
            "UnityEngine.FixedJoint2D",
            "UnityEngine.HingeJoint2D",
            "UnityEngine.FrictionJoint2D",
            "UnityEngine.PlatformEffector2D",
            "UnityEngine.PointEffector2D",
            "UnityEngine.PolygonCollider2D",
            "UnityEngine.SliderJoint2D",
            "UnityEngine.SurfaceEffector2D",
            "UnityEngine.RelativeJoint2D",
            "UnityEngine.TargetJoint2D",
            "UnityEngine.WheelJoint2D",
            "UnityEngine.Joint2D",
            "UnityEngine.ParticleSystemForceField"
        };

        static readonly string[] ComponentTypeWhiteListSdk2 = new string[]
        {
            #if UNITY_STANDALONE
            "VRCSDK2.VRC_CustomRendererBehaviour",
            "VRCSDK2.VRC_MidiNoteIn",
            "VRCSDK2.scripts.Scenes.VRC_Panorama",
            "VRCSDK2.VRC_Water",
            "UnityStandardAssets.Water.WaterBasic",
            "UnityStandardAssets.Water.Displace",
            "UnityStandardAssets.Water.GerstnerDisplace",
            "UnityStandardAssets.Water.PlanarReflection",
            "UnityStandardAssets.Water.SpecularLighting",
            "UnityStandardAssets.Water.Water",
            "UnityStandardAssets.Water.WaterBase",
            "UnityStandardAssets.Water.WaterTile",
            #endif
            "VRCSDK2.VRCTriggerRelay",
            "VRCSDK2.VRC_AudioBank",
            "VRCSDK2.VRC_DataStorage",
            "VRCSDK2.VRC_EventHandler",
            "VRCSDK2.VRC_IKFollower",
            "VRCSDK2.VRC_Label",
            "VRCSDK2.VRC_KeyEvents",
            "VRCSDK2.VRC_PhysicsRoot",
            "VRCSDK2.VRC_CombatSystem",
            "VRCSDK2.VRC_DestructibleStandard",
            "VRC_VisualDamage",
            "VRCSDK2.VRC_OscButtonIn",
            "VRCSDK2.VRC_GunStats",
            "VRCSDK2.VRC_JukeBox",
            "VRCSDK2.VRC_AddDamage",
            "VRCSDK2.VRC_AddHealth",
            "VRCSDK2.VRC_AvatarCalibrator",
            "VRCSDK2.VRC_AvatarPedestal",
            "VRCSDK2.VRC_NPCSpawn",
            "VRCSDK2.VRC_ObjectSpawn",
            "VRCSDK2.VRC_ObjectSync",
            "VRCSDK2.VRC_Pickup",
            "VRCSDK2.VRC_PortalMarker",
            "VRCSDK2.VRC_SlideShow",
            "VRCSDK2.VRC_SpatialAudioSource",
            "VRCSDK2.VRC_StationInput",
            "VRCSDK2.VRC_SyncAnimation",
            "VRCSDK2.VRC_SyncVideoPlayer",
            "VRCSDK2.VRC_SyncVideoStream",
            "VRCSDK2.VRC_VideoScreen",
            "VRCSDK2.VRC_VideoSpeaker",
            "VRCSDK2.VRC_PlayerAudioOverride",
            "VRCSDK2.VRC_MirrorReflection",
            "VRCSDK2.VRC_PlayerMods",
            "VRCSDK2.VRC_SceneDescriptor",
            "VRCSDK2.VRC_SceneResetPosition",
            "VRCSDK2.VRC_SceneSmoothShift",
            "VRCSDK2.VRC_SpecialLayer",
            "VRCSDK2.VRC_Station",
            "VRCSDK2.VRC_StereoObject",
            "VRCSDK2.VRC_TimedEvents",
            "VRCSDK2.VRC_Trigger",
            "VRCSDK2.VRC_TriggerColliderEventTrigger",
            "VRCSDK2.VRC_UseEvents",
            "VRCSDK2.VRC_UiShape",
            "UnityEngine.Animation",
            #if !UNITY_2019_4_OR_NEWER
            "UnityEngine.GUIText",
            "UnityEngine.GUITexture",
            #endif
            "UnityEngine.Video.VideoPlayer",
            "PhysSound.PhysSoundBase",
            "PhysSound.PhysSoundObject",
            "PhysSound.PhysSoundTempAudio",
            "PhysSound.PhysSoundTempAudioPool",
            "PhysSound.PhysSoundTerrain",
            "RealisticEyeMovements.EyeAndHeadAnimator",
            "RealisticEyeMovements.LookTargetController",
            "UnityStandardAssets.Cameras.AbstractTargetFollower",
            "UnityStandardAssets.Cameras.AutoCam",
            "UnityStandardAssets.Cameras.FreeLookCam",
            "UnityStandardAssets.Cameras.HandHeldCam",
            "UnityStandardAssets.Cameras.LookatTarget",
            "UnityStandardAssets.Cameras.PivotBasedCameraRig",
            "UnityStandardAssets.Cameras.ProtectCameraFromWallClip",
            "UnityStandardAssets.Cameras.TargetFieldOfView",
            "UnityStandardAssets.Characters.FirstPerson.FirstPersonController",
            "UnityStandardAssets.Characters.FirstPerson.HeadBob",
            "UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController",
            "UnityStandardAssets.Vehicles.Ball.Ball",
            "UnityStandardAssets.Vehicles.Ball.BallUserControl",
            "UnityStandardAssets.Characters.ThirdPerson.AICharacterControl",
            "UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter",
            "UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl",
            "UnityStandardAssets.CrossPlatformInput.AxisTouchButton",
            "UnityStandardAssets.CrossPlatformInput.ButtonHandler",
            "UnityStandardAssets.CrossPlatformInput.InputAxisScrollbar",
            "UnityStandardAssets.CrossPlatformInput.Joystick",
            "UnityStandardAssets.CrossPlatformInput.MobileControlRig",
            "UnityStandardAssets.CrossPlatformInput.TiltInput",
            "UnityStandardAssets.CrossPlatformInput.TouchPad",
            "UnityStandardAssets.Effects.AfterburnerPhysicsForce",
            "UnityStandardAssets.Effects.ExplosionFireAndDebris",
            "UnityStandardAssets.Effects.ExplosionPhysicsForce",
            "UnityStandardAssets.Effects.Explosive",
            "UnityStandardAssets.Effects.ExtinguishableParticleSystem",
            "UnityStandardAssets.Effects.FireLight",
            "UnityStandardAssets.Effects.Hose",
            "UnityStandardAssets.Effects.ParticleSystemMultiplier",
            "UnityStandardAssets.Effects.SmokeParticles",
            "UnityStandardAssets.Effects.WaterHoseParticles",
            "UnityStandardAssets.Utility.ActivateTrigger",
            "UnityStandardAssets.Utility.AutoMoveAndRotate",
            "UnityStandardAssets.Utility.DragRigidbody",
            "UnityStandardAssets.Utility.DynamicShadowSettings",
            "UnityStandardAssets.Utility.FollowTarget",
            "UnityStandardAssets.Utility.FPSCounter",
            "UnityStandardAssets.Utility.ObjectResetter",
            "UnityStandardAssets.Utility.ParticleSystemDestroyer",
            #if !UNITY_2019_4_OR_NEWER
            "UnityStandardAssets.Utility.SimpleActivatorMenu",
            #endif
            "UnityStandardAssets.Utility.SimpleMouseRotator",
            "UnityStandardAssets.Utility.SmoothFollow",
            "UnityStandardAssets.Utility.TimedObjectActivator",
            "UnityStandardAssets.Utility.TimedObjectDestructor",
            "UnityStandardAssets.Utility.WaypointCircuit",
            "UnityStandardAssets.Utility.WaypointProgressTracker",
            "UnityStandardAssets.Vehicles.Aeroplane.AeroplaneAiControl",
            "UnityStandardAssets.Vehicles.Aeroplane.AeroplaneAudio",
            "UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController",
            "UnityStandardAssets.Vehicles.Aeroplane.AeroplaneControlSurfaceAnimator",
            "UnityStandardAssets.Vehicles.Aeroplane.AeroplanePropellerAnimator",
            "UnityStandardAssets.Vehicles.Aeroplane.AeroplaneUserControl2Axis",
            "UnityStandardAssets.Vehicles.Aeroplane.AeroplaneUserControl4Axis",
            "UnityStandardAssets.Vehicles.Aeroplane.JetParticleEffect",
            "UnityStandardAssets.Vehicles.Aeroplane.LandingGear",
            "UnityStandardAssets.Vehicles.Car.BrakeLight",
            "UnityStandardAssets.Vehicles.Car.CarAIControl",
            "UnityStandardAssets.Vehicles.Car.CarAudio",
            "UnityStandardAssets.Vehicles.Car.CarController",
            "UnityStandardAssets.Vehicles.Car.CarSelfRighting",
            "UnityStandardAssets.Vehicles.Car.CarUserControl",
            "UnityStandardAssets.Vehicles.Car.Mudguard",
            "UnityStandardAssets.Vehicles.Car.SkidTrail",
            "UnityStandardAssets.Vehicles.Car.Suspension",
            "UnityStandardAssets.Vehicles.Car.WheelEffects",
            "RenderHeads.Media.AVProVideo.ApplyToMaterial",
            "RenderHeads.Media.AVProVideo.ApplyToMesh",
            "RenderHeads.Media.AVProVideo.AudioOutput",
            "RenderHeads.Media.AVProVideo.CubemapCube",
            "RenderHeads.Media.AVProVideo.DebugOverlay",
            "RenderHeads.Media.AVProVideo.DisplayBackground",
            "RenderHeads.Media.AVProVideo.DisplayIMGUI",
            "RenderHeads.Media.AVProVideo.DisplayUGUI",
            "RenderHeads.Media.AVProVideo.MediaPlayer",
            "RenderHeads.Media.AVProVideo.StreamParser",
            "RenderHeads.Media.AVProVideo.SubtitlesUGUI",
            "RenderHeads.Media.AVProVideo.UpdateStereoMaterial",
            "AlphaButtonClickMask",
            "EventSystemChecker",
            "VirtualMarketplaceItem",
            "SDK2UrlLauncher"
        };

        static readonly string[] ComponentTypeWhiteListSdk3 = new string[]
        {
            "VRC.SDK3.VRCDestructibleStandard",
            "VRC.SDK3.Components.VRCVisualDamage",
            "VRC.SDK3.Components.VRCAvatarPedestal",
            "VRC.SDK3.Components.VRCPickup",
            "VRC.SDK3.Components.VRCPortalMarker",
            "VRC.SDK3.Components.VRCSpatialAudioSource",
            "VRC.SDK3.Components.VRCMirrorReflection",
            "VRC.SDK3.Components.VRCSceneDescriptor",
            "VRC.SDK3.Components.VRCStation",
            "VRC.SDK3.Components.VRCUiShape",
            "VRC.SDK3.Components.VRCObjectSync",
            "VRC.SDK3.Components.VRCObjectPool",
            "VRC.SDK3.Video.Components.VRCUnityVideoPlayer",
            "VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer",
            "VRC.SDK3.Video.Components.AVPro.VRCAVProVideoScreen",
            "VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker",
            "VRC.SDK3.Midi.VRCMidiListener",
            "VRC.Udon.UdonBehaviour",
            "VRC.Udon.AbstractUdonBehaviourEventProxy",
            "UnityEngine.Animations.AimConstraint",
            "UnityEngine.Animations.LookAtConstraint",
            "UnityEngine.Animations.ParentConstraint",
            "UnityEngine.Animations.PositionConstraint",
            "UnityEngine.Animations.RotationConstraint",
            "UnityEngine.Animations.ScaleConstraint",
            "UnityEngine.ParticleSystemForceField",
            "Cinemachine.Cinemachine3rdPersonAim",
            "Cinemachine.CinemachineBlendListCamera",
            "Cinemachine.CinemachineBrain",
            "Cinemachine.CinemachineCameraOffset",
            "Cinemachine.CinemachineClearShot",
            "Cinemachine.CinemachineCollider",
            "Cinemachine.CinemachineConfiner",
            "Cinemachine.CinemachineDollyCart",
            "Cinemachine.CinemachineExternalCamera",
            "Cinemachine.CinemachineFollowZoom",
            "Cinemachine.CinemachineFreeLook",
            "Cinemachine.CinemachineMixingCamera",
            "Cinemachine.CinemachinePath",
            "Cinemachine.CinemachinePipeline",
            "Cinemachine.CinemachinePixelPerfect",
            "Cinemachine.CinemachineRecomposer",
            "Cinemachine.CinemachineSmoothPath",
            "Cinemachine.CinemachineStateDrivenCamera",
            "Cinemachine.CinemachineStoryboard",
            "Cinemachine.CinemachineTargetGroup",
            "Cinemachine.CinemachineVirtualCamera",
            "Cinemachine.Cinemachine3rdPersonFollow",
            "Cinemachine.CinemachineBasicMultiChannelPerlin",
            "Cinemachine.CinemachineComposer",
            "Cinemachine.CinemachineFramingTransposer",
            "Cinemachine.CinemachineGroupComposer",
            "Cinemachine.CinemachineHardLockToTarget",
            "Cinemachine.CinemachineHardLookAt",
            "Cinemachine.CinemachineOrbitalTransposer",
            "Cinemachine.CinemachinePOV",
            "Cinemachine.CinemachineSameAsFollowTarget",
            "Cinemachine.CinemachineTrackedDolly",
            "Cinemachine.CinemachineTransposer",
            "Cinemachine.CinemachineCore"
        };

        public static readonly string[] ShaderWhiteList = new string[]
        {
            "VRChat/Mobile/Standard Lite",
            "VRChat/Mobile/Diffuse",
            "VRChat/Mobile/Bumped Diffuse",
            "VRChat/Mobile/Bumped Mapped Specular",
            "VRChat/Mobile/Toon Lit",
            "VRChat/Mobile/MatCap Lit",
            "VRChat/Mobile/Lightmapped",
            "VRChat/Mobile/Skybox",
            "VRChat/Mobile/Particles/Additive",
            "VRChat/Mobile/Particles/Multiply",
            "FX/MirrorReflection",
            "UI/Default",
        };

        private static readonly HashSet<int> scannedObjects = new HashSet<int>();

        private static void ConfigureWhiteList(WhiteListConfiguration config)
        {
            if(ComponentTypeWhiteListConfiguration == config ||
               config == WhiteListConfiguration.Unchanged)
            {
                return;
            }

            List<string> concatenation = new List<string>();
            concatenation.AddRange(ComponentTypeWhiteListCommon);

            switch(config)
            {
                case WhiteListConfiguration.VRCSDK2:
                    concatenation.AddRange(ComponentTypeWhiteListSdk2);
                    break;
                case WhiteListConfiguration.VRCSDK3:
                    concatenation.AddRange(ComponentTypeWhiteListSdk3);
                    break;
            }

            ComponentTypeWhiteListConfiguration = config;
            ComponentTypeWhiteList = concatenation.ToArray();
        }

        [PublicAPI]
        public static void RemoveIllegalComponents(List<GameObject> targets, WhiteListConfiguration config, bool retry = true, HashSet<Type> tagWhitelistedTypes = null)
        {
            ConfigureWhiteList(config);
            
            HashSet<Type> whitelist = ValidationUtils.WhitelistedTypes($"world{config}", ComponentTypeWhiteList);

            // combine whitelist types from world tags with cached whitelist
            if (tagWhitelistedTypes != null)
            {
                tagWhitelistedTypes.UnionWith(whitelist);
            }

            foreach(GameObject target in targets)
            {
                ValidationUtils.RemoveIllegalComponents(target, (tagWhitelistedTypes == null) ? whitelist : tagWhitelistedTypes, retry, true, true);
                SecurityScan(target);
                AddScanned(target);
            }
        }

        private static void AddScanned(GameObject obj)
        {
            if(obj == null)
                return;

            if(!scannedObjects.Contains(obj.GetInstanceID()))
                scannedObjects.Add(obj.GetInstanceID());

            for(int idx = 0; idx < obj.transform.childCount; ++idx)
                AddScanned(obj.transform.GetChild(idx)?.gameObject);
        }

        private static bool WasScanned(GameObject obj)
        {
            return scannedObjects.Contains(obj.GetInstanceID());
        }

        [PublicAPI]
        public static void ScanGameObject(GameObject target, WhiteListConfiguration config)
        {
            if(WasScanned(target))
            {
                return;
            }

            ConfigureWhiteList(config);
            HashSet<Type> whitelist = ValidationUtils.WhitelistedTypes("world" + config, ComponentTypeWhiteList);
            ValidationUtils.RemoveIllegalComponents(target, whitelist);
            SecurityScan(target);
            AddScanned(target);

            // Must be called after AddScanned to avoid infinite recursion.
            ScanDropdownTemplates(target, config);
        }

        [PublicAPI]
        public static void ClearScannedGameObjectCache()
        {
            scannedObjects.Clear();
        }

        [PublicAPI]
        public static IEnumerable<Shader> FindIllegalShaders(GameObject target)
        {
            return ShaderValidation.FindIllegalShaders(target, ShaderWhiteList);
        }

        private static void SecurityScan(GameObject target)
        {
            PlayableDirector[] playableDirectors = target.GetComponentsInChildren<PlayableDirector>(true);
            foreach(PlayableDirector playableDirector in playableDirectors)
            {
                StripPlayableDirectorWithPrefabs(playableDirector);
            }
        }

        private static void ScanDropdownTemplates(GameObject target, WhiteListConfiguration config)
        {
            Dropdown[] dropdowns = target.GetComponentsInChildren<Dropdown>(true);
            foreach(Dropdown dropdown in dropdowns)
            {
                if(dropdown == null)
                {
                    continue;
                }

                RectTransform dropdownTemplate = dropdown.template;
                if(dropdownTemplate == null)
                {
                    continue;
                }

                ScanGameObject(dropdownTemplate.transform.root.gameObject, config);
            }
            
            #if TextMeshPro
            TMP_Dropdown[] tmpDropdowns = target.GetComponentsInChildren<TMP_Dropdown>(true);
            foreach(TMP_Dropdown textMeshProDropdown in tmpDropdowns)
            {
                if(textMeshProDropdown == null)
                {
                    continue;
                }

                RectTransform dropdownTemplate = textMeshProDropdown.template;
                if(dropdownTemplate == null)
                {
                    continue;
                }

                ScanGameObject(dropdownTemplate.transform.root.gameObject, config);
            }
            #endif
        }

        private static void StripPlayableDirectorWithPrefabs(PlayableDirector playableDirector)
        {
            if(!(playableDirector.playableAsset is UnityEngine.Timeline.TimelineAsset timelineAsset))
                return;

            IEnumerable<TrackAsset> tracks = timelineAsset.GetOutputTracks();
            foreach(TrackAsset track in tracks)
            {
                if(!(track is ControlTrack))
                    continue;

                IEnumerable<TimelineClip> clips = track.GetClips();
                foreach(TimelineClip clip in clips)
                {
                    if(clip.asset is ControlPlayableAsset controlPlayableAsset && controlPlayableAsset.prefabGameObject != null)
                    {
                        UnityEngine.Object.Destroy(playableDirector);
                        VRC.Core.Logger.LogWarning("PlayableDirector containing prefab removed", DebugLevel, playableDirector.gameObject);
                    }
                }
            }
        }
    }
}
