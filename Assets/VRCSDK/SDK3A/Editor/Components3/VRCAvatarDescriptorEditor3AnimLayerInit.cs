#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;

public partial class AvatarDescriptorEditor3 : Editor
{

    List<EditorAnimLayerInfo> baseAnimLayerTypes = new List<EditorAnimLayerInfo>
    {
        {new EditorAnimLayerInfo("Locomotion", VRCAvatarDescriptor.AnimLayerType.Base)},
        {new EditorAnimLayerInfo("Idle", VRCAvatarDescriptor.AnimLayerType.Additive)},
        {new EditorAnimLayerInfo("Gesture", VRCAvatarDescriptor.AnimLayerType.Gesture)},
        {new EditorAnimLayerInfo("Action", VRCAvatarDescriptor.AnimLayerType.Action)},
        {new EditorAnimLayerInfo("Non-Transform", VRCAvatarDescriptor.AnimLayerType.FX)},
    };

    List<EditorAnimLayerInfo> specialAnimLayerTypes = new List<EditorAnimLayerInfo>
    {
        {new EditorAnimLayerInfo("Sitting", VRCAvatarDescriptor.AnimLayerType.Sitting)},
        {new EditorAnimLayerInfo("TPose", VRCAvatarDescriptor.AnimLayerType.TPose)},
        {new EditorAnimLayerInfo("IKPose", VRCAvatarDescriptor.AnimLayerType.IKPose)},
    };

    Dictionary<VRCAvatarDescriptor.AnimLayerType, string> _animLayerButtonNames = new Dictionary<VRCAvatarDescriptor.AnimLayerType, string>();

    struct EditorAnimLayerInfo
    {
        public EditorAnimLayerInfo(string label, VRCAvatarDescriptor.AnimLayerType type)
        {
            this.label = label;
            this.type = type;
        }

        public string label;
        public VRCAvatarDescriptor.AnimLayerType type;
    }

    string GetAnimLayerButtonName(VRCAvatarDescriptor.AnimLayerType layerType)
    {
        string name = null;
        _animLayerButtonNames.TryGetValue(layerType, out name);
        if (string.IsNullOrEmpty(name))
        {
            foreach (EditorAnimLayerInfo info in baseAnimLayerTypes)
            {
                if (info.type == layerType)
                {
                    if (_animator && (!_animator.isHuman) && info.label == "Face")
                        name = "Default BlendShapes";
                    else
                        name = ("Default " + info.label);
                    _animLayerButtonNames.Add(layerType, name);
                    return name;
                }
            }
            foreach (EditorAnimLayerInfo info in specialAnimLayerTypes)
            {
                if (info.type == layerType)
                {
                    name = ("Default " + info.label);
                    _animLayerButtonNames.Add(layerType, name);
                    return name;
                }
            }
        }
        return name;
    }

    bool IsHumanOnlyAnimLayer(VRCAvatarDescriptor.AnimLayerType layerType)
    {
        switch (layerType)
        {
            case VRCAvatarDescriptor.AnimLayerType.Additive: return true;
            case VRCAvatarDescriptor.AnimLayerType.Gesture: return true;
        }
        return false;
    }

