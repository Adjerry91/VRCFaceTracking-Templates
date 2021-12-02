#if VRC_SDK_VRCSDK2 && UNITY_EDITOR

#pragma warning disable 0618

using UnityEditor;
using System.Collections;

namespace VRCSDK2
{
    [CustomEditor(typeof(VRCSDK2.VRC_KeyEvents))]
    public class VRC_KeyEventsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Obsolete. Please use a VRC_Trigger instead.", MessageType.Error);
        }
    }

    [CustomEditor(typeof(VRCSDK2.VRC_UseEvents))]
    public class VRC_UseEventsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Obsolete. Please use a VRC_Trigger instead.", MessageType.Error);
        }
    }

    [CustomEditor(typeof(VRCSDK2.VRC_TriggerColliderEventTrigger))]
    public class VRC_TriggerColliderEventTriggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Obsolete. Please use a VRC_Trigger instead.", MessageType.Error);
        }
    }

    [CustomEditor(typeof(VRCSDK2.VRC_TimedEvents))]
    public class VRC_TimedEventsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Obsolete. Please use a VRC_Trigger instead.", MessageType.Error);
        }
    }
}

#pragma warning restore 0618
#endif