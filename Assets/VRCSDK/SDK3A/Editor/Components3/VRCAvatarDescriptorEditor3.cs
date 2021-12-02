#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Editor;
using VRC.SDKBase.Editor;

[CustomEditor(typeof(VRCAvatarDescriptor))]
public partial class AvatarDescriptorEditor3 : Editor
{

    VRCAvatarDescriptor avatarDescriptor;
    VRC.Core.PipelineManager pipelineManager;

    static bool _repaint = false;

    public void OnEnable()
    {
        /*
        if (EyelidSkinnedMesh == null)
            EyelidSkinnedMesh = (target as Component).GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (EyelidBlendShapes == null)
            EyelidBlendShapes = GetBlendShapeNames(EyelidSkinnedMesh);
            */

        if (avatarDescriptor == null)
            avatarDescriptor = (VRCAvatarDescriptor)target;

        if (pipelineManager == null)
        {
            pipelineManager = avatarDescriptor.GetComponent<VRC.Core.PipelineManager>();
            if (pipelineManager == null)
                avatarDescriptor.gameObject.AddComponent<VRC.Core.PipelineManager>();
        }

        if (!_doCustomizeAnimLayers.boolValue)
            ResetAnimLayersToDefault();

        EnforceAnimLayerSetup(true);
        serializedObject.ApplyModifiedProperties();

        InitEyeLook();
        Init_Expressions();

    }

    public void OnSceneGUI()
    {
        serializedObject.Update();

        //Draw
        DrawSceneViewpoint();
        DrawSceneEyeLook();
        DrawSceneLipSync();

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if(VRCSdkControlPanel.window != null)
        {
            if( GUILayout.Button( "Select this avatar in the SDK control panel" ) )
                VRCSdkControlPanelAvatarBuilder.SelectAvatar(avatarDescriptor);
        }

        DrawView();
        DrawLipSync();
        DrawEyeLook();
        DrawPlayableLayers();
        DrawLowerBodySettings();
        DrawInspector_Expressions();
        DrawFooter();

        serializedObject.ApplyModifiedProperties();

        if (_repaint)
            EditorUtility.SetDirty(target);
    }

    void DrawFooter()
    {
        if (!string.IsNullOrEmpty(avatarDescriptor.unityVersion))
            EditorGUILayout.LabelField("Unity Version: ", avatarDescriptor.unityVersion);
        if (_animator)
            EditorGUILayout.LabelField("Rig Type: ", (_animator.isHuman ? "Humanoid" : "Non-humanoid"));
        GUILayout.Space(5);
    }

    #region GUIHelperMethods

    static bool Foldout(string editorPrefsKey, string label, bool deft = false)
    {
        bool prevState = EditorPrefs.GetBool(editorPrefsKey, deft);
        bool state = EditorGUILayout.Foldout(prevState, label);
        if (state != prevState)
            EditorPrefs.SetBool(editorPrefsKey, state);
        return state;
    }

    public static void Separator()
    {
        GUILayout.Space(5);
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(2));
        r.height = 2;
        r.x -= 10;
        r.width += 20;
        EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.15f));
        GUILayout.Space(5);
    }

    void MinMaxSlider(string label, SerializedProperty minFloat, SerializedProperty maxFloat, float minLimit, float maxLimit, bool rounded = true)
    {
        float min = minFloat.floatValue;
        float max = maxFloat.floatValue;

        EditorGUI.BeginChangeCheck();
        MinMaxSlider(label, ref min, ref max, minLimit, maxLimit, rounded);
        if (EditorGUI.EndChangeCheck())
        {
            minFloat.floatValue = min;
            maxFloat.floatValue = max;
        }
    }

    void MinMaxSlider(string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool rounded = true)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        minValue = Mathf.Clamp(System.Convert.ToSingle(EditorGUILayout.TextField(minValue.ToString(), GUILayout.MaxWidth(30))), minLimit, maxValue);
        EditorGUILayout.MinMaxSlider(
            ref minValue,
            ref maxValue, minLimit, maxLimit);
        maxValue = Mathf.Clamp(System.Convert.ToSingle(EditorGUILayout.TextField(maxValue.ToString(), GUILayout.MaxWidth(30))), minValue, maxLimit);
        if (rounded)
        {
            minValue = Mathf.Round(minValue);
            maxValue = Mathf.Round(maxValue);
        }
        EditorGUILayout.EndHorizontal();
    }

    static GUIStyle boxStyle;
    static void BeginBox(string label, bool hasFoldouts = false)
    {
        Rect rect = EditorGUILayout.BeginVertical();
        GUILayout.Space(ANIM_LAYER_LIST_MARGIN * 2);
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Space(ANIM_LAYER_LIST_MARGIN + (hasFoldouts ? 8:0));
        EditorGUILayout.BeginVertical();

        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(-(hasFoldouts ? 8 : 0));
            GUILayout.Label(label, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
        }
    }

    static void EndBox()
    {
        EditorGUILayout.EndVertical();
        GUILayout.Space(ANIM_LAYER_LIST_MARGIN);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // encompassing boxes must be drawn last
        /*Rect rect2 = GUILayoutUtility.GetLastRect();
        rect2.y -= ANIM_LAYER_LIST_MARGIN;
        rect2.height += (ANIM_LAYER_LIST_MARGIN * 2.35f);
        GUI.color = boxColor;
        for (int v = 0; v < (boxColor == Color.black ? 3 : 1); v++)
        {
            GUI.Box(rect2, GUIContent.none);
        }
        GUI.color = Color.white;
        EditorGUILayout.EndVertical();
        GUILayout.Space(ANIM_LAYER_LIST_MARGIN * 2);*/
    }

    #endregion

}
#endif