    void ResetAnimLayersToDefault()
    {
        var b = serializedObject.FindProperty("baseAnimationLayers");
        b.ClearArray();
        var s = serializedObject.FindProperty("specialAnimationLayers");
        s.ClearArray();

        foreach (EditorAnimLayerInfo info in baseAnimLayerTypes)
        {
            if (_animator && (!_animator.isHuman) && IsHumanOnlyAnimLayer(info.type))
                continue;
            InitAnimLayer(serializedObject.FindProperty("baseAnimationLayers"), info.type, true);
        }

        foreach (EditorAnimLayerInfo info in specialAnimLayerTypes)
        {
            if (_animator && (!_animator.isHuman) && IsHumanOnlyAnimLayer(info.type))
                continue;
            InitAnimLayer(serializedObject.FindProperty("specialAnimationLayers"), info.type, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void SetLayerMaskFromController(SerializedProperty layer)
    {
// VRC DaveT: avoid exception when observing during runtime
#if !VRC_CLIENT
        var isDefault = layer.FindPropertyRelative("isDefault").boolValue;
        if (!isDefault)
        {
            var maskProp = layer.FindPropertyRelative("mask");
            var controller = layer.FindPropertyRelative("animatorController").objectReferenceValue as AnimatorController;
            if (controller != null)
            {
                maskProp.objectReferenceValue = controller.layers[0].avatarMask;
            }
        }
#endif
    }

    void InitAnimLayer(SerializedProperty list, VRCAvatarDescriptor.AnimLayerType type, bool isDefault, int index = -1)
    {
        list.InsertArrayElementAtIndex(list.arraySize);
        var element = list.GetArrayElementAtIndex((index == -1) ? (list.arraySize - 1) : index);

        element.FindPropertyRelative("type").enumValueIndex = (int)type;
        element.FindPropertyRelative("isDefault").boolValue = isDefault;
    }

    void EnforceAnimLayerSetup(bool isOnEnable = false)
    {
        if (_animator)
        {
            if (_animator.isHuman)
            {
                // human -> add missing base layers from previous non-human setup
                bool haveAdditive = false;
                bool haveGesture = false;

                for (int v = 0; v < _baseAnimLayers.arraySize; v++)
                {
                    SerializedProperty layer = _baseAnimLayers.GetArrayElementAtIndex(v);
                    SerializedProperty type = layer.FindPropertyRelative("type");
                    if ((VRCAvatarDescriptor.AnimLayerType)type.enumValueIndex == VRCAvatarDescriptor.AnimLayerType.Additive)
                    {
                        haveAdditive = true;
                    }
                    if ((VRCAvatarDescriptor.AnimLayerType)type.enumValueIndex == VRCAvatarDescriptor.AnimLayerType.Gesture)
                    {
                        haveGesture = true;
                        SetLayerMaskFromController(layer);
                    }
                }

                if (!haveAdditive)
                    InitAnimLayer(_baseAnimLayers, VRCAvatarDescriptor.AnimLayerType.Additive, true, 1);

                if (!haveGesture)
                    InitAnimLayer(_baseAnimLayers, VRCAvatarDescriptor.AnimLayerType.Gesture, true, 2);
            }
            else
            {
                // non-human -> remove excess base layers from previous human setup
                List<int> baseLayersToDelete = new List<int>();
                for (int v = 0; v < _baseAnimLayers.arraySize; v++)
                {
                    SerializedProperty layer = _baseAnimLayers.GetArrayElementAtIndex(v);
                    SerializedProperty type = layer.FindPropertyRelative("type");
                    VRCAvatarDescriptor.AnimLayerType t = (VRCAvatarDescriptor.AnimLayerType)type.enumValueIndex;
                    if (IsHumanOnlyAnimLayer(t))
                        baseLayersToDelete.Add(v);
                }
                if (baseLayersToDelete.Count > 0)
                    DeleteAnimLayers(_baseAnimLayers, baseLayersToDelete);
            }
        }

        // reset 'Special' layer buttons to default when left empty
        for (int v = 0; v < _specialAnimLayers.arraySize; v++)
        {
            SerializedProperty layer = _specialAnimLayers.GetArrayElementAtIndex(v);
            SerializedProperty type = layer.FindPropertyRelative("type");
            SerializedProperty isDefault = layer.FindPropertyRelative("isDefault");
            SerializedProperty animatorController = layer.FindPropertyRelative("animatorController");

            if ((!isDefault.boolValue) && (animatorController.objectReferenceValue == null))
            {
                if (isOnEnable || (GUI.GetNameOfFocusedControl() != GetAnimLayerButtonName((VRCAvatarDescriptor.AnimLayerType)type.enumValueIndex)))
                    isDefault.boolValue = true;
            }
        }
    }

}
#endif
