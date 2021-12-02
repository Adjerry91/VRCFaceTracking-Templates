using UnityEngine;

namespace VRC.SDKBase.Validation.Performance
{
    public static class MeshUtils
    {
        private const uint INDICES_PER_TRIANGLE = 3U;

        public static uint GetMeshTriangleCount(Mesh sourceMesh)
        {
            if(sourceMesh == null)
            {
                return 0;
            }

            // We can't use GetIndexCount if the mesh isn't readable so just return a huge number.
            // The SDK Control Panel should show a warning in this case.
            if(!sourceMesh.isReadable)
            {
                return uint.MaxValue;
            }

            uint count = 0;
            for(int i = 0; i < sourceMesh.subMeshCount; i++)
            {
                uint indexCount = sourceMesh.GetIndexCount(i);
                count += indexCount / INDICES_PER_TRIANGLE;
            }

            return count;
        }
    }
}
