using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New AnimatorPerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/AnimatorPerformanceScanner"
    )]
    #endif
    public sealed class AnimatorPerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            int animatorCount = 0;

            // Animators
            List<Animator> animatorBuffer = new List<Animator>();
            yield return ScanAvatarForComponentsOfType(avatarObject, animatorBuffer);
            if(shouldIgnoreComponent != null)
            {
                animatorBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            // ReSharper disable once UselessBinaryOperation
            animatorCount += animatorBuffer.Count;

            // Animations
            List<Animation> animationBuffer = new List<Animation>();
            yield return ScanAvatarForComponentsOfType(avatarObject, animationBuffer);
            if(shouldIgnoreComponent != null)
            {
                animationBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            animatorCount += animationBuffer.Count;

            perfStats.animatorCount = animatorCount;
        }
    }
}
