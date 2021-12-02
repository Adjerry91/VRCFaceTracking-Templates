using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New MeshPerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/MeshPerformanceScanner"
    )]
    #endif
    public sealed class MeshPerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            // Renderers
            List<Renderer> rendererBuffer = new List<Renderer>(16);
            yield return ScanAvatarForComponentsOfType(avatarObject, rendererBuffer);
            if(shouldIgnoreComponent != null)
            {
                rendererBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            yield return AnalyzeGeometry(avatarObject, rendererBuffer, perfStats);
            AnalyzeMeshRenderers(rendererBuffer, perfStats);
            AnalyzeSkinnedMeshRenderers(rendererBuffer, perfStats);


            yield return null;
        }

        private static uint CalculateRendererPolyCount(Renderer renderer)
        {
            Mesh sharedMesh = null;
            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if(skinnedMeshRenderer != null)
            {
                sharedMesh = skinnedMeshRenderer.sharedMesh;
            }

            if(sharedMesh == null)
            {
                MeshRenderer meshRenderer = renderer as MeshRenderer;
                if(meshRenderer != null)
                {
                    MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    if(meshFilter != null)
                    {
                        sharedMesh = meshFilter.sharedMesh;
                    }
                }
            }

            if(sharedMesh == null)
            {
                return 0;
            }

            return MeshUtils.GetMeshTriangleCount(sharedMesh);
        }

        private static bool RendererHasMesh(Renderer renderer)
        {
            MeshRenderer meshRenderer = renderer as MeshRenderer;
            if(meshRenderer != null)
            {
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if(meshFilter == null)
                {
                    return false;
                }

                return meshFilter.sharedMesh != null;
            }

            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if(skinnedMeshRenderer != null)
            {
                return skinnedMeshRenderer.sharedMesh != null;
            }

            return false;
        }

        private IEnumerator AnalyzeGeometry(GameObject avatarObject, IEnumerable<Renderer> renderers, AvatarPerformanceStats perfStats)
        {
            List<Renderer> lodGroupRendererIgnoreBuffer = new List<Renderer>(16);
            List<LODGroup> lodBuffer = new List<LODGroup>(16);

            ulong polyCount = 0;
            Bounds bounds = new Bounds(avatarObject.transform.position, Vector3.zero);
            
            yield return ScanAvatarForComponentsOfType(avatarObject, lodBuffer);
            try
            {
                foreach(LODGroup lodGroup in lodBuffer)
                {
                    LOD[] lodLevels = lodGroup.GetLODs();

                    ulong highestLodPolyCount = 0;
                    foreach(LOD lod in lodLevels)
                    {
                        uint thisLodPolyCount = 0;
                        foreach(Renderer renderer in lod.renderers)
                        {
                            lodGroupRendererIgnoreBuffer.Add(renderer);
                            checked
                            {
                                thisLodPolyCount += CalculateRendererPolyCount(renderer);
                            }
                        }

                        if(thisLodPolyCount > highestLodPolyCount)
                        {
                            highestLodPolyCount = thisLodPolyCount;
                        }
                    }

                    checked
                    {
                        polyCount += highestLodPolyCount;
                    }
                }
            }
            catch(OverflowException e)
            {
                VRC.Core.Logger.Log("Overflow exception while analyzing geometry, assuming max value:" + e.ToString(), VRC.Core.DebugLevel.All);
                polyCount = uint.MaxValue;
            }

            Profiler.BeginSample("Calculate Total Polygon Count and Bounds");
            foreach(Renderer renderer in renderers)
            {
                Profiler.BeginSample("Single Renderer");
                if(renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    if(!RendererHasMesh(renderer))
                    {
                        Profiler.EndSample();
                        continue;
                    }

                    bounds.Encapsulate(renderer.bounds);
                }

                if(lodGroupRendererIgnoreBuffer.Contains(renderer))
                {
                    Profiler.EndSample();
                    continue;
                }

                polyCount += CalculateRendererPolyCount(renderer);
                Profiler.EndSample();
            }

            Profiler.EndSample();

            bounds.center -= avatarObject.transform.position;

            lodGroupRendererIgnoreBuffer.Clear();
            lodBuffer.Clear();

            perfStats.polyCount = polyCount > int.MaxValue ? int.MaxValue : (int)polyCount;
            perfStats.aabb = bounds;
        }

        private static void AnalyzeSkinnedMeshRenderers(IEnumerable<Renderer> renderers, AvatarPerformanceStats perfStats)
        {
            Profiler.BeginSample("AnalyzeSkinnedMeshRenderers");
            int count = 0;
            int materialSlots = 0;
            int skinnedBoneCount = 0;
            HashSet<Transform> transformIgnoreBuffer = new HashSet<Transform>();

            foreach(Renderer renderer in renderers)
            {
                Profiler.BeginSample("Analyze SkinnedMeshRenderer");
                SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                if(skinnedMeshRenderer == null)
                {
                    Profiler.EndSample();
                    continue;
                }

                count++;

                Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
                if(sharedMesh != null)
                {
                    materialSlots += sharedMesh.subMeshCount;
                }

                // bone count
                Profiler.BeginSample("Count Bones");
                Transform[] bones = skinnedMeshRenderer.bones;
                foreach(Transform bone in bones)
                {
                    Profiler.BeginSample("Count Bone");
                    if(bone == null || transformIgnoreBuffer.Contains(bone))
                    {
                        Profiler.EndSample();
                        continue;
                    }

                    transformIgnoreBuffer.Add(bone);
                    skinnedBoneCount++;
                    Profiler.EndSample();
                }

                Profiler.EndSample();

                Profiler.EndSample();
            }

            transformIgnoreBuffer.Clear();
            Profiler.EndSample();

            perfStats.skinnedMeshCount = count;
            perfStats.boneCount = skinnedBoneCount;
            perfStats.materialCount = perfStats.materialCount.GetValueOrDefault() + materialSlots;
        }

        private static void AnalyzeMeshRenderers(IEnumerable<Renderer> renderers, AvatarPerformanceStats perfStats)
        {
            Profiler.BeginSample("AnalyzeMeshRenderers");
            int count = 0;
            int materialSlots = 0;
            foreach(Renderer renderer in renderers)
            {
                Profiler.BeginSample("Analyze MeshRenderer");
                MeshRenderer meshRenderer = renderer as MeshRenderer;
                if(meshRenderer == null)
                {
                    Profiler.EndSample();
                    continue;
                }

                count++;

                Profiler.BeginSample("Get MeshFilter");
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                Profiler.EndSample();
                if(meshFilter == null)
                {
                    Profiler.EndSample();
                    continue;
                }

                Mesh sharedMesh = meshFilter.sharedMesh;
                if(sharedMesh != null)
                {
                    materialSlots += sharedMesh.subMeshCount;
                }
            }

            Profiler.EndSample();

            perfStats.meshCount = count;
            perfStats.materialCount = perfStats.materialCount.GetValueOrDefault() + materialSlots;
        }
    }
}
