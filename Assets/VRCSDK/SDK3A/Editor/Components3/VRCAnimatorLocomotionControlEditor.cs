#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

[CustomEditor(typeof(VRCAnimatorLocomotionControl))]
public class VRCAnimatorLocomotionControlEditor : Editor
{
    VRCAnimatorLocomotionControl control;
    GUIStyle styleButtonActive;
    GUIStyle styleButtonInactive;

    public void OnEnable()
    {
        if (target == null)
            return;

        if (control == null)
            control = (VRCAnimatorLocomotionControl)target;

        styleButtonActive = new GUIStyle(EditorStyles.miniButton);
        styleButtonInactive = new GUIStyle(EditorStyles.miniButton);
        styleButtonActive.fixedWidth = 80;
        styleButtonInactive.fixedWidth = 80;
        styleButtonActive.normal.textColor = Color.green;
        styleButtonInactive.normal.textColor = Color.gray;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Locomotion Control", GUILayout.MaxWidth(150));
        if (control.disableLocomotion)
        {
            GUILayout.Button("Disable", styleButtonActive);
            if (GUILayout.Button("Enable", styleButtonInactive))
                control.disableLocomotion = false;
        }
        else
        {
            if (GUILayout.Button("Disable", styleButtonInactive))
                control.disableLocomotion = true;
            GUILayout.Button("Enable", styleButtonActive);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        control.debugString = EditorGUILayout.TextField("Debug String", control.debugString);

        serializedObject.ApplyModifiedProperties();

        //if (_repaint)
        //    EditorUtility.SetDirty(target);
    }
}
#endif
