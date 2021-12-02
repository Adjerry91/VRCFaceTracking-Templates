#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;
using System;
using System.Linq;


public partial class AvatarDescriptorEditor3 : Editor
{
    static string _eyeLookFoldoutPrefsKey = "VRCSDK3_AvatarDescriptorEditor3_EyeLookFoldout";
    static string _eyeMovementFoldoutPrefsKey = "VRCSDK3_AvatarDescriptorEditor3_EyeLookFoldout_Movement";
    static string _eyeTransformFoldoutPrefsKey = "VRCSDK3_AvatarDescriptorEditor3_EyeTransformFoldout";
    static string _eyeRotationFoldoutPrefsKey = "VRCSDK3_AvatarDescriptorEditor3_EyeRotationFoldout";
    static string _eyelidTransformFoldoutPrefsKey = "VRCSDK3_AvatarDescriptorEditor3_EyelidTransformFoldout";
    static string _eyelidRotationFoldoutPrefsKey = "VRCSDK3_AvatarDescriptorEditor3_EyelidRotationFoldout";
    static string _eyelidBlendshapesFoldoutPrefsKey = "VRCSDK3_AvatarDescriptorEditor3_EyeLookFoldout_EyelidBlendshapes";

    static string _linkEyelidsDialog = "Link Left + Right for '{0}'?\n(using values of: {1})";

    static string _activeProperty = null;
    static Color _activeButtonColor = new Color(0.5f, 1, 0.5f, 1);

    SkinnedMeshRenderer _currentEyelidsMesh;
    string[] _eyelidBlendshapeNames;

    static Texture _linkIcon;

    static List<System.Action> activePropertyRestore = new List<System.Action>();

    void InitEyeLook()
    {
        if (_linkIcon == null)
            _linkIcon = Resources.Load<Texture>("EditorUI_Icons/EditorUI_Link");

        EditorPrefs.SetBool(_eyeTransformFoldoutPrefsKey, true);
        EditorPrefs.SetBool(_eyelidTransformFoldoutPrefsKey, true);
    }

    void DrawEyeLook()
    {
        if (Foldout(_eyeLookFoldoutPrefsKey, "Eye Look"))
        {
            SerializedProperty p = serializedObject.FindProperty("enableEyeLook");

            bool toggle = GUILayout.Button(p.boolValue ? "Disable" : "Enable");

            if (toggle)
                p.boolValue = !p.boolValue;

            if (p.boolValue)
            {
                var eyeSettings = serializedObject.FindProperty("customEyeLookSettings");

                EditorGUILayout.BeginVertical();

                DrawEyesGeneralBox(eyeSettings);
                DrawEyesBox(eyeSettings);
                DrawEyelidsBox(eyeSettings);

                EditorGUILayout.EndVertical();
            }

            Separator();
        }
    }

    static void DrawEyesGeneralBox(SerializedProperty eyeSettings)
    {

        BeginBox("General", true);

        if (Foldout(_eyeMovementFoldoutPrefsKey, "Eye Movements"))
        {
            var eyeMovement = eyeSettings.FindPropertyRelative("eyeMovement");
            var confidence = eyeMovement.FindPropertyRelative("confidence");
            var excitement = eyeMovement.FindPropertyRelative("excitement");

            EyeLookMovementSlider(excitement, "Calm", "Excited");
            EyeLookMovementSlider(confidence, "Shy", "Confident");
        }

        EndBox();
    }

    void DrawEyesBox(SerializedProperty eyeSettings)
    {
        BeginBox("Eyes", true);

        var leftEye = eyeSettings.FindPropertyRelative("leftEye");
        var rightEye = eyeSettings.FindPropertyRelative("rightEye");

        if (Foldout(_eyeTransformFoldoutPrefsKey, "Transforms", true))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(leftEye, new GUIContent("Left Eye Bone"));
            EditorGUILayout.PropertyField(rightEye, new GUIContent("Right Eye Bone"));
            if(EditorGUI.EndChangeCheck())
                SetActiveProperty(null); //Disable active property, maintains proper preview/undo state
        }
        Separator();

