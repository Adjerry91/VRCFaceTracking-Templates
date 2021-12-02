using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using VRC.SDKBase;
using VRC.SDKBase.Validation;

// ReSharper disable RedundantNameQualifier

namespace VRC.SDK3.Validation
{
    public static class AvatarValidation
    {
        private const int MAX_STATIONS_PER_AVATAR = 6;
        private const float MAX_STATION_ACTIVATE_DISTANCE = 0f;
        private const float MAX_STATION_LOCATION_DISTANCE = 2f;
        private const float MAX_STATION_COLLIDER_DIMENSION = 2f;

        private static ProfilerMarker _clampRenderQueuesProfilerMarker = new ProfilerMarker("AvatarValidation.ClampRenderQueues");
        private static readonly List<Material> _clampRenderQueuesMaterialsTempList = new List<Material>();

        public static readonly string[] ComponentTypeWhiteListCommon = new string[]
        {
            #if UNITY_STANDALONE
            "DynamicBone",
            "DynamicBoneCollider",
            "RootMotion.FinalIK.IKExecutionOrder",
            "RootMotion.FinalIK.VRIK",
            "RootMotion.FinalIK.FullBodyBipedIK",
            "RootMotion.FinalIK.LimbIK",
            "RootMotion.FinalIK.AimIK",
            "RootMotion.FinalIK.BipedIK",
            "RootMotion.FinalIK.GrounderIK",
            "RootMotion.FinalIK.GrounderFBBIK",
            "RootMotion.FinalIK.GrounderVRIK",
            "RootMotion.FinalIK.GrounderQuadruped",
            "RootMotion.FinalIK.TwistRelaxer",
            "RootMotion.FinalIK.ShoulderRotator",
            "RootMotion.FinalIK.FBBIKArmBending",
            "RootMotion.FinalIK.FBBIKHeadEffector",
            "RootMotion.FinalIK.FABRIK",
            "RootMotion.FinalIK.FABRIKChain",
            "RootMotion.FinalIK.FABRIKRoot",
            "RootMotion.FinalIK.CCDIK",
            "RootMotion.FinalIK.RotationLimit",
            "RootMotion.FinalIK.RotationLimitHinge",
            "RootMotion.FinalIK.RotationLimitPolygonal",
            "RootMotion.FinalIK.RotationLimitSpline",
            "UnityEngine.Cloth",
            "UnityEngine.Light",
            "UnityEngine.BoxCollider",
            "UnityEngine.SphereCollider",
            "UnityEngine.CapsuleCollider",
            "UnityEngine.Rigidbody",
            "UnityEngine.Joint",
            "UnityEngine.Animations.AimConstraint",
            "UnityEngine.Animations.LookAtConstraint",
            "UnityEngine.Animations.ParentConstraint",
            "UnityEngine.Animations.PositionConstraint",
            "UnityEngine.Animations.RotationConstraint",
            "UnityEngine.Animations.ScaleConstraint",
            "UnityEngine.Camera",
            "UnityEngine.AudioSource",
            "ONSPAudioSource",
            #endif
            #if !VRC_CLIENT
            "VRC.Core.PipelineSaver",
            #endif
            "VRC.Core.PipelineManager",
            "UnityEngine.Transform",
            "UnityEngine.Animator",
            "UnityEngine.SkinnedMeshRenderer",
            "LimbIK", // our limbik based on Unity ik
            "LoadingAvatarTextureAnimation",
            "UnityEngine.MeshFilter",
            "UnityEngine.MeshRenderer",
            "UnityEngine.Animation",
            "UnityEngine.ParticleSystem",
            "UnityEngine.ParticleSystemRenderer",
            "UnityEngine.TrailRenderer",
            "UnityEngine.FlareLayer",
            "UnityEngine.GUILayer",
            "UnityEngine.LineRenderer",
            "RealisticEyeMovements.EyeAndHeadAnimator",
            "RealisticEyeMovements.LookTargetController",
        };

        public static readonly string[] ComponentTypeWhiteListSdk2 = new string[]
        {
            #if UNITY_STANDALONE
            "VRCSDK2.VRC_SpatialAudioSource",
            #endif
            "VRCSDK2.VRC_AvatarDescriptor",
            "VRCSDK2.VRC_AvatarVariations",
            "VRCSDK2.VRC_IKFollower",
            "VRCSDK2.VRC_Station",
        };

        public static readonly string[] ComponentTypeWhiteListSdk3 = new string[]
        {
            #if UNITY_STANDALONE
            "VRC.SDK3.Avatars.Components.VRCSpatialAudioSource",
            #endif
            "VRC.SDK3.VRCTestMarker",
            "VRC.SDK3.Avatars.Components.VRCAvatarDescriptor",
            "VRC.SDK3.Avatars.Components.VRCStation",
        };

#pragma warning disable 0414

        private static string[] CombinedComponentTypeWhiteListSdk2 = null;
        private static string[] CombinedComponentTypeWhiteListSdk3 = null;

#pragma warning restore 0414

