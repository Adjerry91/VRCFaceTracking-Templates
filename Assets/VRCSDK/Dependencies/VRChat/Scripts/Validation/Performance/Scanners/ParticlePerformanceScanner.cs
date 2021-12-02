using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New ParticlePerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/ParticlePerformanceScanner"
    )]
    #endif
    public sealed class ParticlePerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            // Particle Systems
            List<ParticleSystem> particleSystemBuffer = new List<ParticleSystem>();
            yield return ScanAvatarForComponentsOfType(avatarObject, particleSystemBuffer);
            if(shouldIgnoreComponent != null)
            {
                particleSystemBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            AnalyzeParticleSystemRenderers(particleSystemBuffer, perfStats);

            yield return null;
        }

        private static void AnalyzeParticleSystemRenderers(IEnumerable<ParticleSystem> particleSystems, AvatarPerformanceStats perfStats)
        {
            int particleSystemCount = 0;
            ulong particleTotalCount = 0;
            ulong particleTotalMaxMeshPolyCount = 0;
            bool particleTrailsEnabled = false;
            bool particleCollisionEnabled = false;
            int materialSlots = 0;

            Profiler.BeginSample("AnalyzeParticleSystemRenderers");
            foreach(ParticleSystem particleSystem in particleSystems)
            {
                Profiler.BeginSample("Single Particle System");
                int particleCount = particleSystem.main.maxParticles;
                if(particleCount <= 0)
                {
                    Profiler.EndSample();
                    continue;
                }

                particleSystemCount++;
                particleTotalCount += (uint)particleCount;

                ParticleSystemRenderer particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                if(particleSystemRenderer == null)
                {
                    Profiler.EndSample();
                    continue;
                }

                materialSlots++;

                // mesh particles
                if(particleSystemRenderer.renderMode == ParticleSystemRenderMode.Mesh && particleSystemRenderer.meshCount > 0)
                {
                    uint highestPolyCount = 0;

                    Mesh[] meshes = new Mesh[particleSystemRenderer.meshCount];
                    int particleRendererMeshCount = particleSystemRenderer.GetMeshes(meshes);
                    for(int meshIndex = 0; meshIndex < particleRendererMeshCount; meshIndex++)
                    {
                        Mesh mesh = meshes[meshIndex];
                        if(mesh == null)
                        {
                            continue;
                        }

                        uint polyCount = MeshUtils.GetMeshTriangleCount(mesh);
                        if(polyCount > highestPolyCount)
                        {
                            highestPolyCount = polyCount;
                        }
                    }

                    ulong maxMeshParticlePolyCount = (uint)particleCount * highestPolyCount;
                    particleTotalMaxMeshPolyCount += maxMeshParticlePolyCount;
                }

                if(particleSystem.trails.enabled)
                {
                    particleTrailsEnabled = true;
                    materialSlots++;
                }

                if(particleSystem.collision.enabled)
                {
                    particleCollisionEnabled = true;
                }

                Profiler.EndSample();
            }

            Profiler.EndSample();

            perfStats.particleSystemCount = particleSystemCount;
            perfStats.particleTotalCount = particleTotalCount > int.MaxValue ? int.MaxValue : (int)particleTotalCount;
            perfStats.particleMaxMeshPolyCount = particleTotalMaxMeshPolyCount > int.MaxValue ? int.MaxValue : (int)particleTotalMaxMeshPolyCount;
            perfStats.particleTrailsEnabled = particleTrailsEnabled;
            perfStats.particleCollisionEnabled = particleCollisionEnabled;
            perfStats.materialCount = perfStats.materialCount.GetValueOrDefault() + materialSlots;
        }
    }
}
