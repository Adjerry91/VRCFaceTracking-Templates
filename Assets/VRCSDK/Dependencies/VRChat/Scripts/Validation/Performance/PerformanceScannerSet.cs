using System.Collections;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Scanners;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName =  "New PerformanceScannerSet",
        menuName = "VRC Scriptable Objects/Performance/PerformanceScannerSet"
    )]
    #endif
    public class PerformanceScannerSet : ScriptableObject
    {
        public AbstractPerformanceScanner[] performanceScanners;

        public void RunPerformanceScan(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            foreach(AbstractPerformanceScanner performanceScanner in performanceScanners)
            {
                if(performanceScanner == null)
                {
                    continue;
                }

                performanceScanner.RunPerformanceScan(avatarObject, perfStats, shouldIgnoreComponent);
            }
        }

        public IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            foreach(AbstractPerformanceScanner performanceScanner in performanceScanners)
            {
                if(performanceScanner == null)
                {
                    continue;
                }

                yield return performanceScanner.RunPerformanceScanEnumerator(avatarObject, perfStats, shouldIgnoreComponent);
            }
        }
    }
}