        EditorGUI.BeginDisabledGroup(leftEye.objectReferenceValue == null || rightEye.objectReferenceValue == null);
        if (Foldout(_eyeRotationFoldoutPrefsKey, "Rotation States"))
        {
            var eyesLookingStraight = eyeSettings.FindPropertyRelative("eyesLookingStraight");
            var eyesLookingUp = eyeSettings.FindPropertyRelative("eyesLookingUp");
            var eyesLookingDown = eyeSettings.FindPropertyRelative("eyesLookingDown");
            var eyesLookingLeft = eyeSettings.FindPropertyRelative("eyesLookingLeft");
            var eyesLookingRight = eyeSettings.FindPropertyRelative("eyesLookingRight");

            RotationFieldEyeLook(eyesLookingStraight, leftEye, rightEye, "Looking Straight");
            RotationFieldEyeLook(eyesLookingUp, leftEye, rightEye, "Looking Up", () => { BeginEyesUpDown(true); });
            RotationFieldEyeLook(eyesLookingDown, leftEye, rightEye, "Looking Down", () => { BeginEyesUpDown(false); });
            RotationFieldEyeLook(eyesLookingLeft, leftEye, rightEye, "Looking Left");
            RotationFieldEyeLook(eyesLookingRight, leftEye, rightEye, "Looking Right");
        }
        EditorGUI.EndDisabledGroup();

