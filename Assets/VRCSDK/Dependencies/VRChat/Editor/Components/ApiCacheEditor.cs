using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using VRC.Core;

[CustomEditor(typeof(ApiCache))]
public class ApiCacheEditor : Editor {
    public override void OnInspectorGUI()
    {
        foreach (System.Type type in ApiCache.cache.Keys)
        {
            Dictionary<string, ApiCache.CacheEntry> typeCache = ApiCache.cache[type];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(type.Name);
            EditorGUILayout.LabelField(typeCache.Count.ToString());
            EditorGUILayout.EndHorizontal();
        }
    }
}
