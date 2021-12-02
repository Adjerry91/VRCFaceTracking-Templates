#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

[CustomEditor(typeof(VRCAnimatorTemporaryPoseSpace))]
public class VRCAnimatorSetViewEditor : Editor
{
    VRCAnimatorTemporaryPoseSpace view;
    GUIStyle styleButtonActive;
    GUIStyle styleButtonInactive;

    public void OnEnable()
    {
        if (target == null) return;

        if (view == null)
            view = (VRCAnimatorTemporaryPoseSpace)target;

        styleButtonActive = new GUIStyle(EditorStyles.miniButton);
        styleButtonInactive = new GUIStyle(EditorStyles.miniButton);
        styleButtonActive.fixedWidth = 50;
        styleButtonInactive.fixedWidth = 50;
        styleButtonActive.normal.textColor = Color.green;
        styleButtonInactive.normal.textColor = Color.gray;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Pose Space", "Enter or exit a pose space based on the avatar's current pose."), GUILayout.MaxWidth(150));

        if (GUILayout.Button("Enter", view.enterPoseSpace ? styleButtonActive : styleButtonInactive))
            view.enterPoseSpace = true;
        if (GUILayout.Button("Exit", !view.enterPoseSpace ? styleButtonActive : styleButtonInactive))
            view.enterPoseSpace = false;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedDelay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("delayTime"), new GUIContent(view.fixedDelay ? "Delay Time (s)" : "Delay Time (%)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugString"));

        serializedObject.ApplyModifiedProperties();

        //if (_repaint)
        //    EditorUtility.SetDirty(target);
    }
}
#endif
