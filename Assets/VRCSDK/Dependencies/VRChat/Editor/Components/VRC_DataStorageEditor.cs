#if VRC_SDK_VRCSDK2 && UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace VRCSDK2
{
    [CustomPropertyDrawer(typeof(VRCSDK2.VRC_DataStorage.VrcDataElement))]
    public class CustomDataElementDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (property == null)
                return;

            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            SerializedProperty mirrorProperty = property.FindPropertyRelative("mirror");
            SerializedProperty typeProperty = property.FindPropertyRelative("type");
            SerializedProperty valueProperty = null;
            switch (typeProperty.enumValueIndex)
            {
                case (int)VRCSDK2.VRC_DataStorage.VrcDataType.Bool:
                    valueProperty = property.FindPropertyRelative("valueBool");
                    break;
                case (int)VRCSDK2.VRC_DataStorage.VrcDataType.Float:
                    valueProperty = property.FindPropertyRelative("valueFloat");
                    break;
                case (int)VRCSDK2.VRC_DataStorage.VrcDataType.Int:
                    valueProperty = property.FindPropertyRelative("valueInt");
                    break;
                case (int)VRCSDK2.VRC_DataStorage.VrcDataType.String:
                    valueProperty = property.FindPropertyRelative("valueString");
                    break;
                case (int)VRCSDK2.VRC_DataStorage.VrcDataType.SerializeObject:
                    valueProperty = property.FindPropertyRelative("serializeComponent");
                    break;
                case (int)VRCSDK2.VRC_DataStorage.VrcDataType.None:
                case (int)VRCSDK2.VRC_DataStorage.VrcDataType.SerializeBytes:
                    break;
            }

            EditorGUI.BeginProperty(rect, label, property);

            int baseWidth = (int)(rect.width / 4);
            Rect nameRect = new Rect(rect.x, rect.y, baseWidth, rect.height);
            Rect mirrorRect = new Rect(rect.x + baseWidth, rect.y, baseWidth, rect.height);
            Rect typeRect = new Rect(rect.x + baseWidth * 2, rect.y, baseWidth, rect.height);
            Rect valueRect = new Rect(rect.x + baseWidth * 3, rect.y, baseWidth, rect.height);
            Rect typeValueRect = new Rect(rect.x + baseWidth * 2, rect.y, baseWidth * 2, rect.height);

            EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
            EditorGUI.PropertyField(mirrorRect, mirrorProperty, GUIContent.none);

            switch (mirrorProperty.enumValueIndex)
            {
                case (int)VRCSDK2.VRC_DataStorage.VrcDataMirror.None:
                    if (valueProperty == null)
                        VRC_EditorTools.FilteredEnumPopup<VRCSDK2.VRC_DataStorage.VrcDataType>(typeValueRect, typeProperty, t => true);
                    else
                    {
                        VRC_EditorTools.FilteredEnumPopup<VRCSDK2.VRC_DataStorage.VrcDataType>(typeRect, typeProperty, t => true);
                        EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
                    }
                    break;
                case (int)VRCSDK2.VRC_DataStorage.VrcDataMirror.SerializeComponent:
                    typeProperty.enumValueIndex = (int)VRCSDK2.VRC_DataStorage.VrcDataType.SerializeObject;
                    EditorGUI.PropertyField(typeValueRect, valueProperty, GUIContent.none);
                    break;
                default:
                    VRC_EditorTools.FilteredEnumPopup<VRCSDK2.VRC_DataStorage.VrcDataType>(typeValueRect, typeProperty, t => true);
                    break;
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomEditor(typeof(VRCSDK2.VRC_DataStorage)), CanEditMultipleObjects]
    public class VRC_DataStorageEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            VRCSDK2.VRC_ObjectSync os = ((VRCSDK2.VRC_DataStorage)target).GetComponent<VRCSDK2.VRC_ObjectSync>();
            if (os != null && os.SynchronizePhysics)
                EditorGUILayout.HelpBox("Consider either removing the VRC_ObjectSync or disabling SynchronizePhysics.", MessageType.Warning);

            DrawDefaultInspector();
        }
    }
}
#endif