        public static readonly string[] ShaderWhiteList = new string[]
        {
            "VRChat/Mobile/Standard Lite",
            "VRChat/Mobile/Diffuse",
            "VRChat/Mobile/Bumped Diffuse",
            "VRChat/Mobile/Bumped Mapped Specular",
            "VRChat/Mobile/Toon Lit",
            "VRChat/Mobile/MatCap Lit",

            "VRChat/Mobile/Particles/Additive",
            "VRChat/Mobile/Particles/Multiply",
        };

        public static bool ps_limiter_enabled = false;
        public static int ps_max_particles = 50000;
        public static int ps_max_systems = 200;
        public static int ps_max_emission = 5000;
        public static int ps_max_total_emission = 40000;
        public static int ps_mesh_particle_divider = 50;
        public static int ps_mesh_particle_poly_limit = 50000;
        public static int ps_collision_penalty_high = 120;
        public static int ps_collision_penalty_med = 60;
        public static int ps_collision_penalty_low = 10;
        public static int ps_trails_penalty = 10;
        public static int ps_max_particle_force = 0; // can not be disabled

        private static HashSet<System.Type> GetWhitelistForSDK(GameObject avatar)
        {
            VRC.SDKBase.VRC_AvatarDescriptor descriptor = avatar.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>();

            #if VRC_SDK_VRCSDK2
            if(descriptor is VRCSDK2.VRC_AvatarDescriptor)
            {
                if(CombinedComponentTypeWhiteListSdk2 == null)
                {
                    List<string> concatenation = new List<string>(ComponentTypeWhiteListCommon);
                    concatenation.AddRange(ComponentTypeWhiteListSdk2);
                    CombinedComponentTypeWhiteListSdk2 = concatenation.ToArray();
                }

                return ValidationUtils.WhitelistedTypes("avatar-sdk2", CombinedComponentTypeWhiteListSdk2);
            }
            #endif
            #if VRC_SDK_VRCSDK3
            if(descriptor is VRC.SDK3.Avatars.Components.VRCAvatarDescriptor)
            {
                if(CombinedComponentTypeWhiteListSdk3 == null)
                {
                    List<string> concatenation = new List<string>(ComponentTypeWhiteListCommon);
                    concatenation.AddRange(ComponentTypeWhiteListSdk3);
                    CombinedComponentTypeWhiteListSdk3 = concatenation.ToArray();
                }

                return ValidationUtils.WhitelistedTypes("avatar-sdk3", CombinedComponentTypeWhiteListSdk3);
            }
            #endif
            //throw new System.Exception("Malformed avatar");
            // instead of exception, log error, and return empty whitelist
            Debug.LogError("Malformed avatar");
            return new HashSet<System.Type>();
        }

        public static void RemoveIllegalComponents(GameObject target, bool retry = true)
        {
            ValidationUtils.RemoveIllegalComponents(target, GetWhitelistForSDK(target), retry);
        }

        public static IEnumerable<Component> FindIllegalComponents(GameObject target)
        {
            return ValidationUtils.FindIllegalComponents(target, GetWhitelistForSDK(target));
        }

        private static ProfilerMarker _enforceAudioSourceLimitsProfilerMarker = new ProfilerMarker("AvatarValidation.EnforceAudioSourceLimits");

        public static void EnforceAudioSourceLimits(GameObject currentAvatar)
        {
            using(_enforceAudioSourceLimitsProfilerMarker.Auto())
            {
                if(currentAvatar == null)
                {
                    return;
                }

                Queue<GameObject> children = new Queue<GameObject>();
                if(currentAvatar != null)
                {
                    children.Enqueue(currentAvatar.gameObject);
                }

                while(children.Count > 0)
                {
                    GameObject child = children.Dequeue();
                    if(child == null)
                    {
                        continue;
                    }

                    int childCount = child.transform.childCount;
                    for(int idx = 0; idx < childCount; ++idx)
                    {
                        children.Enqueue(child.transform.GetChild(idx).gameObject);
                    }

                    #if VRC_CLIENT
                    if(child.GetComponent<USpeaker>() != null)
                    {
                        continue;
                    }
                    #endif

                    AudioSource[] sources = child.transform.GetComponents<AudioSource>();
                    if(sources == null || sources.Length <= 0)
                    {
                        continue;
                    }

                    AudioSource audioSource = sources[0];
                    if(audioSource == null)
                    {
                        continue;
                    }


                    #if VRC_CLIENT
                    audioSource.outputAudioMixerGroup = VRCAudioManager.GetAvatarGroup();
                    audioSource.priority = Mathf.Clamp(audioSource.priority, 200, 255);
                    #else
                    ProcessSpatialAudioSources(audioSource);
                    #endif //!VRC_CLIENT

                    if(sources.Length <= 1)
                    {
                        continue;
                    }

                    Debug.LogError("Disabling extra AudioSources on GameObject(" + child.name + "). Only one is allowed per GameObject.");
                    for(int i = 1; i < sources.Length; i++)
                    {
                        if(sources[i] == null)
                        {
                            Profiler.EndSample();
                            continue;
                        }

                        #if VRC_CLIENT
                        sources[i].enabled = false;
                        sources[i].clip = null;
                        #else
                        ValidationUtils.RemoveComponent(sources[i]);
                        #endif //!VRC_CLIENT
                    }
                }
            }
        }

