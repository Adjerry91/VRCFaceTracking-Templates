using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName = "New PhysicsPerformanceScanner",
        menuName = "VRC Scriptable Objects/Performance/Avatar/Scanners/PhysicsPerformanceScanner"
    )]
    #endif
    public sealed class PhysicsPerformanceScanner : AbstractPerformanceScanner
    {
        public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            // Colliders
            List<Collider> colliderBuffer = new List<Collider>();
            yield return ScanAvatarForComponentsOfType(avatarObject, colliderBuffer);
            colliderBuffer.RemoveAll(
                o =>
                {
                    if(shouldIgnoreComponent != null && shouldIgnoreComponent(o))
                    {
                        return true;
                    }

                    if(o.GetComponent<VRC.SDKBase.VRCStation>() != null)
                    {
                        return true;
                    }

                    return false;
                }
            );

            perfStats.physicsColliderCount = colliderBuffer.Count;

            // Rigidbodies
            List<Rigidbody> rigidbodyBuffer = new List<Rigidbody>();
            yield return ScanAvatarForComponentsOfType(avatarObject, rigidbodyBuffer);
            rigidbodyBuffer.RemoveAll(
                o =>
                {
                    if(shouldIgnoreComponent != null && shouldIgnoreComponent(o))
                    {
                        return true;
                    }

                    if(o.GetComponent<VRC.SDKBase.VRCStation>() != null)
                    {
                        return true;
                    }

                    return false;
                }
            );

            perfStats.physicsRigidbodyCount = rigidbodyBuffer.Count;
        }
    }
}
