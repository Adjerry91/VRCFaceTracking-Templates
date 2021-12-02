using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New TrailRendererPerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/TrailRendererPerformanceScanner"
    )]
    #endif
    public sealed class TrailRendererPerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            // Trail Renderers
            List<TrailRenderer> trailRendererBuffer = new List<TrailRenderer>();
            yield return ScanAvatarForComponentsOfType(avatarObject, trailRendererBuffer);
            if(shouldIgnoreComponent != null)
            {
                trailRendererBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            int numTrailRenderers = trailRendererBuffer.Count;
            perfStats.trailRendererCount = numTrailRenderers;
            perfStats.materialCount = perfStats.materialCount.GetValueOrDefault() + numTrailRenderers;
        }
    }
}
