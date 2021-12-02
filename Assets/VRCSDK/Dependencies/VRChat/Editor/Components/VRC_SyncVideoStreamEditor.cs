#if VRC_SDK_VRCSDK2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDKBase;

[CustomPropertyDrawer(typeof(VRCSDK2.VRC_SyncVideoStream.VideoEntry))]
public class CustomVideoStreamEntryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty source = property.FindPropertyRelative("Source");
        SerializedProperty speed = property.FindPropertyRelative("PlaybackSpeed");
        SerializedProperty clip = property.FindPropertyRelative("VideoClip");
        SerializedProperty url = property.FindPropertyRelative("URL");
        SerializedProperty live = property.FindPropertyRelative("SyncType");
        SerializedProperty sync = property.FindPropertyRelative("SyncMinutes");

        return EditorGUI.GetPropertyHeight(source, new GUIContent("Source"), true) + EditorGUIUtility.standardVerticalSpacing
            + EditorGUI.GetPropertyHeight(speed, new GUIContent("Playback Speed"), true) + EditorGUIUtility.standardVerticalSpacing
            + Mathf.Max(EditorGUI.GetPropertyHeight(clip, new GUIContent("VideoClip"), true), EditorGUI.GetPropertyHeight(url, new GUIContent("URL"), true)) + EditorGUIUtility.standardVerticalSpacing
            + EditorGUI.GetPropertyHeight(live, new GUIContent("SyncType"), true) + EditorGUIUtility.standardVerticalSpacing
            + EditorGUI.GetPropertyHeight(sync, new GUIContent("SyncMinutes"), true) + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        SerializedProperty source = property.FindPropertyRelative("Source");
        SerializedProperty speed = property.FindPropertyRelative("PlaybackSpeed");
        SerializedProperty clip = property.FindPropertyRelative("VideoClip");
        SerializedProperty url = property.FindPropertyRelative("URL");
        SerializedProperty live = property.FindPropertyRelative("SyncType");
        SerializedProperty sync = property.FindPropertyRelative("SyncMinutes");

        EditorGUI.BeginProperty(rect, label, property);
        float x = rect.x;
        float y = rect.y;
        float w = rect.width;
        float h = EditorGUI.GetPropertyHeight(source, new GUIContent("Source"), true) + EditorGUIUtility.standardVerticalSpacing;
        VRC_EditorTools.FilteredEnumPopup<UnityEngine.Video.VideoSource>(new Rect(x, y, w, h), source, (e) => e == UnityEngine.Video.VideoSource.Url);
        y += h;

        if (source.enumValueIndex == (int)UnityEngine.Video.VideoSource.Url)
        {
            h = EditorGUI.GetPropertyHeight(url, new GUIContent("URL"), true) + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(new Rect(x, y, w, h), url);
            y += h;
        }
        else
        {
            h = EditorGUI.GetPropertyHeight(clip, new GUIContent("VideoClip"), true) + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(new Rect(x, y, w, h), clip);
            y += h;
        }

        h = EditorGUI.GetPropertyHeight(speed, new GUIContent("Playback Speed"), true) + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(new Rect(x, y, w, h), speed);
        if (speed.floatValue == 0f)
            speed.floatValue = 1f;
        y += h;

        h = EditorGUI.GetPropertyHeight(live, new GUIContent("SyncType"), true) + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(new Rect(x, y, w, h), live);
        y += h;

        h = EditorGUI.GetPropertyHeight(sync, new GUIContent("SyncMinutes"), true) + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(new Rect(x, y, w, h), sync);
        if (sync.floatValue < 1f)
            sync.floatValue = 0;
        y += h;

        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(VRCSDK2.VRC_SyncVideoStream))]
public class SyncVideoStreamEditor : Editor
{
    ReorderableList sourceList;

    public override void OnInspectorGUI()
    {
        SerializedProperty searchRoot = serializedObject.FindProperty("VideoSearchRoot");
        EditorGUILayout.PropertyField(searchRoot);
        SerializedProperty maxQual = serializedObject.FindProperty("MaxStreamQuality");
        EditorGUILayout.PropertyField(maxQual);
        SerializedProperty autoStart = serializedObject.FindProperty("AutoStart");
        EditorGUILayout.PropertyField(autoStart);

        EditorGUILayout.Space();

        sourceList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        SerializedProperty videos = serializedObject.FindProperty("Videos");
        sourceList = new ReorderableList(serializedObject, videos);
        sourceList.drawElementCallback += (Rect rect, int index, bool active, bool focused) =>
        {
            EditorGUI.PropertyField(rect, serializedObject.FindProperty("Videos").GetArrayElementAtIndex(index));
        };
        sourceList.elementHeightCallback += (int index) =>
        {
            SerializedProperty element = serializedObject.FindProperty("Videos").GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element);
        };
        sourceList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Videos");
    }
}

#endif
