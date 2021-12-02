#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

[CustomEditor(typeof(VRCAnimatorTrackingControl))]
public class VRCAnimatorTrackingControlEditor : Editor
{
    VRCAnimatorTrackingControl control;
    const float columnWidth = 64f;
    VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType trackingAll;

    string[] PopupOptions = new string[3]
    {
        "Tracking",
        "Animation",
        "None",
    };

    public void OnEnable()
    {
        if (target==null)
            return;

        if (control == null)
            control = (VRCAnimatorTrackingControl)target;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Tracking Control");
        EditorGUILayout.BeginVertical(GUI.skin.box);

        //Labels
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("No Change", GUILayout.MinWidth(columnWidth));
        EditorGUILayout.LabelField("Tracking", GUILayout.MinWidth(columnWidth));
        EditorGUILayout.LabelField("Animation", GUILayout.MinWidth(columnWidth));
        EditorGUILayout.EndHorizontal();

        //Force all
        var trackingAll = CheckAll();
        var lastAll = trackingAll;
        DrawTrackingOption("All", ref trackingAll);
        if (lastAll != trackingAll)
        {
            control.trackingHead =
                control.trackingLeftHand =
                control.trackingRightHand =
                control.trackingHip =
                control.trackingLeftFoot =
                control.trackingRightFoot =
                control.trackingLeftFingers =
                control.trackingRightFingers =
                control.trackingEyes =
                control.trackingMouth = trackingAll;
        }
        EditorGUILayout.Space();

        //Individual
        EditorGUI.BeginChangeCheck();
        {
            DrawTrackingOption("Head", ref control.trackingHead);
            DrawTrackingOption("Left Hand", ref control.trackingLeftHand);
            DrawTrackingOption("Right Hand", ref control.trackingRightHand);
            DrawTrackingOption("Hip", ref control.trackingHip);
            DrawTrackingOption("Left Foot", ref control.trackingLeftFoot);
            DrawTrackingOption("Right Foot", ref control.trackingRightFoot);
            DrawTrackingOption("Left Fingers", ref control.trackingLeftFingers);
            DrawTrackingOption("Right Fingers", ref control.trackingRightFingers);
            DrawTrackingOption("Eyes & Eyelids", ref control.trackingEyes);
            DrawTrackingOption("Mouth & Jaw", ref control.trackingMouth);
        }
        if (EditorGUI.EndChangeCheck())
            trackingAll = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)999;

        EditorGUILayout.EndVertical();

        control.debugString = EditorGUILayout.TextField("Debug String", control.debugString);

        serializedObject.ApplyModifiedProperties();

        //if (_repaint)
        //    EditorUtility.SetDirty(target);
    }
    void DrawTrackingOption(string name, ref VRCAnimatorTrackingControl.TrackingType value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name);
        bool result;

        //No Change
        result = EditorGUILayout.Toggle(value == VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.NoChange, GUILayout.MinWidth(columnWidth));
        if (result)
            value = VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.NoChange;

        //Tracking
        result = EditorGUILayout.Toggle(value == VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.Tracking, GUILayout.MinWidth(columnWidth));
        if (result)
            value = VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.Tracking;

        //Animation
        result = EditorGUILayout.Toggle(value == VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.Animation, GUILayout.MinWidth(columnWidth));
        if (result)
            value = VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.Animation;

        EditorGUILayout.EndHorizontal();
    }
    VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType CheckAll()
    {
        var type = control.trackingHead;
        bool same = (control.trackingHead == type &&
                control.trackingLeftHand == type &&
                control.trackingRightHand == type &&
                control.trackingHip == type &&
                control.trackingLeftFoot == type &&
                control.trackingRightFoot == type &&
                control.trackingLeftFingers == type &&
                control.trackingRightFingers == type &&
                control.trackingEyes == type &&
                control.trackingMouth == type);
        return same ? type : (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)999;
    }
}
#endif
