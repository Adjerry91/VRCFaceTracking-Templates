using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance
{
    public static class SDKPerformanceDisplay
    {
        public static void GetSDKPerformanceInfoText(
            AvatarPerformanceStats perfStats,
            AvatarPerformanceCategory perfCategory,
            out string text,
            out PerformanceInfoDisplayLevel displayLevel
        )
        {
            text = "";
            displayLevel = PerformanceInfoDisplayLevel.None;

            PerformanceRating rating = perfStats.GetPerformanceRatingForCategory(perfCategory);
            switch(perfCategory)
            {
                case AvatarPerformanceCategory.Overall:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Info;
                            text = string.Format("Overall Performance: {0}", AvatarPerformanceStats.GetPerformanceRatingDisplayName(rating));
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Overall Performance: {0} - This avatar may not perform well on many systems." +
                                " See additional warnings for suggestions on how to improve performance. Click 'Avatar Optimization Tips' below for more information.",
                                AvatarPerformanceStats.GetPerformanceRatingDisplayName(rating)
                            );

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            if(VRC.ValidationHelpers.IsMobilePlatform())
                            {
                                text = string.Format(
                                    "Overall Performance: {0} - This avatar does not meet minimum performance requirements for VRChat. " +
                                    "It will be blocked by default on VRChat for Quest, and will not show unless a user chooses to show your avatar." +
                                    " See additional warnings for suggestions on how to improve performance. Click 'Avatar Optimization Tips' below for more information.",
                                    AvatarPerformanceStats.GetPerformanceRatingDisplayName(rating));
                            }
                            else
                            {
                                text = string.Format(
                                    "Overall Performance: {0} - This avatar does not meet minimum performance requirements for VRChat. " +
                                    "It may be blocked by users depending on their Performance settings." +
                                    " See additional warnings for suggestions on how to improve performance. Click 'Avatar Optimization Tips' below for more information.",
                                    AvatarPerformanceStats.GetPerformanceRatingDisplayName(rating));
                            }

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.PolyCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Info;
                            text = string.Format("Polygons: {0}", perfStats.polyCount);
                            break;
                        }
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Info;
                            text = string.Format("Polygons: {0} (Recommended: {1})", perfStats.polyCount, AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).polyCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Polygons: {0} - Please try to reduce your avatar poly count to less than {1} (Recommended: {2})",
                                perfStats.polyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Good).polyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).polyCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Polygons: {0} - This avatar has too many polygons. " +
                                (VRC.ValidationHelpers.IsMobilePlatform()
                                    ? "It will be blocked by default on VRChat for Quest, and will not show unless a user chooses to show your avatar."
                                    : "It may be blocked by users depending on their Performance settings.") +
                                " It should have less than {1}. VRChat recommends having less than {2}.",
                                perfStats.polyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).polyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).polyCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.AABB:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Bounding box (AABB) size: {0}", perfStats.aabb.GetValueOrDefault().size.ToString());
                            break;
                        }
                        case PerformanceRating.Good:
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Bounding box (AABB) size: {0} (Recommended: {1})",
                                perfStats.aabb.GetValueOrDefault().size.ToString(),
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).aabb.size.ToString());

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "This avatar's bounding box (AABB) is too large on at least one axis. Current size: {0}, Maximum size: {1}",
                                perfStats.aabb.GetValueOrDefault().size.ToString(),
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).aabb.size.ToString());

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.SkinnedMeshCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Skinned Mesh Renderers: {0}", perfStats.skinnedMeshCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Skinned Mesh Renderers: {0} (Recommended: {1}) - Combine multiple skinned meshes for optimal performance.",
                                perfStats.skinnedMeshCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).skinnedMeshCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Skinned Mesh Renderers: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many skinned meshes." +
                                " Combine multiple skinned meshes for optimal performance.",
                                perfStats.skinnedMeshCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).skinnedMeshCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).skinnedMeshCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.MeshCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Mesh Renderers: {0}", perfStats.meshCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Mesh Renderers: {0} (Recommended: {1}) - Combine multiple meshes for optimal performance.",
                                perfStats.meshCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).meshCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Mesh Renderers: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many meshes. Combine multiple meshes for optimal performance.",
                                perfStats.meshCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).meshCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).meshCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.MaterialCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Material Slots: {0}", perfStats.materialCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Material Slots: {0} (Recommended: {1}) - Combine materials and atlas textures for optimal performance.",
                                perfStats.materialCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).materialCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Material Slots: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many materials. Combine materials and atlas textures for optimal performance.",
                                perfStats.materialCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).materialCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).materialCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.AnimatorCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Animator Count: {0}", perfStats.animatorCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Animator Count: {0} (Recommended: {1}) - Avoid using extra Animators for optimal performance.",
                                perfStats.animatorCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).animatorCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Animator Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many Animators. Avoid using extra Animators for optimal performance.",
                                perfStats.animatorCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).animatorCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).animatorCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.BoneCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Bones: {0}", perfStats.boneCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Bones: {0} (Recommended: {1}) - Reduce number of bones for optimal performance.",
                                perfStats.boneCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).boneCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Bones: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many bones. Reduce number of bones for optimal performance.",
                                perfStats.boneCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).boneCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).boneCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.LightCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Lights: {0}", perfStats.lightCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Lights: {0} (Recommended: {1}) - Avoid use of dynamic lights for optimal performance.",
                                perfStats.lightCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).lightCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Lights: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many dynamic lights. Avoid use of dynamic lights for optimal performance.",
                                perfStats.lightCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).lightCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).lightCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.ParticleSystemCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Particle Systems: {0}", perfStats.particleSystemCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Particle Systems: {0} (Recommended: {1}) - Reduce number of particle systems for better performance.",
                                perfStats.particleSystemCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).particleSystemCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Particle Systems: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many particle systems." +
                                " Reduce number of particle systems for better performance.",
                                perfStats.particleSystemCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).particleSystemCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).particleSystemCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.ParticleTotalCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Total Combined Max Particle Count: {0}", perfStats.particleTotalCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Total Combined Max Particle Count: {0} (Recommended: {1}) - Reduce 'Max Particles' across all particle systems for better performance.",
                                perfStats.particleTotalCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).particleTotalCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Total Combined Max Particle Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar uses too many particles." +
                                " Reduce 'Max Particles' across all particle systems for better performance.",
                                perfStats.particleTotalCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).particleTotalCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).particleTotalCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.ParticleMaxMeshPolyCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Mesh Particle Total Max Poly Count: {0}", perfStats.particleMaxMeshPolyCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Mesh Particle Total Max Poly Count: {0} (Recommended: {1}) - Reduce number of polygons in particle meshes, and reduce 'Max Particles' for better performance.",
                                perfStats.particleMaxMeshPolyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).particleMaxMeshPolyCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Mesh Particle Total Max Poly Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar uses too many mesh particle polygons." +
                                " Reduce number of polygons in particle meshes, and reduce 'Max Particles' for better performance.",
                                perfStats.particleMaxMeshPolyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).particleTotalCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).particleMaxMeshPolyCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.ParticleTrailsEnabled:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Particle Trails Enabled: {0}", perfStats.particleTrailsEnabled);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Particle Trails Enabled: {0} (Recommended: {1}) - Avoid particle trails for better performance.",
                                perfStats.particleTrailsEnabled,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).particleTrailsEnabled);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.ParticleCollisionEnabled:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Particle Collision Enabled: {0}", perfStats.particleCollisionEnabled);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Particle Collision Enabled: {0} (Recommended: {1}) - Avoid particle collision for better performance.",
                                perfStats.particleCollisionEnabled,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).particleCollisionEnabled);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.TrailRendererCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Trail Renderers: {0}", perfStats.trailRendererCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Trail Renderers: {0} (Recommended: {1}) - Reduce number of TrailRenderers for better performance.",
                                perfStats.trailRendererCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).trailRendererCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Trail Renderers: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many TrailRenderers. Reduce number of TrailRenderers for better performance.",
                                perfStats.trailRendererCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).trailRendererCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).trailRendererCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.LineRendererCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Line Renderers: {0}", perfStats.lineRendererCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Line Renderers: {0} (Recommended: {1}) - Reduce number of LineRenderers for better performance.",
                                perfStats.lineRendererCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).lineRendererCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Line Renderers: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many LineRenderers. Reduce number of LineRenderers for better performance.",
                                perfStats.lineRendererCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).lineRendererCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).lineRendererCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.DynamicBoneComponentCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Dynamic Bone Components: {0}", perfStats.dynamicBoneComponentCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Dynamic Bone Components: {0} (Recommended: {1}) - Reduce number of DynamicBone components for better performance.",
                                perfStats.dynamicBoneComponentCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).dynamicBoneComponentCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Dynamic Bone Components: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many DynamicBone components." +
                                " Reduce number of DynamicBone components for better performance.",
                                perfStats.dynamicBoneComponentCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).dynamicBoneComponentCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).dynamicBoneComponentCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.DynamicBoneSimulatedBoneCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Dynamic Bone Simulated Bone Count: {0}", perfStats.dynamicBoneSimulatedBoneCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Dynamic Bone Simulated Bone Count: {0} (Recommended: {1}) - " +
                                "Reduce number of transforms in hierarchy under DynamicBone components, or set EndLength or EndOffset to zero to reduce the number of simulated bones.",
                                perfStats.dynamicBoneSimulatedBoneCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).dynamicBoneSimulatedBoneCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Dynamic Bone Simulated Bone Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many bones simulated by DynamicBone." +
                                " Reduce number of transforms in hierarchy under DynamicBone components, or set EndLength or EndOffset to zero to reduce the number of simulated bones.",
                                perfStats.dynamicBoneSimulatedBoneCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).dynamicBoneSimulatedBoneCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).dynamicBoneSimulatedBoneCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.DynamicBoneColliderCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Dynamic Bone Collider Count: {0}", perfStats.dynamicBoneColliderCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Dynamic Bone Collider Count: {0} (Recommended: {1}) - Avoid use of DynamicBoneColliders for better performance.",
                                perfStats.dynamicBoneColliderCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).dynamicBoneColliderCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Dynamic Bone Collider Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many DynamicBoneColliders." +
                                " Avoid use of DynamicBoneColliders for better performance.",
                                perfStats.dynamicBoneColliderCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).dynamicBoneColliderCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).dynamicBoneColliderCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.DynamicBoneCollisionCheckCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Dynamic Bone Collision Check Count: {0}", perfStats.dynamicBoneCollisionCheckCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Dynamic Bone Collision Check Count: {0} (Recommended: {1}) - Avoid use of DynamicBoneColliders for better performance.",
                                perfStats.dynamicBoneCollisionCheckCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).dynamicBoneCollisionCheckCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Dynamic Bone Collision Check Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many DynamicBoneColliders." +
                                " Avoid use of DynamicBoneColliders for better performance.",
                                perfStats.dynamicBoneCollisionCheckCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).dynamicBoneCollisionCheckCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).dynamicBoneCollisionCheckCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.ClothCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Cloth Component Count: {0}", perfStats.clothCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Cloth Component Count: {0} (Recommended: {1}) - Avoid use of cloth for optimal performance.",
                                perfStats.clothCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).clothCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Cloth Component Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many Cloth components. Avoid use of cloth for optimal performance.",
                                perfStats.clothCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).clothCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).clothCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.ClothMaxVertices:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Cloth Total Vertex Count: {0}", perfStats.clothMaxVertices);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Cloth Total Vertex Count: {0} (Recommended: {1}) - Reduce number of vertices in cloth meshes for improved performance.",
                                perfStats.clothMaxVertices,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).clothMaxVertices);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Cloth Total Vertex Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many vertices in cloth meshes." +
                                " Reduce number of vertices in cloth meshes for improved performance.",
                                perfStats.clothMaxVertices,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).clothMaxVertices,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).clothMaxVertices);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.PhysicsColliderCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Physics Collider Count: {0}", perfStats.physicsColliderCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Physics Collider Count: {0} (Recommended: {1}) - Avoid use of colliders for optimal performance.",
                                perfStats.physicsColliderCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).physicsColliderCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Physics Collider Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many colliders. Avoid use of colliders for optimal performance.",
                                perfStats.physicsColliderCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).physicsColliderCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).physicsColliderCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.PhysicsRigidbodyCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Physics Rigidbody Count: {0}", perfStats.physicsRigidbodyCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Physics Rigidbody Count: {0} (Recommended: {1}) - Avoid use of rigidbodies for optimal performance.",
                                perfStats.physicsRigidbodyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).physicsRigidbodyCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Physics Rigidbody Count: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many rigidbodies. Avoid use of rigidbodies for optimal performance.",
                                perfStats.physicsRigidbodyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).physicsRigidbodyCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).physicsRigidbodyCount);

                            break;
                        }
                    }

                    break;
                }
                case AvatarPerformanceCategory.AudioSourceCount:
                {
                    switch(rating)
                    {
                        case PerformanceRating.Excellent:
                        case PerformanceRating.Good:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Verbose;
                            text = string.Format("Audio Sources: {0}", perfStats.audioSourceCount);
                            break;
                        }
                        case PerformanceRating.Medium:
                        case PerformanceRating.Poor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Audio Sources: {0} (Recommended: {1}) - Reduce number of audio sources for better performance.",
                                perfStats.audioSourceCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).audioSourceCount);

                            break;
                        }
                        case PerformanceRating.VeryPoor:
                        {
                            displayLevel = PerformanceInfoDisplayLevel.Warning;
                            text = string.Format(
                                "Audio Sources: {0} (Maximum: {1}, Recommended: {2}) - This avatar has too many audio sources. Reduce number of audio sources for better performance.",
                                perfStats.audioSourceCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Poor).audioSourceCount,
                                AvatarPerformanceStats.GetStatLevelForRating(PerformanceRating.Excellent).audioSourceCount);

                            break;
                        }
                    }

                    break;
                }
                default:
                {
                    text = "";
                    displayLevel = PerformanceInfoDisplayLevel.None;
                    break;
                }
            }
        }
    }
}
