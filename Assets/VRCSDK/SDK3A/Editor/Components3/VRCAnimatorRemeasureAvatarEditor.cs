#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

//Will be revisiting this, removed temporarily Kiro - Aug/5/2020
/*[CustomEditor(typeof(VRCAnimatorRemeasureAvatar))]
public class VRCAnimatorRemeasureAvatarEditor : Editor
{
    VRCAnimatorRemeasureAvatar view;

    public void OnEnable()
    {
        if (target == null) return;

        if (view == null)
            view = (VRCAnimatorRemeasureAvatar)target;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedDelay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("delayTime"), new GUIContent(view.fixedDelay ? "Delay Time (s)" : "Delay Time (%)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugString"));

        serializedObject.ApplyModifiedProperties();

        //if (_repaint)
        //    EditorUtility.SetDirty(target);
    }
}*/
#endif
