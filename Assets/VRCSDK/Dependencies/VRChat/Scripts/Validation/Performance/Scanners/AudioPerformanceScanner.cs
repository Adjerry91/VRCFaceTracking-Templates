using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New AudioPerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/AudioPerformanceScanner"
    )]
    #endif
    public sealed class AudioPerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            // Audio Sources
            List<AudioSource> audioSourceBuffer = new List<AudioSource>();
            yield return ScanAvatarForComponentsOfType(avatarObject, audioSourceBuffer);
            if(shouldIgnoreComponent != null)
            {
                audioSourceBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            perfStats.audioSourceCount = audioSourceBuffer.Count;
        }
    }
}
