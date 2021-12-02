#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

public partial class AvatarDescriptorEditor3 : Editor
{
	static string _ExpressionsFoldoutPrefsKey = "VRCSDK3_AvatarDescriptorEditor3_ExpressionsFoldout";
	void Init_Expressions()
	{
	}
	void DrawInspector_Expressions()
	{
		var menu = serializedObject.FindProperty("expressionsMenu");
		var parameters = serializedObject.FindProperty("expressionParameters");
		var customize = serializedObject.FindProperty("customExpressions");

		if (Foldout(_ExpressionsFoldoutPrefsKey, "Expressions", false))
		{
			if(customize.boolValue)
			{
				if(GUILayout.Button("Reset To Default"))
				{
					if (EditorUtility.DisplayDialog("Reset to Default", "This will erase any custom expression settings. Are you sure?", "OK", "Cancel"))
					{
						menu.objectReferenceValue = null;
						parameters.objectReferenceValue = null;
						customize.boolValue = false;
					}
				}

				//Menu
				EditorGUILayout.PropertyField(menu, new GUIContent("Menu"));

				//Parameters
				EditorGUILayout.PropertyField(parameters, new GUIContent("Parameters"));
			}
			else
			{
				if (GUILayout.Button("Customize"))
				{
					customize.boolValue = true;
				}
			}
		}
		Separator();
	}
}
#endif
