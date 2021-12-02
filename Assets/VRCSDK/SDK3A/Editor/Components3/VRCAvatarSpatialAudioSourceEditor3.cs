#if VRC_SDK_VRCSDK3
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace VRC.SDK3.Editor
{
    [CustomEditor(typeof(VRC.SDK3.Avatars.Components.VRCSpatialAudioSource))]
    public class VRCAvatarSpatialAudioSourceEditor3 : UnityEditor.Editor
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

            VRC.SDKBase.VRC_SpatialAudioSource target = serializedObject.targetObject as VRC.SDKBase.VRC_SpatialAudioSource;
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
#endif