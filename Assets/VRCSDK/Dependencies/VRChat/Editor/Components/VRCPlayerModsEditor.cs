#if VRC_SDK_VRCSDK2
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace VRCSDK2
{
	[CustomEditor(typeof(VRCSDK2.VRC_PlayerMods))]
	public class VRCPlayerModsEditor : UnityEditor.Editor
	{
        VRCSDK2.VRC_PlayerMods myTarget;

		void OnEnable()
		{
			if(myTarget == null)
				myTarget = (VRCSDK2.VRC_PlayerMods)target;
		}

		public override void OnInspectorGUI()
		{
			myTarget.isRoomPlayerMods = EditorGUILayout.Toggle("isRoomPlayerMods", myTarget.isRoomPlayerMods);
			
			List<VRCSDK2.VRCPlayerMod> playerMods = myTarget.playerMods;
			for(int i=0; i<playerMods.Count; ++i)
			{
				VRCSDK2.VRCPlayerMod mod = playerMods[i];
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField(mod.name, EditorStyles.boldLabel);
				if( mod.allowNameEdit )
					mod.name = EditorGUILayout.TextField( "Mod Name: ", mod.name );
				for(int j=0; j<mod.properties.Count; ++j)
				{
					VRCSDK2.VRCPlayerModProperty prop = mod.properties[j];
					myTarget.playerMods[i].properties[j] = DrawFieldForProp(prop);
				}
				if(GUILayout.Button ("Remove Mod"))
				{
					myTarget.RemoveMod(mod);
					break;
				}
				EditorGUILayout.EndVertical();
			}
			if(GUILayout.Button("Add Mods"))
			{
				VRCPlayerModEditorWindow.AddModCallback adcb = OnInspectorGUI;
				VRCPlayerModEditorWindow.Init(myTarget, adcb);
			}
		}

		VRCSDK2.VRCPlayerModProperty DrawFieldForProp(VRCSDK2.VRCPlayerModProperty property)
		{
			if(property.type.SystemType == typeof(int))
			{
				property.intValue = EditorGUILayout.IntField(property.name, property.intValue);
			}
			else if(property.type.SystemType == typeof(float))
			{
				property.floatValue = EditorGUILayout.FloatField(property.name, property.floatValue);
			}
			else if(property.type.SystemType == typeof(string))
			{
				property.stringValue = EditorGUILayout.TextField(property.name, property.stringValue);
			}
			else if(property.type.SystemType == typeof(bool))
			{
				property.boolValue = EditorGUILayout.Toggle(property.name, property.boolValue);
			}
			else if(property.type.SystemType == typeof(GameObject))
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( property.name );
				property.gameObjectValue = (GameObject) EditorGUILayout.ObjectField( property.gameObjectValue, typeof( GameObject ), true );
				EditorGUILayout.EndHorizontal();
			}
			else if(property.type.SystemType == typeof(KeyCode))
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( property.name );
				property.keyCodeValue = (KeyCode) EditorGUILayout.EnumPopup( property.keyCodeValue );
				EditorGUILayout.EndHorizontal();
			}
			else if(property.type.SystemType == typeof(VRCSDK2.VRC_EventHandler.VrcBroadcastType))
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( property.name );
				property.broadcastValue = (VRCSDK2.VRC_EventHandler.VrcBroadcastType) EditorGUILayout.EnumPopup( property.broadcastValue );
				EditorGUILayout.EndHorizontal();
			}
			else if(property.type.SystemType == typeof(VRCSDK2.VRCPlayerModFactory.HealthOnDeathAction))
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( property.name );
				property.onDeathActionValue = (VRCSDK2.VRCPlayerModFactory.HealthOnDeathAction) EditorGUILayout.EnumPopup( property.onDeathActionValue);
				EditorGUILayout.EndHorizontal();
			}
			else if(property.type.SystemType == typeof(RuntimeAnimatorController))
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( property.name );
				property.animationController = (RuntimeAnimatorController) EditorGUILayout.ObjectField( property.animationController, typeof( RuntimeAnimatorController ), false );
				EditorGUILayout.EndHorizontal();
			}
			return property;
		}
	}
}
#endif