        public static void EnforceClothLimits(GameObject avatarGameObject)
        {
            const int clothMaxSolverFrequency = 240;
            foreach(Cloth cloth in avatarGameObject.GetComponentsInChildren<Cloth>(true))
            {
                if(cloth.clothSolverFrequency > clothMaxSolverFrequency)
                {
                    cloth.clothSolverFrequency = clothMaxSolverFrequency;
                }
            }
        }

        #if VRC_CLIENT
        public static void EnforceAimIKLimits(GameObject avatarGameObject)
        {
            const int aimIKMaxSolverFrequency = 64;
            foreach (RootMotion.FinalIK.AimIK aimIK in avatarGameObject.GetComponentsInChildren<RootMotion.FinalIK.AimIK>(true))
            {
                if (aimIK.solver.maxIterations > aimIKMaxSolverFrequency)
                {
                    aimIK.solver.maxIterations = aimIKMaxSolverFrequency;
                }
            }
        }
        #endif

        #if !VRC_CLIENT
        private static void ProcessSpatialAudioSources(AudioSource audioSource)
        {
            #if VRC_SDK_VRCSDK2
            VRC_SpatialAudioSource vrcSpatialAudioSource2 = audioSource.gameObject.GetComponent<VRC_SpatialAudioSource>();
            if(vrcSpatialAudioSource2 == null)
            {
                // user has not yet added VRC_SpatialAudioSource (or ONSP)
                // so set up some defaults
                vrcSpatialAudioSource2 = audioSource.gameObject.AddComponent<VRC_SpatialAudioSource>();
                vrcSpatialAudioSource2.Gain = AudioManagerSettings.AvatarAudioMaxGain;
                vrcSpatialAudioSource2.Far = AudioManagerSettings.AvatarAudioMaxRange;
                vrcSpatialAudioSource2.Near = 0f;
                vrcSpatialAudioSource2.VolumetricRadius = 0f;
                vrcSpatialAudioSource2.EnableSpatialization = true;
                vrcSpatialAudioSource2.enabled = true;
                audioSource.spatialize = true;
                audioSource.priority = Mathf.Clamp(audioSource.priority, 200, 255);
                audioSource.bypassEffects = false;
                audioSource.bypassListenerEffects = false;
                audioSource.spatialBlend = 1f;
                audioSource.spread = 0;

                // user is allowed to change, but for now put a safe default
                audioSource.maxDistance = AudioManagerSettings.AvatarAudioMaxRange;
                audioSource.minDistance = audioSource.maxDistance / 500f;
                audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            }
            #elif VRC_SDK_VRCSDK3
            VRC.SDK3.Avatars.Components.VRCSpatialAudioSource vrcSpatialAudioSource2 = audioSource.gameObject.GetComponent<VRC.SDK3.Avatars.Components.VRCSpatialAudioSource>();
            if (vrcSpatialAudioSource2 == null)
            {
                // user has not yet added VRC_SpatialAudioSource (or ONSP)
                // so set up some defaults
                vrcSpatialAudioSource2 = audioSource.gameObject.AddComponent<VRC.SDK3.Avatars.Components.VRCSpatialAudioSource>();
                vrcSpatialAudioSource2.Gain = AudioManagerSettings.AvatarAudioMaxGain;
                vrcSpatialAudioSource2.Far = AudioManagerSettings.AvatarAudioMaxRange;
                vrcSpatialAudioSource2.Near = 0f;
                vrcSpatialAudioSource2.VolumetricRadius = 0f;
                vrcSpatialAudioSource2.EnableSpatialization = true;
                vrcSpatialAudioSource2.enabled = true;
                audioSource.spatialize = true;
                audioSource.priority = Mathf.Clamp(audioSource.priority, 200, 255);
                audioSource.bypassEffects = false;
                audioSource.bypassListenerEffects = false;
                audioSource.spatialBlend = 1f;
                audioSource.spread = 0;

                // user is allowed to change, but for now put a safe default
                audioSource.maxDistance = AudioManagerSettings.AvatarAudioMaxRange;
                audioSource.minDistance = audioSource.maxDistance / 500f;
                audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            }
            #endif
        }
        #endif

