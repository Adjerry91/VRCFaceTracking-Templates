using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New LightPerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/LightPerformanceScanner"
    )]
    #endif
    public sealed class LightPerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            // Lights
            List<Light> lightBuffer = new List<Light>();
            yield return ScanAvatarForComponentsOfType(avatarObject, lightBuffer);
            if(shouldIgnoreComponent != null)
            {
                lightBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            perfStats.lightCount = lightBuffer.Count;
        }
    }
}
