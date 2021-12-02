using UnityEngine;

namespace VRC.SDKBase.Validation.Performance.Stats
{
    public class AvatarPerformanceStatsLevel : ScriptableObject
    {
        public int polyCount;
        public Bounds aabb;
        public int skinnedMeshCount;
        public int meshCount;
        public int materialCount;
        public int animatorCount;
        public int boneCount;
        public int lightCount;
        public int particleSystemCount;
        public int particleTotalCount;
        public int particleMaxMeshPolyCount;
        public bool particleTrailsEnabled;
        public bool particleCollisionEnabled;
        public int trailRendererCount;
        public int lineRendererCount;
        public int dynamicBoneComponentCount;
        public int dynamicBoneSimulatedBoneCount;
        public int dynamicBoneColliderCount;
        public int dynamicBoneCollisionCheckCount;
        public int clothCount;
        public int clothMaxVertices;
        public int physicsColliderCount;
        public int physicsRigidbodyCount;
        public int audioSourceCount;
    }
}