        public static void EnforceRealtimeParticleSystemLimits(Dictionary<ParticleSystem, int> particleSystems, bool includeDisabled = false, bool stopSystems = true)
        {
            float totalEmission = 0;
            ParticleSystem ps = null;
            int max = 0;
            int em_penalty = 1;
            ParticleSystem.EmissionModule em;
            float emission = 0;
            ParticleSystem.Burst[] bursts;

            foreach(KeyValuePair<ParticleSystem, int> kp in particleSystems)
            {
                if(kp.Key == null)
                    continue;

                if(!kp.Key.isPlaying && !includeDisabled)
                    continue;

                ps = kp.Key;
                max = kp.Value;
                em_penalty = 1;
                if(ps.collision.enabled)
                {
                    // particle force is always restricted (not dependent on ps_limiter_enabled)
                    var restrictedCollision = ps.collision;
                    restrictedCollision.colliderForce = ps_max_particle_force;

                    if(ps_limiter_enabled)
                    {
                        switch(ps.collision.quality)
                        {
                            case ParticleSystemCollisionQuality.High:
                                max = max / ps_collision_penalty_high;
                                em_penalty += 3;
                                break;
                            case ParticleSystemCollisionQuality.Medium:
                                max = max / ps_collision_penalty_med;
                                em_penalty += 2;
                                break;
                            case ParticleSystemCollisionQuality.Low:
                                max = max / ps_collision_penalty_low;
                                em_penalty += 2;
                                break;
                        }
                    }
                }

                if(ps_limiter_enabled && ps.trails.enabled)
                {
                    max = max / ps_trails_penalty;
                    em_penalty += 3;
                }

                if(ps_limiter_enabled && ps.emission.enabled)
                {
                    em = ps.emission;
                    emission = 0;
                    emission += GetCurveMax(em.rateOverTime);
                    emission += GetCurveMax(em.rateOverDistance);

                    bursts = new ParticleSystem.Burst[em.burstCount];
                    em.GetBursts(bursts);
                    for(int i = 0; i < bursts.Length; i++)
                    {
                        float adjMax = bursts[i].repeatInterval > 1 ? bursts[i].maxCount : bursts[i].maxCount * bursts[i].repeatInterval;
                        if(adjMax > ps_max_emission)
                            bursts[i].maxCount = (short)Mathf.Clamp(adjMax, 0, ps_max_emission);
                    }

                    em.SetBursts(bursts);

                    emission *= em_penalty;
                    totalEmission += emission;
                    if((emission > ps_max_emission || totalEmission > ps_max_total_emission) && stopSystems)
                    {
                        kp.Key.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        // Debug.LogWarning("Particle system named " + kp.Key.gameObject.name + " breached particle emission limits, it has been stopped");
                    }
                }

                if(ps_limiter_enabled && ps.main.maxParticles > Mathf.Clamp(max, 1, kp.Value))
                {
                    ParticleSystem.MainModule psm = ps.main;
                    psm.maxParticles = Mathf.Clamp(psm.maxParticles, 1, max);
                    if(stopSystems)
                        kp.Key.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                    Debug.LogWarning("Particle system named " + kp.Key.gameObject.name + " breached particle limits, it has been limited");
                }
            }
        }

        private static ProfilerMarker _enforceAvatarStationLimitsProfilerMarker = new ProfilerMarker("AvatarValidation.EnforceAudioSourceLimits");

        public static void EnforceAvatarStationLimits(GameObject currentAvatar)
        {
            using(_enforceAvatarStationLimitsProfilerMarker.Auto())
            {
                int stationCount = 0;
                foreach(VRC.SDKBase.VRCStation station in currentAvatar.gameObject.GetComponentsInChildren<VRC.SDKBase.VRCStation>(true))
                {
                    if(station == null)
                    {
                        continue;
                    }

                    #if VRC_CLIENT
                    VRC_StationInternal stationInternal = station.transform.GetComponent<VRC_StationInternal>();
                    #endif
                    if(stationCount < MAX_STATIONS_PER_AVATAR)
                    {
                        #if VRC_CLIENT
                        bool markedForDestruction = false;
                        #endif
                        // keep this station, but limit it
                        if(station.disableStationExit)
                        {
                            Debug.LogError("[" + currentAvatar.name + "]==> Stations on avatars cannot disable station exit. Re-enabled.");
                            station.disableStationExit = false;
                        }

                        if(station.stationEnterPlayerLocation != null)
                        {
                            if(Vector3.Distance(station.stationEnterPlayerLocation.position, station.transform.position) > MAX_STATION_LOCATION_DISTANCE)
                            {
                                #if VRC_CLIENT
                                markedForDestruction = true;
                                Debug.LogError(
                                    "[" + currentAvatar.name + "]==> Station enter location is too far from station (max dist=" + MAX_STATION_LOCATION_DISTANCE +
                                    "). Station disabled.");
                                #else
                                Debug.LogError("Station enter location is too far from station (max dist=" + MAX_STATION_LOCATION_DISTANCE + "). Station will be disabled at runtime.");
                                #endif
                            }

                            if(Vector3.Distance(station.stationExitPlayerLocation.position, station.transform.position) > MAX_STATION_LOCATION_DISTANCE)
                            {
                                #if VRC_CLIENT
                                markedForDestruction = true;
                                Debug.LogError(
                                    "[" + currentAvatar.name + "]==> Station exit location is too far from station (max dist=" + MAX_STATION_LOCATION_DISTANCE +
                                    "). Station disabled.");
                                #else
                                Debug.LogError("Station exit location is too far from station (max dist=" + MAX_STATION_LOCATION_DISTANCE + "). Station will be disabled at runtime.");
                                #endif
                            }

                            #if VRC_CLIENT
                            if(markedForDestruction)
                            {
                                
                                ValidationUtils.RemoveComponent(station);
                                if(stationInternal != null)
                                {
                                    ValidationUtils.RemoveComponent(stationInternal);
                                }
                            }
                            #endif
                        }
                    }
                    else
                    {
                        #if VRC_CLIENT
                        Debug.LogError("[" + currentAvatar.name + "]==> Removing station over limit of " + MAX_STATIONS_PER_AVATAR);
                        ValidationUtils.RemoveComponent(station);
                        if(stationInternal != null)
                        {
                            ValidationUtils.RemoveComponent(stationInternal);
                        }
                        #else
                        Debug.LogError("Too many stations on avatar(" + currentAvatar.name + "). Maximum allowed=" + MAX_STATIONS_PER_AVATAR + ". Extra stations will be removed at runtime.");
                        #endif
                    }

                    stationCount++;
                }
            }
        }

