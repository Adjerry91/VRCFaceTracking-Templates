using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New LineRendererPerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/LineRendererPerformanceScanner"
    )]
    #endif
    public sealed class LineRendererPerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            // Line Renderers
            List<LineRenderer> lineRendererBuffer = new List<LineRenderer>();
            yield return ScanAvatarForComponentsOfType(avatarObject, lineRendererBuffer);
            if(shouldIgnoreComponent != null)
            {
                lineRendererBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            int numLineRenderers = lineRendererBuffer.Count;
            perfStats.lineRendererCount = numLineRenderers;
            perfStats.materialCount = perfStats.materialCount.GetValueOrDefault() + numLineRenderers;
        }
    }
}
