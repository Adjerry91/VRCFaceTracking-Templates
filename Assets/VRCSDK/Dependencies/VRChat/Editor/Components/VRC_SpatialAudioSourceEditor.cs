#if VRC_SDK_VRCSDK2 && UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

namespace VRCSDK2
{
    [CustomEditor(typeof(VRCSDK2.VRC_SpatialAudioSource))]
    public class VRC_SpatialAudioSourceEditor : UnityEditor.Editor
    {
        private bool showAdvancedOptions = false;
        private SerializedProperty gainProperty;
        private SerializedProperty nearProperty;
        private SerializedProperty farProperty;
        private SerializedProperty volRadiusProperty;
        private SerializedProperty enableSpatialProperty;
        private SerializedProperty useCurveProperty;

        public override void OnInspectorGUI()
        {
            gainProperty = serializedObject.FindProperty("Gain");
            nearProperty = serializedObject.FindProperty("Near");
            farProperty = serializedObject.FindProperty("Far");
            volRadiusProperty = serializedObject.FindProperty("VolumetricRadius");
            enableSpatialProperty = serializedObject.FindProperty("EnableSpatialization");
            useCurveProperty = serializedObject.FindProperty("UseAudioSourceVolumeCurve");

            serializedObject.Update();

            VRCSDK2.VRC_SpatialAudioSource target = serializedObject.targetObject as VRCSDK2.VRC_SpatialAudioSource;
            AudioSource source = target.GetComponent<AudioSource>();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(gainProperty, new GUIContent("Gain"));
            EditorGUILayout.PropertyField(farProperty, new GUIContent("Far"));
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
            bool enableSp = enableSpatialProperty.boolValue;
            if (showAdvancedOptions)
            {
                EditorGUILayout.PropertyField(nearProperty, new GUIContent("Near"));
                EditorGUILayout.PropertyField(volRadiusProperty, new GUIContent("Volumetric Radius"));
                EditorGUILayout.PropertyField(enableSpatialProperty, new GUIContent("Enable Spatialization"));
                if (enableSp)
                    EditorGUILayout.PropertyField(useCurveProperty, new GUIContent("Use AudioSource Volume Curve"));
            }

            EditorGUILayout.EndVertical();

            if (source != null)
                source.spatialize = enableSp;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif