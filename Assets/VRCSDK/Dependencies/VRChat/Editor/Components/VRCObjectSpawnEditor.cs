#if VRC_SDK_VRCSDK2
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(VRCSDK2.VRC_ObjectSpawn))]
public class VRCObjectSpawnEditor : Editor
{
    VRCSDK2.VRC_ObjectSpawn spawn;

    void OnEnable()
    {
        if (spawn == null)
            spawn = (VRCSDK2.VRC_ObjectSpawn)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
#endif