        public static void RemoveCameras(GameObject currentAvatar, bool localPlayer, bool friend)
        {
            if(!localPlayer && currentAvatar != null)
            {
                foreach(Camera camera in currentAvatar.GetComponentsInChildren<Camera>(true))
                {
                    if(camera == null || camera.gameObject == null)
                        continue;

                    Debug.LogWarning("Removing camera from " + camera.gameObject.name);

                    if(friend && camera.targetTexture != null)
                    {
                        camera.enabled = false;
                    }
                    else
                    {
                        camera.enabled = false;
                        if(camera.targetTexture != null)
                            camera.targetTexture = new RenderTexture(16, 16, 24);

                        ValidationUtils.RemoveComponent(camera);
                    }
                }
            }
        }

        public static void StripAnimations(GameObject currentAvatar)
        {
            foreach(Animator anim in currentAvatar.GetComponentsInChildren<Animator>(true))
            {
                if(anim == null)
                    continue;

                StripRuntimeAnimatorController(anim.runtimeAnimatorController);
            }

            foreach(VRC.SDKBase.VRCStation station in currentAvatar.GetComponentsInChildren<VRC.SDKBase.VRCStation>(true))
            {
                if(station == null)
                    continue;

                StripRuntimeAnimatorController(station.animatorController);
            }
            #if VRC_SDK_VRCSDK3
            // also strip any controllers inside the av3 descriptor
            var desc3 = currentAvatar.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if(desc3 != null)
            {
                foreach(var layer in desc3.baseAnimationLayers)
                    StripRuntimeAnimatorController(layer.animatorController);

                foreach(var layer in desc3.specialAnimationLayers)
                    StripRuntimeAnimatorController(layer.animatorController);
            }
            #endif
        }

        private static void StripRuntimeAnimatorController(RuntimeAnimatorController rc)
        {
            if(rc == null || rc.animationClips == null)
                return;

            foreach(AnimationClip clip in rc.animationClips)
            {
                if(clip == null)
                    continue;

                if(clip.events != null && clip.events.Length > 0)
                    Debug.LogWarning("Removing animation events found on " + clip.name + " on animcontroller " + rc.name);

                clip.events = null;
            }
        }

        public static void RemoveExtraAnimationComponents(GameObject currentAvatar)
        {
            if(currentAvatar == null)
                return;

            // remove Animator comps
            {
                Animator mainAnimator = currentAvatar.GetComponent<Animator>();
                bool removeMainAnimator = false;
                if(mainAnimator != null)
                {
                    if(!mainAnimator.isHuman || mainAnimator.avatar == null || !mainAnimator.avatar.isValid)
                    {
                        removeMainAnimator = true;
                    }
                }

                foreach(Animator anim in currentAvatar.GetComponentsInChildren<Animator>(true))
                {
                    if(anim == null || anim.gameObject == null)
                        continue;

                    // exclude the main avatar animator
                    if(anim == mainAnimator)
                    {
                        if(!removeMainAnimator)
                        {
                            continue;
                        }
                    }

                    Debug.LogWarning("Removing Animator comp from " + anim.gameObject.name);

                    anim.enabled = false;
                    ValidationUtils.RemoveComponent(anim);
                }
            }

            ValidationUtils.RemoveComponentsOfType<UnityEngine.Animation>(currentAvatar);
        }

        private static Color32 GetTrustLevelColor(VRC.Core.APIUser user)
        {
            #if VRC_CLIENT
            Color32 color = new Color32(255, 255, 255, 255);
            if (user == null)
            {
                return color;
            }

            color = VRCPlayer.GetDisplayColorForSocialRank(user);
            return color;
            #else
            // we are in sdk, this is not meaningful anyway
            return (Color32)Color.grey;
            #endif
        }

