#if VRC_SDK_VRCSDK3

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

[CustomEditor(typeof(VRC.SDK3.Avatars.Components.VRCStation))]
public class VRCAvatarPlayerStationEditor3 : Editor 
{
    VRC.SDK3.Avatars.Components.VRCStation myTarget;

    void OnEnable()
    {
        if(myTarget == null)
            myTarget = (VRC.SDK3.Avatars.Components.VRCStation)target;
    }

    public override void OnInspectorGUI()
    {
        myTarget.PlayerMobility = (VRC.SDKBase.VRCStation.Mobility)EditorGUILayout.EnumPopup("Player Mobility", myTarget.PlayerMobility);
        myTarget.canUseStationFromStation = EditorGUILayout.Toggle("Can Use Station From Station", myTarget.canUseStationFromStation);
        myTarget.animatorController = (RuntimeAnimatorController) EditorGUILayout.ObjectField("Animator Controller", myTarget.animatorController, typeof(RuntimeAnimatorController), false );
        myTarget.disableStationExit = EditorGUILayout.Toggle("Disable Station Exit", myTarget.disableStationExit );
        myTarget.seated = EditorGUILayout.Toggle("Seated", myTarget.seated);
        myTarget.stationEnterPlayerLocation = (Transform)EditorGUILayout.ObjectField("Player Enter Location", myTarget.stationEnterPlayerLocation, typeof(Transform), true);
        myTarget.stationExitPlayerLocation = (Transform)EditorGUILayout.ObjectField("Player Exit Location", myTarget.stationExitPlayerLocation, typeof(Transform), true);
        myTarget.controlsObject = (VRC.SDKBase.VRC_ObjectApi)EditorGUILayout.ObjectField("API Object", myTarget.controlsObject, typeof(VRC.SDKBase.VRC_ObjectApi), false);
    }
}
#endif