        EndBox();
    }
    void RotationFieldEyeLook(SerializedProperty property, SerializedProperty leftEyeBone, SerializedProperty rightEyeBone, string label, System.Action onSetActive=null)
    {
        RotationField(property, leftEyeBone, rightEyeBone, label, isActive: IsActiveProperty(property), SetActive: SetActive);
        void SetActive()
        {
            //Set
            SetActiveProperty(property);

            //Check for transforms
            if (((Transform)leftEyeBone.objectReferenceValue) == null || ((Transform)rightEyeBone.objectReferenceValue) == null)
                return;

            //Record
            RecordEyeRotations(property, leftEyeBone, rightEyeBone);

            //Other
            onSetActive?.Invoke();
        }
    }

    void DrawEyelidsBox(SerializedProperty eyeSettings)
    {
        BeginBox("Eyelids", true);

        DrawEyelidType(eyeSettings);

        if (avatarDescriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes)
        {
            DrawEyelidBlendshapeDropdowns(eyeSettings);
        }
        else if (avatarDescriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Bones) 
        {
            DrawEyelidBoneRotations(eyeSettings);
            _currentEyelidsMesh = null;
        }
        else
        {
            _currentEyelidsMesh = null;
        }

        EndBox();
    }
    void DrawEyelidType(SerializedProperty eyeSettings)
    {
        EditorGUI.BeginChangeCheck();
        var eyelidType = eyeSettings.FindPropertyRelative("eyelidType");
        EditorGUILayout.PropertyField(eyelidType);
        if (EditorGUI.EndChangeCheck())
        {
            if (eyelidType.enumValueIndex == (int)VRCAvatarDescriptor.EyelidType.Blendshapes)
                EditorPrefs.SetBool(_eyelidBlendshapesFoldoutPrefsKey, true);
        }
    }
    void DrawEyelidBoneRotations(SerializedProperty eyeSettings)
    {
        Separator();

        var upperLeftEyelid = eyeSettings.FindPropertyRelative("upperLeftEyelid");
        var upperRightEyelid = eyeSettings.FindPropertyRelative("upperRightEyelid");
        var lowerLeftEyelid = eyeSettings.FindPropertyRelative("lowerLeftEyelid");
        var lowerRightEyelid = eyeSettings.FindPropertyRelative("lowerRightEyelid");

        if (Foldout(_eyelidTransformFoldoutPrefsKey, "Transforms", true))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(upperLeftEyelid);
            EditorGUILayout.PropertyField(upperRightEyelid);
            EditorGUILayout.PropertyField(lowerLeftEyelid);
            EditorGUILayout.PropertyField(lowerRightEyelid);
            if (EditorGUI.EndChangeCheck())
                SetActiveProperty(null); //Disable active property, maintains proper preview/undo state
        }
        Separator();
        if (Foldout(_eyelidRotationFoldoutPrefsKey, "Rotation States"))
        {
            var eyelidsDefault = eyeSettings.FindPropertyRelative("eyelidsDefault");
            var eyelidsClosed = eyeSettings.FindPropertyRelative("eyelidsClosed");
            var eyelidsLookingUp = eyeSettings.FindPropertyRelative("eyelidsLookingUp");
            var eyelidsLookingDown = eyeSettings.FindPropertyRelative("eyelidsLookingDown");

            GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.BeginVertical();
            RotationFieldEyelids(eyelidsDefault, upperLeftEyelid, upperRightEyelid, lowerLeftEyelid, lowerRightEyelid, "Default");
            RotationFieldEyelids(eyelidsClosed, upperLeftEyelid, upperRightEyelid, lowerLeftEyelid, lowerRightEyelid, "Closed");
            RotationFieldEyelids(eyelidsLookingUp, upperLeftEyelid, upperRightEyelid, lowerLeftEyelid, lowerRightEyelid, "Looking Up", () => { BeginEyesUpDown(true); } );
            RotationFieldEyelids(eyelidsLookingDown, upperLeftEyelid, upperRightEyelid, lowerLeftEyelid, lowerRightEyelid, "Looking Down", () => { BeginEyesUpDown(false); });

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
    void RotationFieldEyelids(SerializedProperty property,
        SerializedProperty upperLeftBone, SerializedProperty upperRightBone,
        SerializedProperty lowerLeftBone, SerializedProperty lowerRightBone,
        string label = null,
        System.Action onSetActive=null)
    {
        var upperProperty = property.FindPropertyRelative("upper");
        var lowerProperty = property.FindPropertyRelative("lower");
        GUILayout.BeginHorizontal();
        if (EditorGUILayout.PropertyField(property, new GUIContent(label), GUILayout.MinWidth(100)))
        {
            Button();
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();

            RotationField(upperProperty, upperLeftBone, upperRightBone, "Upper Eyelids", showButton:false, isActive:IsActiveProperty(property), SetActive:SetActive);
            RotationField(lowerProperty, lowerLeftBone, lowerRightBone, "Lower Eyelids", showButton:false, isActive:IsActiveProperty(property), SetActive:SetActive);
            GUILayout.EndVertical();
        }
        else
        {
            Button();
            GUILayout.EndHorizontal();
        }

        void SetActive()
        {
            SetActiveProperty(property);

            //Record
            RecordEyeRotations(upperProperty, upperLeftBone, upperRightBone);
            RecordEyeRotations(lowerProperty, lowerLeftBone, lowerRightBone);

            //Other
            onSetActive?.Invoke();
        }

        void Button()
        {
            bool isActiveProperty = IsActiveProperty(property);
            GUI.backgroundColor = isActiveProperty ? _activeButtonColor : Color.white;
            if (GUILayout.Button(isActiveProperty ? "Return" : "Preview", EditorStyles.miniButton, GUILayout.MaxWidth(PreviewButtonWidth), GUILayout.Height(PreviewButtonHeight)))
            {
                if(isActiveProperty)
                {
                    SetActiveProperty(null);
                }
                else
                {
                    SetActive();
                }
            }
            GUI.backgroundColor = Color.white;
        }
    }
    void DrawEyelidBlendshapeDropdowns(SerializedProperty eyeSettings)
    {
        Separator();

        var eyelidsMeshProp = eyeSettings.FindPropertyRelative("eyelidsSkinnedMesh");
        eyelidsMeshProp.objectReferenceValue = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Eyelids Mesh", eyelidsMeshProp.objectReferenceValue, typeof(SkinnedMeshRenderer), true);

        if (eyelidsMeshProp.objectReferenceValue == null)
        {
            _eyelidBlendshapeNames = null;
            return;
        }

        if (_currentEyelidsMesh == null)
        {
            _currentEyelidsMesh = (SkinnedMeshRenderer)eyelidsMeshProp.objectReferenceValue;
            _eyelidBlendshapeNames = GetBlendShapeNames(_currentEyelidsMesh);
        }

        var eyelidsBlendshapes = eyeSettings.FindPropertyRelative("eyelidsBlendshapes");

        if (Foldout(_eyelidBlendshapesFoldoutPrefsKey, "Blendshape States"))
        {

            if (eyelidsBlendshapes.arraySize != 3)
                eyelidsBlendshapes.arraySize = 3;
            int[] indices = new int[] { eyelidsBlendshapes.GetArrayElementAtIndex(0).intValue,
                                        eyelidsBlendshapes.GetArrayElementAtIndex(1).intValue,
                                        eyelidsBlendshapes.GetArrayElementAtIndex(2).intValue};

            PreviewBlendshapeField("Blink", 0, eyelidsBlendshapes.GetArrayElementAtIndex(0));
            PreviewBlendshapeField("Looking Up", 1, eyelidsBlendshapes.GetArrayElementAtIndex(1), () => { BeginEyesUpDown(true); });
            PreviewBlendshapeField("Looking Down", 2, eyelidsBlendshapes.GetArrayElementAtIndex(2), () => { BeginEyesUpDown(false); });
        }
    }

    /*static float NormalizedDegAngle ( float degrees )
    {
        int factor = (int) (degrees/360);
        degrees -= factor * 360;
        if ( degrees > 180 )
            return degrees - 360;

        if ( degrees < -180 )
            return degrees + 360;

        return degrees;
    }
    static Vector3 NormalizedEulers(Vector3 eulers)
    {
        Vector3 normEulers;
        normEulers.x = NormalizedDegAngle(eulers.x);
        normEulers.y = NormalizedDegAngle(eulers.y);
        normEulers.z = NormalizedDegAngle(eulers.z);

        return normEulers;
    }*/
    static int PreviewButtonWidth = 55;
    static int PreviewButtonHeight = 24;

    static void RecordEyeRotations(SerializedProperty property, SerializedProperty leftEyeBone, SerializedProperty rightEyeBone)
    {
        //Record restore point
        var transformL = (Transform)leftEyeBone.objectReferenceValue;
        var transformR = (Transform)rightEyeBone.objectReferenceValue;
        var prevRotationL = transformL != null ? transformL.localRotation : Quaternion.identity;
        var prevRotationR = transformR != null ? transformR.localRotation : Quaternion.identity;
        System.Action restore = () =>
        {
            if (transformL != null)
                transformL.localRotation = prevRotationL;
            if (transformR != null)
                transformR.localRotation = prevRotationR;
        };
        activePropertyRestore.Add(restore);

        //Set to value
        var leftRotation = property.FindPropertyRelative("left");
        var rightRotation = property.FindPropertyRelative("right");
        if(transformL != null)
            transformL.localRotation = leftRotation.quaternionValue;
        if (transformR != null)
            transformR.localRotation = rightRotation.quaternionValue;
    }
    void BeginEyesUpDown(bool isUp)
    {
        var eyeSettings = serializedObject.FindProperty("customEyeLookSettings");

        //Record - Eye Up
        {
            var eyesLooking = eyeSettings.FindPropertyRelative(isUp ? "eyesLookingUp" : "eyesLookingDown");
            var leftEye = eyeSettings.FindPropertyRelative("leftEye");
            var rightEye = eyeSettings.FindPropertyRelative("rightEye");
            RecordEyeRotations(eyesLooking, leftEye, rightEye);
        }

        //Record - Eyelid Up Bones
        if (avatarDescriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Bones)
        {
            var upperLeftEyelid = eyeSettings.FindPropertyRelative("upperLeftEyelid");
            var upperRightEyelid = eyeSettings.FindPropertyRelative("upperRightEyelid");
            var lowerLeftEyelid = eyeSettings.FindPropertyRelative("lowerLeftEyelid");
            var lowerRightEyelid = eyeSettings.FindPropertyRelative("lowerRightEyelid");

            var eyelidsLooking = eyeSettings.FindPropertyRelative(isUp ? "eyelidsLookingUp" : "eyelidsLookingDown");
            var upperProperty = eyelidsLooking.FindPropertyRelative("upper");
            var lowerProperty = eyelidsLooking.FindPropertyRelative("lower");

            RecordEyeRotations(upperProperty, upperLeftEyelid, upperRightEyelid);
            RecordEyeRotations(lowerProperty, lowerLeftEyelid, lowerRightEyelid);
        }

        //Record - Eyelid Blendshapes
        if (avatarDescriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes)
        {
            var eyelidsBlendshapes = eyeSettings.FindPropertyRelative("eyelidsBlendshapes");
            var blendshapeIndex = eyelidsBlendshapes.GetArrayElementAtIndex(isUp ? 1 : 2);

            RecordBlendShape(blendshapeIndex.intValue);
        }
    }

    static Vector3 EditorQuaternionToVector3(Quaternion value)
    {
        var result = value.eulerAngles;

        //Fix the axis flipping
        if(Mathf.Approximately(value.eulerAngles.y, 180) && Mathf.Approximately(value.eulerAngles.z, 180))
        {
            if (result.x < 90.0f)
                result.x = 90f + (90f - result.x);
            else
                result.x = 270f + (270f - result.x);
            result.y = 0;
            result.z = 0;
        }

        //Represent angle as -180 to 180
        if (result.x > 180.0f)
            result.x = -(360f-result.x);
        if (result.y > 180.0f)
            result.y = -(360f - result.y);
        if (result.z > 180.0f)
            result.z = -(360f - result.z);

        //Prevent small number in editor, they arn't nessecary here
        if (result.x < 0.001f && result.x > -0.001f)
            result.x = 0f;
        if (result.y < 0.001f && result.y > -0.001f)
            result.y = 0f;
        if (result.z < 0.001f && result.z > -0.001f)
            result.z = 0f;

        //Return
        return result;
    }
    static bool RotationField(SerializedProperty property, SerializedProperty leftEyeBone, SerializedProperty rightEyeBone, string label = null, bool showButton = true, bool isActive=false, System.Action SetActive=null)
    {
        bool dirty = false;

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        var leftRotation = property.FindPropertyRelative("left");
        var rightRotation = property.FindPropertyRelative("right");
        var linked = property.FindPropertyRelative("linked");
        GUILayout.BeginVertical();
        label = string.IsNullOrEmpty(label) ? property.displayName : label;
        if (GUILayout.Button(label, EditorStyles.label))
            dirty = true;
        if (linked.boolValue)
        {
            GUILayout.BeginHorizontal();
            {
                //Link button
                GUI.color = GUI.skin.label.normal.textColor;
                if (GUILayout.Button(new GUIContent(_linkIcon), GUI.skin.label, GUILayout.MaxWidth(16)))
                    linked.boolValue = false;
                GUI.color = Color.white;

                var testA1 = Quaternion.Euler(new Vector3(20, 0, 0)).eulerAngles;
                var testA2 = Quaternion.Euler(new Vector3(45, 0, 0)).eulerAngles;
                var testA3 = Quaternion.Euler(new Vector3(90, 0, 0)).eulerAngles;
                var testA4 = Quaternion.Euler(new Vector3(95, 0, 0)).eulerAngles;
                var testA5 = Quaternion.Euler(new Vector3(120, 0, 0)).eulerAngles;

                var testB1 = EditorQuaternionToVector3(Quaternion.Euler(new Vector3(20, 0, 0)));
                var testB2 = EditorQuaternionToVector3(Quaternion.Euler(new Vector3(45, 0, 0)));
                var testB3 = EditorQuaternionToVector3(Quaternion.Euler(new Vector3(90, 0, 0)));
                var testB4 = EditorQuaternionToVector3(Quaternion.Euler(new Vector3(95, 0, 0)));
                var testB5 = EditorQuaternionToVector3(Quaternion.Euler(new Vector3(120, 0, 0)));

                //Values
                //leftRotation.quaternionValue = EditorGUILayout.Vector3Field(GUIContent.none, QuaternionToVector3(leftRotation.quaternionValue), GUILayout.MinWidth(100)));
                leftRotation.quaternionValue =  Quaternion.Euler( EditorGUILayout.Vector3Field(GUIContent.none, EditorQuaternionToVector3(leftRotation.quaternionValue), GUILayout.MinWidth(100)) );
                rightRotation.quaternionValue = leftRotation.quaternionValue;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.contentOffset = new Vector2(0, -4);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("L", style, GUILayout.MaxHeight(16)))
            {
                string message = string.Format(_linkEyelidsDialog, label, "L");
                if ((rightRotation.quaternionValue == leftRotation.quaternionValue)
                    || EditorUtility.DisplayDialog("Collapse?", message, "Yes", "No"))
                {
                    linked.boolValue = true;
                    rightRotation.quaternionValue = leftRotation.quaternionValue;
                }
            }
            leftRotation.quaternionValue = Quaternion.Euler(EditorGUILayout.Vector3Field(GUIContent.none, EditorQuaternionToVector3(leftRotation.quaternionValue), GUILayout.MinWidth(100)));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("R", style, GUILayout.MaxHeight(16)))
            {
                string message = string.Format(_linkEyelidsDialog, label, "R");
                if ((leftRotation.quaternionValue == rightRotation.quaternionValue)
                    || (EditorUtility.DisplayDialog("Collapse?", message, "Yes", "No")))
                {
                    linked.boolValue = true;
                    leftRotation.quaternionValue = rightRotation.quaternionValue;
                }
            }
            rightRotation.quaternionValue = Quaternion.Euler(EditorGUILayout.Vector3Field(GUIContent.none, EditorQuaternionToVector3(rightRotation.quaternionValue), GUILayout.MinWidth(100)));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        bool changed = EditorGUI.EndChangeCheck();

        //Edit button
        if (showButton)
        {
            GUI.backgroundColor = (isActive ? _activeButtonColor : Color.white);
            GUILayout.BeginVertical();
            GUILayout.Space(linked.boolValue ? 4 : 20);
            if (GUILayout.Button(isActive ? "Return" : "Preview", EditorStyles.miniButton, GUILayout.Width(PreviewButtonWidth), GUILayout.Height(PreviewButtonHeight)))
            {
                if (isActive)
                {
                    SetActiveProperty(null);
                    isActive = false;
                }
                else
                {
                    SetActive();
                    isActive = true;
                }
                dirty = _repaint = true;
            }
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        //Mark active if changed
        if(changed)
        {
            //Set active if not already
            if (!isActive)
            {
                SetActive();
                isActive = true;
            }

            //Mark dirty
            dirty = _repaint = true;
        }

        //Update values, always update if active not only on change.
        //We do this because the user may change values with a control-z/undo operation
        if (isActive)
        {
            //Left
            var leftEyeTransform = (Transform)leftEyeBone.objectReferenceValue;
            if (leftEyeTransform != null)
                leftEyeTransform.localRotation = leftRotation.quaternionValue;

            //Right
            var rightEyeTransform = (Transform)rightEyeBone.objectReferenceValue;
            if (rightEyeTransform != null)
                rightEyeTransform.localRotation = rightRotation.quaternionValue;
        }

        //Return
        return dirty;
    }

    void PreviewBlendshapeField(string label, int buttonIndex, SerializedProperty blendShapeIndex, System.Action onSetActive=null)
    {
        if (_eyelidBlendshapeNames == null)
            return;

        bool setActive = false;
        GUILayout.BeginHorizontal();
        {
            //Dropdown
            EditorGUI.BeginChangeCheck();
            blendShapeIndex.intValue = EditorGUILayout.Popup(label, blendShapeIndex.intValue + 1, _eyelidBlendshapeNames) - 1;
            if (EditorGUI.EndChangeCheck())
            {
                SetActiveProperty(null);
                setActive = true;
            }   

            //Preview
            bool isActiveProperty = IsActiveProperty(blendShapeIndex);
            GUI.backgroundColor = isActiveProperty ? _activeButtonColor : Color.white;
            if (GUILayout.Button(isActiveProperty ? "Return" : "Preview", EditorStyles.miniButton, GUILayout.MaxWidth(PreviewButtonWidth)) || setActive)
            {
                if (isActiveProperty)
                {
                    SetActiveProperty(null);
                }
                else
                {
                    onSetActive?.Invoke();
                    SetActiveProperty(blendShapeIndex);

                    //Record
                    RecordBlendShape(blendShapeIndex.intValue);

                    //Other
                    onSetActive?.Invoke();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.EndHorizontal();
    }
    void RecordBlendShape(int index, float newWeight = 100.0f)
    {
        //Validate
        if (avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh == null || index < 0 || index >= avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.sharedMesh.blendShapeCount)
            return;

        //Record old position
        int oldIndex = index;
        float oldWeight = avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.GetBlendShapeWeight(index);
        System.Action restore = () =>
        {
            avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.SetBlendShapeWeight(oldIndex, oldWeight);
        };
        activePropertyRestore.Add(restore);

        //Set new weight
        avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.SetBlendShapeWeight(index, newWeight);
    }

    void ResetBlendshapes(int[] indices)
    {
        for (int v = 0; v < indices.Length; v++)
        {
            if (indices[v] < 0) continue;
            avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.SetBlendShapeWeight(indices[v], 0);
        }
    }

    static void EyeLookMovementSlider(SerializedProperty property, string minLabel, string maxLabel)
    {

        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.alignment = TextAnchor.MiddleRight;
        style.padding.left = 10;
        style.padding.right = 10;

        GUILayout.BeginHorizontal();

        GUILayout.Label(GUIContent.none);
        Rect r = GUILayoutUtility.GetLastRect();

        // left word
        float leftLabelWidth = r.width * 0.2f;
        float rightLabelWidth = r.width * 0.3f;
        float sliderWidth = r.width * 0.5f;
        r.width = leftLabelWidth;
        GUI.Label(r, minLabel, style);
        r.x += r.width;

        // slider
        r.width = sliderWidth;
        property.floatValue = GUI.HorizontalSlider(r, property.floatValue, 0f, 1f);
        property.floatValue = Mathf.Round(property.floatValue * 10) * 0.1f;
        r.x += r.width;

        // right word
        r.width = rightLabelWidth;
        style.alignment = TextAnchor.MiddleLeft;
        GUI.Label(r, maxLabel, style);

        GUILayout.EndHorizontal();
    }

    static bool IsActiveProperty(SerializedProperty property)
    {
        return (_activeProperty != null) && _activeProperty.Equals(property.propertyPath, System.StringComparison.Ordinal);
    }
    static void SetActiveProperty(SerializedProperty property)
    {
        if (_activeProperty == property?.propertyPath)
            return;

        //Set
        _activeProperty = (property?.propertyPath);

        //Restore previous state
        activePropertyRestore.Reverse(); //Iterate from last to first
        foreach (var restore in activePropertyRestore)
            restore();
        activePropertyRestore.Clear();

        //Redraw
        _repaint = true;
    }

    public static string[] GetBlendShapeNames(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        if (!skinnedMeshRenderer)
            return null;
        string[] names = new string[skinnedMeshRenderer.sharedMesh.blendShapeCount+1];
        names[0] = "-none-";
        for (int v = 0; v < skinnedMeshRenderer.sharedMesh.blendShapeCount; v++)
        {
            names[v+1] = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(v);
        }
        return names;
    }

    void DrawSceneViewpoint()
    {
        var viewPosition = serializedObject.FindProperty("ViewPosition");
        if(IsActiveProperty(viewPosition))
        {
            viewPosition.vector3Value = Handles.PositionHandle(viewPosition.vector3Value, Quaternion.identity);
        }
    }

    void DrawSceneEyeLook()
    {
        var eyeSettings = serializedObject.FindProperty("customEyeLookSettings");
        var leftEye = eyeSettings.FindPropertyRelative("leftEye");
        var rightEye = eyeSettings.FindPropertyRelative("rightEye");

        //Eye Rotation State
        DrawSceneEyes(eyeSettings.FindPropertyRelative("eyesLookingStraight"), leftEye, rightEye);
        DrawSceneEyes(eyeSettings.FindPropertyRelative("eyesLookingUp"), leftEye, rightEye);
        DrawSceneEyes(eyeSettings.FindPropertyRelative("eyesLookingDown"), leftEye, rightEye);
        DrawSceneEyes(eyeSettings.FindPropertyRelative("eyesLookingLeft"), leftEye, rightEye);
        DrawSceneEyes(eyeSettings.FindPropertyRelative("eyesLookingRight"), leftEye, rightEye);

        //Eyelid Rotation States
        if(avatarDescriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Bones)
        {
            var upperLeftEyelid = eyeSettings.FindPropertyRelative("upperLeftEyelid");
            var upperRightEyelid = eyeSettings.FindPropertyRelative("upperRightEyelid");
            var lowerLeftEyelid = eyeSettings.FindPropertyRelative("lowerLeftEyelid");
            var lowerRightEyelid = eyeSettings.FindPropertyRelative("lowerRightEyelid");

            DrawSceneEyelids(eyeSettings.FindPropertyRelative("eyelidsDefault"), upperLeftEyelid, upperRightEyelid, lowerLeftEyelid, lowerRightEyelid);
            DrawSceneEyelids(eyeSettings.FindPropertyRelative("eyelidsClosed"), upperLeftEyelid, upperRightEyelid, lowerLeftEyelid, lowerRightEyelid);
            DrawSceneEyelids(eyeSettings.FindPropertyRelative("eyelidsLookingUp"), upperLeftEyelid, upperRightEyelid, lowerLeftEyelid, lowerRightEyelid);
            DrawSceneEyelids(eyeSettings.FindPropertyRelative("eyelidsLookingDown"), upperLeftEyelid, upperRightEyelid, lowerLeftEyelid, lowerRightEyelid);
        }
    }
    void DrawSceneEyes(SerializedProperty property, SerializedProperty leftEye, SerializedProperty rightEye, bool checkActive=true)
    {
        if (checkActive && !IsActiveProperty(property))
            return;

        var leftRotation = property.FindPropertyRelative("left");
        var rightRotation = property.FindPropertyRelative("right");
        var linked = property.FindPropertyRelative("linked").boolValue;

        bool changeL = DrawRotationHandles(leftEye, leftRotation);
        bool changeR = DrawRotationHandles(rightEye, rightRotation);

        if(linked)
        {
            if(changeL)
            {
                var rotation = leftRotation.quaternionValue;
                (rightEye.objectReferenceValue as Transform).localRotation = rotation;
                rightRotation.quaternionValue = rotation;
            }
            else if (changeR)
            {
                var rotation = rightRotation.quaternionValue;
                (leftEye.objectReferenceValue as Transform).localRotation = rotation;
                leftRotation.quaternionValue = rotation;
            }
        }
    }
    void DrawSceneEyelids(SerializedProperty property, SerializedProperty upperLeftEyelid, SerializedProperty upperRightEyelid, SerializedProperty lowerLeftEyelid, SerializedProperty lowerRightEyelid)
    {
        if (!IsActiveProperty(property))
            return;

        var upperProperty = property.FindPropertyRelative("upper");
        var lowerProperty = property.FindPropertyRelative("lower");

        DrawSceneEyes(upperProperty, upperLeftEyelid, upperRightEyelid, false);
        DrawSceneEyes(lowerProperty, lowerLeftEyelid, lowerRightEyelid, false);
    }
    bool DrawRotationHandles(SerializedProperty transformProperty, SerializedProperty rotationProperty)
    {
        var transform = transformProperty.objectReferenceValue as Transform;
        if (transform == null)
            return false;
        Handles.matrix = Matrix4x4.TRS(transform.position, transform.parent.rotation, Vector3.one);

        transform.localRotation = rotationProperty.quaternionValue;
        var result = Handles.RotationHandle(transform.localRotation, Vector3.zero);
        if (result != transform.localRotation)
        {
            transform.localRotation = result;
            rotationProperty.quaternionValue = result;
            return true;
        }

        Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Handles.color = new Color(1, 1, 1, 0.5f);
        Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 0.01f);

        return false;
    }
}
#endif
