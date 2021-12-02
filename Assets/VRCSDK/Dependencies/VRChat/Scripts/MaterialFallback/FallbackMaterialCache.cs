using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FallbackMaterialCache
{
    private readonly Dictionary<Material, Material> _fallbackMaterialCache = new Dictionary<Material, Material>();

    public void AddFallbackMaterial(Material material, Material fallbackMaterial)
    {
        if(!_fallbackMaterialCache.ContainsKey(material))
        {
            _fallbackMaterialCache.Add(material, fallbackMaterial);
        }
        else
        {
            Debug.LogError($"Attempted to add a duplicate fallback material '{fallbackMaterial.name}' for original material '{material.name}'.");
        }
    }

    public bool TryGetFallbackMaterial(Material material, out Material fallbackMaterial)
    {
        if(material != null)
        {
            return _fallbackMaterialCache.TryGetValue(material, out fallbackMaterial);
        }

        fallbackMaterial = null;
        return false;
    }

    public void Clear()
    {
        Material[] cachedFallbackMaterials = _fallbackMaterialCache.Values.ToArray();
        for(int i = cachedFallbackMaterials.Length - 1; i >= 0; i--)
        {
            Object.Destroy(cachedFallbackMaterials[i]);
        }

        _fallbackMaterialCache.Clear();
    }
}
