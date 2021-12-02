using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRC.SDKBase.Validation
{
    public static class ShaderValidation
    {
        public static IEnumerable<Shader> FindIllegalShaders(GameObject target, string[] whitelist)
        {
            List<Shader> illegalShaders = new List<Shader>();
            IEnumerator seeker = FindIllegalShadersEnumerator(target, whitelist, (c) => illegalShaders.Add(c));
            while(seeker.MoveNext())
            {
            }

            return illegalShaders;
        }

        private static IEnumerator FindIllegalShadersEnumerator(GameObject target, string[] whitelist, System.Action<Shader> onFound, bool useWatch = false)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            if(useWatch)
            {
                watch.Start();
            }

            List<Material> materialCache = new List<Material>();
            Queue<GameObject> children = new Queue<GameObject>();
            children.Enqueue(target.gameObject);
            while(children.Count > 0)
            {
                GameObject child = children.Dequeue();
                if(child == null)
                {
                    continue;
                }

                for(int idx = 0; idx < child.transform.childCount; ++idx)
                {
                    children.Enqueue(child.transform.GetChild(idx).gameObject);
                }

                foreach(Renderer childRenderers in child.transform.GetComponents<Renderer>())
                {
                    if(childRenderers == null)
                    {
                        continue;
                    }

                    foreach(Material sharedMaterial in childRenderers.sharedMaterials)
                    {
                        if(materialCache.Any(cacheMtl => sharedMaterial == cacheMtl)) // did we already look at this one?
                        {
                            continue;
                        }

                        // Skip empty material slots, or materials without shaders.
                        // Both will end up using the magenta error shader.
                        if(sharedMaterial == null || sharedMaterial.shader == null)
                        {
                            continue;
                        }

                        if(whitelist.All(okayShaderName => sharedMaterial.shader.name != okayShaderName))
                        {
                            onFound(sharedMaterial.shader);
                            yield return null;
                        }

                        materialCache.Add(sharedMaterial);
                    }

                    if(!useWatch || watch.ElapsedMilliseconds <= 1)
                    {
                        continue;
                    }

                    yield return null;
                    watch.Reset();
                }
            }
        }
    }
}