        private static Material CreateFallbackMaterial(Material originalMaterial, VRC.Core.APIUser user)
        {
            #if VRC_CLIENT
            Material fallbackMaterial;
            Color trustCol = user != null ? (Color)GetTrustLevelColor(user) : Color.white;
            string displayName = user != null ? user.displayName : "localUser";

            if (originalMaterial == null || originalMaterial.shader == null)
            {
                fallbackMaterial = VRC.Core.AssetManagement.CreateMatCap(trustCol * 0.8f + new Color(0.2f, 0.2f, 0.2f));
                fallbackMaterial.name = string.Format("MC_{0}_{1}", fallbackMaterial.shader.name, displayName);
            }
            else
            {
                var safeShader = VRC.Core.AssetManagement.GetSafeShader(originalMaterial.shader.name);
                if (safeShader == null)
                {
                    fallbackMaterial = VRC.Core.AssetManagement.CreateSafeFallbackMaterial(originalMaterial, trustCol * 0.8f + new Color(0.2f, 0.2f, 0.2f));
                    fallbackMaterial.name = string.Format("FB_{0}_{1}_{2}", fallbackMaterial.shader.name, displayName, originalMaterial.name);
                }
                else
                {
                    //Debug.Log("<color=cyan>*** using safe internal fallback for shader:"+ safeShader.name + "</color>");
                    fallbackMaterial = new Material(safeShader);
                    if (safeShader.name == "Standard" || safeShader.name == "Standard (Specular setup)")
                    {
                        VRC.Core.AssetManagement.SetupBlendMode(fallbackMaterial);
                    }

                    fallbackMaterial.CopyPropertiesFromMaterial(originalMaterial);
                    fallbackMaterial.name = string.Format("INT_{0}_{1}_{2}", fallbackMaterial.shader.name, displayName, originalMaterial.name);
                }
            }

            return fallbackMaterial;
            #else
            // we are in sdk, this is not meaningful anyway
            return new Material(Shader.Find("Standard"));
            #endif
        }

        public static void BuildAvatarRenderersList(GameObject currentAvatar, List<Renderer> avatarRenderers)
        {
            currentAvatar.GetComponentsInChildren(true, avatarRenderers);
        }

        // TCL's method of allocation avoidance
        private static readonly List<Material> _replaceShadersWorkingList = new List<Material>();

        public static void ReplaceShaders(VRC.Core.APIUser user, List<Renderer> avatarRenderers, FallbackMaterialCache fallbackMaterialCache, bool debug = false)
        {
            foreach(Renderer avatarRenderer in avatarRenderers)
            {
                if(avatarRenderer == null)
                {
                    continue;
                }

                avatarRenderer.GetSharedMaterials(_replaceShadersWorkingList);
                bool anyReplaced = false;
                for(int i = 0; i < _replaceShadersWorkingList.Count; ++i)
                {
                    Material currentMaterial = _replaceShadersWorkingList[i];
                    if(currentMaterial == null)
                    {
                        continue;
                    }

                    // Check if the material has a cached fallback material if not then create a new one.
                    if(!fallbackMaterialCache.TryGetFallbackMaterial(currentMaterial, out Material fallbackMaterial))
                    {
                        fallbackMaterial = CreateFallbackMaterial(currentMaterial, user);

                        // Map the current material to the fallback and the fallback to itself.
                        fallbackMaterialCache.AddFallbackMaterial(currentMaterial, fallbackMaterial);
                        fallbackMaterialCache.AddFallbackMaterial(fallbackMaterial, fallbackMaterial);

                        if(debug)
                        {
                            Debug.Log($"<color=cyan>*** Creating new fallback: '{fallbackMaterial.shader.name}' </color>");
                        }

                        if(fallbackMaterial == currentMaterial)
                        {
                            continue;
                        }

                        _replaceShadersWorkingList[i] = fallbackMaterial;
                        anyReplaced = true;
                        continue;
                    }

                    // If the material is the fallback then we don't need to change it.
                    if(currentMaterial == fallbackMaterial)
                    {
                        continue;
                    }

                    if(debug)
                    {
                        Debug.Log($"<color=cyan>*** Using existing fallback: '{fallbackMaterial.shader.name}' </color>");
                    }

                    _replaceShadersWorkingList[i] = fallbackMaterial;
                    anyReplaced = true;
                }

                if(anyReplaced)
                {
                    avatarRenderer.sharedMaterials = _replaceShadersWorkingList.ToArray();
                }
            }
        }

        public static void ReplaceShadersRealtime(VRC.Core.APIUser user, List<Renderer> avatarRenderers, FallbackMaterialCache fallbackMaterialCache, bool debug = false)
        {
            ReplaceShaders(user, avatarRenderers, fallbackMaterialCache, debug);
        }

