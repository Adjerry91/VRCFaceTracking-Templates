#if VRC_SDK_VRCSDK2 && UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VRCSDK2
{
    [CustomEditor(typeof(VRCSDK2.VRC_Pickup))]
    public class VRC_PickupEditor : UnityEditor.Editor
    {
        private void InspectorField(string propertyName, string humanName)
        {
            SerializedProperty propertyField = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(propertyField, new GUIContent(humanName), true);
        }

        private SerializedProperty momentumTransferMethodProperty;
        private SerializedProperty disallowTheftProperty;
        private SerializedProperty exactGunProperty;
        private SerializedProperty exactGripProperty;
        private SerializedProperty allowManipulationWhenEquippedProperty;
        private SerializedProperty orientationProperty;
        private SerializedProperty autoHoldProperty;
        private SerializedProperty interactionTextProperty;
        private SerializedProperty useTextProperty;
        private SerializedProperty throwVelocityBoostMinSpeedProperty;
        private SerializedProperty throwVelocityBoostScaleProperty;
        private SerializedProperty pickupableProperty;
        private SerializedProperty proximityProperty;

        public override void OnInspectorGUI()
        {
            momentumTransferMethodProperty = serializedObject.FindProperty("MomentumTransferMethod");
            disallowTheftProperty = serializedObject.FindProperty("DisallowTheft");
            exactGunProperty = serializedObject.FindProperty("ExactGun");
            exactGripProperty = serializedObject.FindProperty("ExactGrip");
            allowManipulationWhenEquippedProperty = serializedObject.FindProperty("allowManipulationWhenEquipped");
            orientationProperty = serializedObject.FindProperty("orientation");
            autoHoldProperty = serializedObject.FindProperty("AutoHold");
            interactionTextProperty = serializedObject.FindProperty("InteractionText");
            useTextProperty = serializedObject.FindProperty("UseText");
            throwVelocityBoostMinSpeedProperty = serializedObject.FindProperty("ThrowVelocityBoostMinSpeed");
            throwVelocityBoostScaleProperty = serializedObject.FindProperty("ThrowVelocityBoostScale");
            pickupableProperty = serializedObject.FindProperty("pickupable");
            proximityProperty = serializedObject.FindProperty("proximity");

            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - 30));

            EditorGUILayout.PropertyField(momentumTransferMethodProperty, new GUIContent("Momentum Transfer Method"));
            EditorGUILayout.PropertyField(disallowTheftProperty, new GUIContent("Disallow Theft"));
            EditorGUILayout.PropertyField(exactGunProperty, new GUIContent("Exact Gun"));
            EditorGUILayout.PropertyField(exactGripProperty, new GUIContent("Exact Grip"));
            EditorGUILayout.PropertyField(allowManipulationWhenEquippedProperty, new GUIContent("Allow Manipulation When Equipped"));
            EditorGUILayout.PropertyField(orientationProperty, new GUIContent("Orientation"));
            EditorGUILayout.PropertyField(autoHoldProperty, new GUIContent("AutoHold", "If the pickup is supposed to be aligned to the hand (i.e. orientation field is set to Gun or Grip), auto-detect means that it will be Equipped(not dropped when they release trigger),  otherwise just hold as a normal pickup."));
            EditorGUILayout.PropertyField(interactionTextProperty, new GUIContent("Interaction Text","Text displayed when user hovers over the pickup."));
            if (autoHoldProperty.enumValueIndex != (int)VRCSDK2.VRC_Pickup.AutoHoldMode.No)
                EditorGUILayout.PropertyField(useTextProperty, new GUIContent("Use Text", "Text to display describing action for clicking button, when this pickup is already being held."));
            EditorGUILayout.PropertyField(throwVelocityBoostMinSpeedProperty, new GUIContent("Throw Velocity Boost Min Speed"));
            EditorGUILayout.PropertyField(throwVelocityBoostScaleProperty, new GUIContent("Throw Velocity Boost Scale"));
            EditorGUILayout.PropertyField(pickupableProperty, new GUIContent("Pickupable"));
            EditorGUILayout.PropertyField(proximityProperty, new GUIContent("Proximity"));

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif