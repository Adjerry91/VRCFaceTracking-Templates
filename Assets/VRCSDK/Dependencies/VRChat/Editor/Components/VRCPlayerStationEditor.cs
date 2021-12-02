#if VRC_SDK_VRCSDK2

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

[CustomEditor(typeof(VRCSDK2.VRC_Station))]
public class VRCPlayerStationEditor : Editor 
{
    VRCSDK2.VRC_Station myTarget;

	SerializedProperty onRemoteEnter;
	SerializedProperty onRemoteExit;
	SerializedProperty onLocalEnter;
	SerializedProperty onLocalExit;

	void OnEnable()
	{
		if(myTarget == null)
			myTarget = (VRCSDK2.VRC_Station)target;
		onRemoteEnter = serializedObject.FindProperty("OnRemotePlayerEnterStation");
		onRemoteExit = serializedObject.FindProperty("OnRemotePlayerExitStation");
		onLocalEnter = serializedObject.FindProperty("OnLocalPlayerEnterStation");
		onLocalExit = serializedObject.FindProperty("OnLocalPlayerExitStation");
	}

	public override void OnInspectorGUI()
	{
		myTarget.PlayerMobility = (VRC.SDKBase.VRCStation.Mobility)EditorGUILayout.EnumPopup("Player Mobility", myTarget.PlayerMobility);
		myTarget.canUseStationFromStation = EditorGUILayout.Toggle("Can Use Station From Station", myTarget.canUseStationFromStation);
		myTarget.animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller", myTarget.animatorController, typeof(RuntimeAnimatorController), false);
		myTarget.disableStationExit = EditorGUILayout.Toggle("Disable Station Exit", myTarget.disableStationExit);
		myTarget.seated = EditorGUILayout.Toggle("Seated", myTarget.seated);
		myTarget.stationEnterPlayerLocation = (Transform)EditorGUILayout.ObjectField("Player Enter Location", myTarget.stationEnterPlayerLocation, typeof(Transform), true);
		myTarget.stationExitPlayerLocation = (Transform)EditorGUILayout.ObjectField("Player Exit Location", myTarget.stationExitPlayerLocation, typeof(Transform), true);
		myTarget.controlsObject = (VRC.SDKBase.VRC_ObjectApi)EditorGUILayout.ObjectField("API Object", myTarget.controlsObject, typeof(VRC.SDKBase.VRC_ObjectApi), false);

		EditorGUILayout.PropertyField(onRemoteEnter, new GUIContent("On Remote Player Enter"));
		EditorGUILayout.PropertyField(onRemoteExit, new GUIContent("On Remote Player Exit"));
		EditorGUILayout.PropertyField(onLocalEnter, new GUIContent("On Local Player Enter"));
		EditorGUILayout.PropertyField(onLocalExit, new GUIContent("On Local Player Exit"));
	}
}
#endif