        public static void SetupParticleLimits()
        {
            ps_limiter_enabled = VRC.Core.ConfigManager.RemoteConfig.GetBool("ps_limiter_enabled", ps_limiter_enabled);
            ps_max_particles = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_max_particles", ps_max_particles);
            ps_max_systems = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_max_systems", ps_max_systems);
            ps_max_emission = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_max_emission", ps_max_emission);
            ps_max_total_emission = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_max_total_emission", ps_max_total_emission);
            ps_mesh_particle_divider = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_mesh_particle_divider", ps_mesh_particle_divider);
            ps_mesh_particle_poly_limit = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_mesh_particle_poly_limit", ps_mesh_particle_poly_limit);
            ps_collision_penalty_high = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_collision_penalty_high", ps_collision_penalty_high);
            ps_collision_penalty_med = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_collision_penalty_med", ps_collision_penalty_med);
            ps_collision_penalty_low = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_collision_penalty_low", ps_collision_penalty_low);
            ps_trails_penalty = VRC.Core.ConfigManager.RemoteConfig.GetInt("ps_trails_penalty", ps_trails_penalty);

            if(Application.isMobilePlatform)
            {
                ps_limiter_enabled = true;
            }
            else
            {
                ps_limiter_enabled = VRC.Core.ConfigManager.LocalConfig.GetList("betas").Contains("particle_system_limiter") || ps_limiter_enabled;
                ps_max_particles = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_max_particles", ps_max_particles);
                ps_max_systems = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_max_systems", ps_max_systems);
                ps_max_emission = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_max_emission", ps_max_emission);
                ps_max_total_emission = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_max_total_emission", ps_max_total_emission);
                ps_mesh_particle_divider = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_mesh_particle_divider", ps_mesh_particle_divider);
                ps_mesh_particle_poly_limit = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_mesh_particle_poly_limit", ps_mesh_particle_poly_limit);
                ps_collision_penalty_high = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_collision_penalty_high", ps_collision_penalty_high);
                ps_collision_penalty_med = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_collision_penalty_med", ps_collision_penalty_med);
                ps_collision_penalty_low = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_collision_penalty_low", ps_collision_penalty_low);
                ps_trails_penalty = VRC.Core.ConfigManager.LocalConfig.GetInt("ps_trails_penalty", ps_trails_penalty);
            }
        }

        public static Dictionary<ParticleSystem, int> EnforceParticleSystemLimits(GameObject currentAvatar)
        {
            Dictionary<ParticleSystem, int> particleSystems = new Dictionary<ParticleSystem, int>();

            foreach(ParticleSystem ps in currentAvatar.transform.GetComponentsInChildren<ParticleSystem>(true))
            {
                int realtime_max = ps_max_particles;

                // always limit collision force
                var collision = ps.collision;
                if(collision.colliderForce > ps_max_particle_force)
                {
                    collision.colliderForce = ps_max_particle_force;
                    Debug.LogError("Collision force is restricted on avatars, particle system named " + ps.gameObject.name + " collision force restricted to " + ps_max_particle_force);
                }

                if(ps_limiter_enabled)
                {
                    if(particleSystems.Count > ps_max_systems)
                    {
                        Debug.LogError("Too many particle systems, #" + particleSystems.Count + " named " + ps.gameObject.name + " deleted");
                        ValidationUtils.RemoveComponent(ps);
                        continue;
                    }
                    else
                    {
                        var main = ps.main;
                        var emission = ps.emission;

                        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
                        if(renderer != null)
                        {
                            if(renderer.renderMode == ParticleSystemRenderMode.Mesh)
                            {
                                Mesh[] meshes = new Mesh[0];
                                int highestPoly = 0;
                                renderer.GetMeshes(meshes);
                                if(meshes.Length == 0 && renderer.mesh != null)
                                {
                                    meshes = new Mesh[] {renderer.mesh};
                                }

                                // Debug.Log(meshes.Length + " meshes possible emmited meshes from " + ps.gameObject.name);
                                foreach(Mesh m in meshes)
                                {
                                    if(m.isReadable)
                                    {
                                        if(m.triangles.Length / 3 > highestPoly)
                                        {
                                            highestPoly = m.triangles.Length / 3;
                                        }
                                    }
                                    else
                                    {
                                        if(1000 > highestPoly)
                                        {
                                            highestPoly = int.MaxValue;
                                        }
                                    }
                                }

                                if(highestPoly > 0)
                                {
                                    highestPoly = Mathf.Clamp(highestPoly / ps_mesh_particle_divider, 1, highestPoly);
                                    realtime_max = Mathf.FloorToInt((float)realtime_max / highestPoly);

                                    if(highestPoly > ps_mesh_particle_poly_limit)
                                    {
                                        Debug.LogError("Particle system named " + ps.gameObject.name + " breached polygon limits, it has been deleted");
                                        ValidationUtils.RemoveComponent(ps);
                                        continue;
                                    }
                                }
                            }
                        }


                        ParticleSystem.MinMaxCurve rate = emission.rateOverTime;

                        if(rate.mode == ParticleSystemCurveMode.Constant)
                        {
                            rate.constant = Mathf.Clamp(rate.constant, 0, ps_max_emission);
                        }
                        else if(rate.mode == ParticleSystemCurveMode.TwoConstants)
                        {
                            rate.constantMax = Mathf.Clamp(rate.constantMax, 0, ps_max_emission);
                        }
                        else
                        {
                            rate.curveMultiplier = Mathf.Clamp(rate.curveMultiplier, 0, ps_max_emission);
                        }

                        emission.rateOverTime = rate;
                        rate = emission.rateOverDistance;

                        if(rate.mode == ParticleSystemCurveMode.Constant)
                        {
                            rate.constant = Mathf.Clamp(rate.constant, 0, ps_max_emission);
                        }
                        else if(rate.mode == ParticleSystemCurveMode.TwoConstants)
                        {
                            rate.constantMax = Mathf.Clamp(rate.constantMax, 0, ps_max_emission);
                        }
                        else
                        {
                            rate.curveMultiplier = Mathf.Clamp(rate.curveMultiplier, 0, ps_max_emission);
                        }

                        emission.rateOverDistance = rate;

                        //Disable collision with PlayerLocal layer
                        collision.collidesWith &= ~(1 << 10);
                    }
                }

                particleSystems.Add(ps, realtime_max);
            }

            EnforceRealtimeParticleSystemLimits(particleSystems, true, false);

            return particleSystems;
        }

