using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDK2
{
    //[CustomPropertyDrawer(typeof(VRC_AvatarVariations.VariationCategory))]
    //public class PropertyDrawer_AvatarVariation_VariationCategory : PropertyDrawer
    //{
    //    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    //    {
    //        //EditorGUILayout.Label("blah");

    //        if (property == null)
    //            return;

    //        SerializedProperty nameProperty = property.FindPropertyRelative("name");
    //        //SerializedProperty mirrorProperty = property.FindPropertyRelative("mirror");
    //        //SerializedProperty typeProperty = property.FindPropertyRelative("type");
    //        //SerializedProperty valueProperty = null;
    //        //switch (typeProperty.enumValueIndex)
    //        //{
    //        //    case (int)VRC_DataStorage.VrcDataType.Bool:
    //        //        valueProperty = property.FindPropertyRelative("valueBool");
    //        //        break;
    //        //    case (int)VRC_DataStorage.VrcDataType.Float:
    //        //        valueProperty = property.FindPropertyRelative("valueFloat");
    //        //        break;
    //        //    case (int)VRC_DataStorage.VrcDataType.Int:
    //        //        valueProperty = property.FindPropertyRelative("valueInt");
    //        //        break;
    //        //    case (int)VRC_DataStorage.VrcDataType.String:
    //        //        valueProperty = property.FindPropertyRelative("valueString");
    //        //        break;
    //        //    case (int)VRC_DataStorage.VrcDataType.SerializeObject:
    //        //        valueProperty = property.FindPropertyRelative("serializeComponent");
    //        //        break;
    //        //    case (int)VRC_DataStorage.VrcDataType.None:
    //        //    case (int)VRC_DataStorage.VrcDataType.SerializeBytes:
    //        //        break;
    //        //}

    //        EditorGUI.BeginProperty(rect, label, property);

    //        int baseWidth = (int)(rect.width / 4);
    //        Rect nameRect = new Rect(rect.x, rect.y, baseWidth, rect.height);
    //        //Rect mirrorRect = new Rect(rect.x + baseWidth, rect.y, baseWidth, rect.height);
    //        //Rect typeRect = new Rect(rect.x + baseWidth * 2, rect.y, baseWidth, rect.height);
    //        //Rect valueRect = new Rect(rect.x + baseWidth * 3, rect.y, baseWidth, rect.height);
    //        //Rect typeValueRect = new Rect(rect.x + baseWidth * 2, rect.y, baseWidth * 2, rect.height);

    //        EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
    //        //EditorGUI.PropertyField(mirrorRect, mirrorProperty, GUIContent.none);

    //        //switch (mirrorProperty.enumValueIndex)
    //        //{
    //        //    case (int)VRC_DataStorage.VrcDataMirror.None:
    //        //        if (valueProperty == null)
    //        //            VRC_EditorTools.FilteredEnumPopup<VRC_DataStorage.VrcDataType>(typeValueRect, typeProperty, t => true);
    //        //        else
    //        //        {
    //        //            VRC_EditorTools.FilteredEnumPopup<VRC_DataStorage.VrcDataType>(typeRect, typeProperty, t => true);
    //        //            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
    //        //        }
    //        //        break;
    //        //    case (int)VRC_DataStorage.VrcDataMirror.SerializeComponent:
    //        //        typeProperty.enumValueIndex = (int)VRC_DataStorage.VrcDataType.SerializeObject;
    //        //        EditorGUI.PropertyField(typeValueRect, valueProperty, GUIContent.none);
    //        //        break;
    //        //    default:
    //        //        VRC_EditorTools.FilteredEnumPopup<VRC_DataStorage.VrcDataType>(typeValueRect, typeProperty, t => true);
    //        //        break;
    //        //}

    //        EditorGUI.EndProperty();
    //    }
    //}

    //[CustomEditor(typeof(VRC_AvatarVariations))]
    //public class VRC_AvatarVariationsEditor : Editor
    //{
    //    SerializedProperty categories;

    //    void OnEnable()
    //    {
    //        categories = serializedObject.FindProperty("categories");
    //    }

    //    public override void OnInspectorGUI()
    //    {
    //        //serializedObject.Update();
    //        // EditorGUILayout.PropertyField(categories);
    //        //serializedObject.ApplyModifiedProperties();



    //        //if (target == null)
    //        //    return;

    //        ////var prop = serializedObject.FindProperty("root");
    //        ////EditorGUILayout.PropertyField(prop, new GUIContent("Show Help"));
    //        //VRCSDK2.VRC_AvatarVariations variations = target as VRCSDK2.VRC_AvatarVariations;
    //        //if (variations.categories == null)
    //        //    variations.categories = new VRC_AvatarVariations.VariationCategory[0];

    //        //foreach ( var vc in variations.categories )
    //        //{
    //        //    vc.name = EditorGUILayout.TextField("Variation Name", vc.name);
    //        ////    SerializedProperty triggers = triggersProperty.Copy();
    //        ////    int triggersLength = triggers.arraySize;

    //        ////    List<int> to_remove = new List<int>();
    //        ////    for (int idx = 0; idx < triggersLength; ++idx)
    //        ////    {
    //        ////        SerializedProperty triggerProperty = triggers.GetArrayElementAtIndex(idx);
    //        ////    }

    //        ////        EditorGUILayout.LabelField("");
    //        //////    helpProperty = serializedObject.FindProperty("ShowHelp");
    //        //////    EditorGUILayout.PropertyField(helpProperty, new GUIContent("Show Help"));
    //        //}

    //        ////EditorGUILayout.

    //        DrawDefaultInspector();
    //    }
    //}
    
}
