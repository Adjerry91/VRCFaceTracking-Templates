#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;
using System.Reflection;

public partial class AvatarDescriptorEditor3 : Editor
{

    SerializedProperty _baseAnimLayers { get { return serializedObject.FindProperty("baseAnimationLayers"); } }
    SerializedProperty _specialAnimLayers { get { return serializedObject.FindProperty("specialAnimationLayers"); } }
    SerializedProperty _doCustomizeAnimLayers { get { return serializedObject.FindProperty("customizeAnimationLayers"); } }

    Animator _animator
    {
        get
        {
            if (_cachedAnimator == null)
                _cachedAnimator = (target as Component).GetComponent<Animator>();
            return _cachedAnimator;
        }
    }
    private Animator _cachedAnimator = null;

    const float ANIM_LAYER_LIST_MARGIN = 5f;

    void DrawPlayableLayers()
    {
        if (Foldout("VRCSDK3_AvatarDescriptorEditor3_AnimationFoldout", "Playable Layers"))
        {
            if (_doCustomizeAnimLayers.boolValue)
            {
                if (GUILayout.Button("Reset to Default"))
                {
                    if (EditorUtility.DisplayDialog("Reset to Default", "This will erase any custom layer settings. Are you sure?", "OK", "Cancel"))
                    {
                        ResetAnimLayersToDefault();
                        _doCustomizeAnimLayers.boolValue = false;
                    }
                }

                DrawAnimLayerList("Base", _baseAnimLayers, Color.white);
                DrawAnimLayerList("Special", _specialAnimLayers, Color.black);

                GUILayout.Space(10);

                if (GUI.changed)
                    EnforceAnimLayerSetup();
            }
            else
            {
                if (GUILayout.Button("Customize"))
                {
                    ResetAnimLayersToDefault();
                    _doCustomizeAnimLayers.boolValue = true;
                }
            }

            Separator();
        }
    }

    void DrawLowerBodySettings()
    {
        if (_animator && _animator.isHuman)
        {
            if (Foldout("VRCSDK3_AvatarDescriptorEditor3_LowerBodyFoldout", "Lower Body"))
            {
                var autoFoot = serializedObject.FindProperty("autoFootsteps");
                var autoLoco = serializedObject.FindProperty("autoLocomotion");
                autoFoot.boolValue = EditorGUILayout.ToggleLeft("Use Auto-Footsteps for 3 and 4 point tracking", autoFoot.boolValue);
                autoLoco.boolValue = EditorGUILayout.ToggleLeft("Force Locomotion animations for 6 point tracking", autoLoco.boolValue);
            }

            Separator();
        }
    }

    void DrawAnimLayerList(string label, SerializedProperty list, Color boxColor, string buttonProperty = null)
    {
        BeginBox(label);

        bool isBaseLayer = (list.name == "baseAnimationLayers");
        bool isNonHumanBaseLayer = (isBaseLayer && (_animator && !_animator.isHuman));

        for (int v = 0; v < list.arraySize; v++)
        {
            SerializedProperty layer = list.GetArrayElementAtIndex(v);
            var type = layer.FindPropertyRelative("type");

            DrawAnimLayerListElement(" " + System.Enum.GetName(typeof(VRCAvatarDescriptor.AnimLayerType), type.enumValueIndex), layer, isNonHumanBaseLayer);
        }

        GUILayout.Space(10);

        EndBox();
    }

    void DeleteAnimLayers(SerializedProperty list, List<int> layersToDelete)
    {
        for (int v = (list.arraySize - 1); v > -1; v--)
        {
            if(layersToDelete.Contains(v))
                list.DeleteArrayElementAtIndex(v);
        }
    }

    // returns false if layer should be deleted
    bool DrawAnimLayerListElement(string label, SerializedProperty layer, bool isNonHumanBaseLayer = false)
    {
        bool keepLayer = true;

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(100));

        var isDefault = layer.FindPropertyRelative("isDefault");
        var type = layer.FindPropertyRelative("type");
        var controller = layer.FindPropertyRelative("animatorController");

        if (isDefault.boolValue && !isNonHumanBaseLayer)
        {
            // set next control name & focus the button if pressed
            // (this is to reset 'Special' layer buttons to default if left empty in 'EnforceAnimLayerSetup')
            string buttonName = GetAnimLayerButtonName((VRCAvatarDescriptor.AnimLayerType)type.enumValueIndex);
            GUI.SetNextControlName(buttonName);
            if (GUILayout.Button(buttonName, GUILayout.MinWidth(0)))    // minwidth 0 prevents buttons from breaking margin on small inspector size
            {
                isDefault.boolValue = false;
                GUI.FocusControl(buttonName);
            }
        }
        else
        {
            float maxWidth = (Screen.width -170);   // ObjectField with SerializedProperty requires some scaling lore

            GUILayout.BeginHorizontal();
            Object prev = controller.objectReferenceValue;
            EditorGUILayout.ObjectField(controller, typeof(RuntimeAnimatorController), GUIContent.none, GUILayout.MaxWidth(maxWidth));
            if(controller.objectReferenceValue != prev)
                isDefault.boolValue = !controller.objectReferenceValue;
            GUILayout.EndHorizontal();
        }
        if (isDefault.boolValue)
            GUI.enabled = false;
        if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
        {
            GUI.FocusControl(null); // resets 'Special' layer buttons to default if left empty in 'EnforceAnimLayerSetup'
            controller.objectReferenceValue = null;
            isDefault.boolValue = true;
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        return keepLayer;
    }

}
#endif
