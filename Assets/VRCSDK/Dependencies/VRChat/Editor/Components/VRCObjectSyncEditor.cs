#if VRC_SDK_VRCSDK2

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(VRCSDK2.VRC_ObjectSync))]
public class VRCObjectSyncEditor : Editor
{
    VRCSDK2.VRC_ObjectSync sync;

    void OnEnable()
    {
        if (sync == null)
            sync = (VRCSDK2.VRC_ObjectSync)target;
    }

    public override void OnInspectorGUI()
    {
        sync.SynchronizePhysics = EditorGUILayout.Toggle("Synchronize Physics",sync.SynchronizePhysics);
        sync.AllowCollisionTransfer = EditorGUILayout.Toggle("Allow Collision Transfer", sync.AllowCollisionTransfer);
    }
}
#endif