        public static bool ClearLegacyAnimations(GameObject currentAvatar)
        {
            bool hasLegacyAnims = false;
            foreach(var ani in currentAvatar.GetComponentsInChildren<UnityEngine.Animation>(true))
            {
                if(ani.clip != null)
                    if(ani.clip.legacy)
                    {
                        Debug.LogWarningFormat("Legacy animation found named '{0}' on '{1}', removing", ani.clip.name, ani.gameObject.name);
                        ani.clip = null;
                        hasLegacyAnims = true;
                    }

                foreach(AnimationState anistate in ani)
                    if(anistate.clip.legacy)
                    {
                        Debug.LogWarningFormat("Legacy animation found named '{0}' on '{1}', removing", anistate.clip.name, ani.gameObject.name);
                        ani.RemoveClip(anistate.clip);
                        hasLegacyAnims = true;
                    }
            }

            return hasLegacyAnims;
        }

        private static float GetCurveMax(ParticleSystem.MinMaxCurve minMaxCurve)
        {
            switch(minMaxCurve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return minMaxCurve.constant;
                case ParticleSystemCurveMode.TwoConstants:
                    return minMaxCurve.constantMax;
                default:
                    return minMaxCurve.curveMultiplier;
            }
        }

        public static bool AreAnyParticleSystemsPlaying(Dictionary<ParticleSystem, int> particleSystems)
        {
            foreach(KeyValuePair<ParticleSystem, int> kp in particleSystems)
            {
                if(kp.Key != null && kp.Key.isPlaying)
                    return true;
            }

            return false;
        }

        public static void StopAllParticleSystems(Dictionary<ParticleSystem, int> particleSystems)
        {
            foreach(KeyValuePair<ParticleSystem, int> kp in particleSystems)
            {
                if(kp.Key != null && kp.Key.isPlaying)
                {
                    kp.Key.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        public static IEnumerable<Shader> FindIllegalShaders(GameObject target)
        {
            return ShaderValidation.FindIllegalShaders(target, ShaderWhiteList);
        }

        /// <summary>
        /// NOTE: intended to be called from 'VRCAvatarManager.SafetyCheckAndComponentScan'
        /// but temporarily disabled (until we enable texture streaming)
        /// </summary>  
        public static void ReportTexturesWithoutMipMapStreaming(VRC.Core.ApiAvatar avatar, GameObject target)
        {
            var badTextures = new List<Texture2D>();
            foreach(Renderer r in target.GetComponentsInChildren<Renderer>())
            {
                foreach(Material m in r.sharedMaterials)
                {
                    foreach(int i in m.GetTexturePropertyNameIDs())
                    {
                        Texture2D t = m.GetTexture(i) as Texture2D;
                        if(!t)
                            continue;

                        if((t.mipmapCount > 0) && !t.streamingMipmaps)
                            badTextures.Add(t);
                    }
                }
            }

            if(badTextures.Count > 0)
            {
                string warning = "[" + avatar.name + "]==> One or more avatar textures have non-streaming mipmaps: ";
                foreach(Texture2D t in badTextures)
                {
                    warning += "'" + t.name + "', ";
                }

                warning = warning.Remove(warning.LastIndexOf(",", StringComparison.Ordinal));
                Debug.LogWarning(warning + ".");
            }
        }

        public static void ClampRenderQueues(List<Renderer> avatarRenderers, int minimumRenderQueue, int maximumRenderQueue)
        {
            using(_clampRenderQueuesProfilerMarker.Auto())
            {
                foreach(Renderer avatarRenderer in avatarRenderers)
                {
                    if(avatarRenderer == null)
                    {
                        continue;
                    }

                    avatarRenderer.GetSharedMaterials(_clampRenderQueuesMaterialsTempList);
                    foreach(Material avatarSharedMaterial in _clampRenderQueuesMaterialsTempList)
                    {
                        if(avatarSharedMaterial == null)
                        {
                            continue;
                        }

                        int renderQueue = avatarSharedMaterial.renderQueue;
                        if(renderQueue < minimumRenderQueue)
                        {
                            avatarSharedMaterial.renderQueue = minimumRenderQueue;
                        }
                        else if(renderQueue > maximumRenderQueue)
                        {
                            avatarSharedMaterial.renderQueue = maximumRenderQueue;
                        }
                    }
                }
            }
        }
    }
}
