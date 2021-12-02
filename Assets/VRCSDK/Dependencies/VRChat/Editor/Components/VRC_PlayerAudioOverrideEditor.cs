#if VRC_SDK_VRCSDK2
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

namespace VRCSDK2
{
    [CustomEditor(typeof(VRCSDK2.VRC_PlayerAudioOverride))]
    public class VRC_PlayerAudioOverrideEditor : UnityEditor.Editor
    {
        private bool voShow = true;
        private bool voAdv = false;
        private bool avShow = true;
        private bool avAdv = false;
        private SerializedProperty prioProperty;
        private SerializedProperty globalProperty;
        private SerializedProperty regionProperty;
        private SerializedProperty voGainProperty;
        private SerializedProperty voNearProperty;
        private SerializedProperty voFarProperty;
        private SerializedProperty voRadiusProperty;
        private SerializedProperty voDisableLpProperty;
        private SerializedProperty avGainProperty;
        private SerializedProperty avNearProperty;
        private SerializedProperty avFarProperty;
        private SerializedProperty avRadiusProperty;
        private SerializedProperty avForceSpatialProperty;
        private SerializedProperty avAllowCustomProperty;

        public override void OnInspectorGUI()
        {
            globalProperty = serializedObject.FindProperty("global");
            regionProperty = serializedObject.FindProperty("region");
            prioProperty = serializedObject.FindProperty("regionPriority");

            voGainProperty = serializedObject.FindProperty("VoiceGain");
            voNearProperty = serializedObject.FindProperty("VoiceNear");
            voFarProperty = serializedObject.FindProperty("VoiceFar");
            voRadiusProperty = serializedObject.FindProperty("VoiceVolumetricRadius");
            voDisableLpProperty = serializedObject.FindProperty("VoiceDisableLowpass");

            avGainProperty = serializedObject.FindProperty("AvatarGainLimit");
            avNearProperty = serializedObject.FindProperty("AvatarNearLimit");
            avFarProperty = serializedObject.FindProperty("AvatarFarLimit");
            avRadiusProperty = serializedObject.FindProperty("AvatarVolumetricRadiusLimit");
            avForceSpatialProperty = serializedObject.FindProperty("AvatarForceSpatial");
            avAllowCustomProperty = serializedObject.FindProperty("AvatarAllowCustomCurve");

            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            var ovr = serializedObject.targetObject as VRCSDK2.VRC_PlayerAudioOverride;

            EditorGUILayout.PropertyField(globalProperty, new GUIContent("Global"));
            if (!ovr.global)
            {
                EditorGUILayout.PropertyField(regionProperty, new GUIContent("Region"));
                EditorGUILayout.PropertyField(prioProperty, new GUIContent("Priority"));
            }

            voShow = EditorGUILayout.Foldout(voShow, "Voice Settings");

            if (voShow)
            {
                EditorGUILayout.PropertyField(voGainProperty, new GUIContent("Gain"));
                EditorGUILayout.PropertyField(voFarProperty, new GUIContent("Far"));

                EditorGUI.indentLevel++;
                voAdv = EditorGUILayout.Foldout(voAdv, "Advanced Options");
                if (voAdv)
                {
                    EditorGUILayout.PropertyField(voNearProperty, new GUIContent("Near"));
                    EditorGUILayout.PropertyField(voRadiusProperty, new GUIContent("Volumetric Radius"));
                    EditorGUILayout.PropertyField(voDisableLpProperty, new GUIContent("Disable Lowpass Filter"));
                }
                EditorGUI.indentLevel--;
            }

            avShow = EditorGUILayout.Foldout(avShow, "Avatar Audio Limits");

            if (avShow)
            {
                EditorGUILayout.PropertyField(avGainProperty, new GUIContent("Gain Limit"));
                EditorGUILayout.PropertyField(avFarProperty, new GUIContent("Far Limit"));

                EditorGUI.indentLevel++;
                avAdv = EditorGUILayout.Foldout(avAdv, "Advanced Options");
                if (avAdv)
                {
                    EditorGUILayout.PropertyField(avNearProperty, new GUIContent("Near Limit"));
                    EditorGUILayout.PropertyField(avRadiusProperty, new GUIContent("Volumetric Radius Limit"));
                    EditorGUILayout.PropertyField(avForceSpatialProperty, new GUIContent("Force Spatial"));
                    EditorGUILayout.PropertyField(avAllowCustomProperty, new GUIContent("Allow Custom Curve"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif