#if VRC_SDK_VRCSDK2
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VRCSDK2.VRC_ObjectSync))]
public class VRC_ObjectSyncEditor : Editor {
    public override void OnInspectorGUI()
    {
        VRCSDK2.VRC_ObjectSync c = ((VRCSDK2.VRC_ObjectSync)target);
        if ((c.gameObject.GetComponent<Animator>() != null || c.gameObject.GetComponent<Animation>() != null) && c.SynchronizePhysics)
            EditorGUILayout.HelpBox("If the Animator or Animation moves the root position of this object then it will conflict with physics synchronization.", MessageType.Warning);
        if (c.GetComponent<VRCSDK2.VRC_DataStorage>() != null && c.SynchronizePhysics)
            EditorGUILayout.HelpBox("Consider either removing the VRC_DataStorage or disabling SynchronizePhysics.", MessageType.Warning);
        DrawDefaultInspector();
    }
}
#endif