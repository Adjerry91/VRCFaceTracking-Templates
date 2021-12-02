using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New ClothPerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/ClothPerformanceScanner"
    )]
    #endif
    public sealed class ClothPerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            // Cloth
            List<Cloth> clothBuffer = new List<Cloth>();
            yield return ScanAvatarForComponentsOfType(avatarObject, clothBuffer);
            if(shouldIgnoreComponent != null)
            {
                clothBuffer.RemoveAll(c => shouldIgnoreComponent(c));
            }

            int totalClothVertices = 0;
            foreach(Cloth cloth in clothBuffer)
            {
                if(cloth == null)
                {
                    continue;
                }

                Vector3[] clothVertices = cloth.vertices;
                if(clothVertices == null)
                {
                    continue;
                }

                totalClothVertices += clothVertices.Length;
            }

            perfStats.clothCount = clothBuffer.Count;
            perfStats.clothMaxVertices = totalClothVertices;
        }
    }
}
