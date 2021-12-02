#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;

public partial class AvatarDescriptorEditor3 : Editor
{

    void DrawView()
    {

        if (Foldout("VRCSDK3_AvatarDescriptorEditor3_ViewFoldout", "View"))
        {
            var viewPosition = serializedObject.FindProperty("ViewPosition");

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(viewPosition);

                bool isActive = IsActiveProperty(viewPosition);
                if(isActive)
                    GUI.backgroundColor = _activeButtonColor;
                if (GUILayout.Button(isActive ? "Return" : "Edit", EditorStyles.miniButton, GUILayout.MaxWidth(PreviewButtonWidth)))
                {
                    if (isActive)
                        SetActiveProperty(null);
                    else
                        SetActiveProperty(viewPosition);
                    _repaint = true;
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
            Separator();
        }

    }


}
#endif
