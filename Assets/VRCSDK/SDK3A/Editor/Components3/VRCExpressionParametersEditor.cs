using UnityEngine;
using UnityEditor;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using UnityEngine.UI;

[CustomEditor(typeof(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters))]
public class VRCExpressionParametersEditor : Editor
{
	int selected = -1;
	GUIStyle boxNormal;
	GUIStyle boxSelected;

	void InitStyles()
	{
		//Normal
		if(boxNormal == null)
			boxNormal = new GUIStyle(GUI.skin.box);

		//Selected
		if(boxSelected == null)
		{
			boxSelected = new GUIStyle(GUI.skin.box);
			boxSelected.normal.background = MakeStyleBackground(new Color(0.0f, 0.5f, 1f, 0.5f));
		}
	}
	Texture2D MakeStyleBackground(Color color)
	{
		var texture = new Texture2D(1, 1);
		texture.SetPixel(0, 0, color);
		texture.Apply();
		return texture;
	}

	void SelectParam(int value)
	{
		selected = value;
		Repaint();
	}
	public void OnEnable()
	{
		//Init parameters
		var expressionParameters = target as ExpressionParameters;
		if (expressionParameters.parameters == null)
			InitExpressionParameters(true);

		SelectParam(-1);
	}
	public override void OnInspectorGUI()
	{
		InitStyles();

		serializedObject.Update();
		{
			EditorGUILayout.LabelField("Parameters");
			var parameters = serializedObject.FindProperty("parameters");

			//Controls
			EditorGUILayout.BeginHorizontal();
			{
				//Add
				if (GUILayout.Button("Add"))
					parameters.arraySize = parameters.arraySize + 1;

				EditorGUI.BeginDisabledGroup(selected < 0);
				{
					//Move Up
					if (GUILayout.Button("Up"))
					{
						if(selected > 0)
						{
							SwapParams(selected, selected - 1);
							selected = selected - 1;
							Repaint();
						}
					}

					//Move Down
					if (GUILayout.Button("Down"))
					{
						if (selected < parameters.arraySize-1)
						{
							SwapParams(selected, selected + 1);
							selected = selected + 1;
							Repaint();
						}
					}

					void SwapParams(int indexA, int indexB)
					{
						var script = (ExpressionParameters)target;
						var itemA = script.parameters[indexA];
						var itemB = script.parameters[indexB];
						script.parameters[indexA] = itemB;
						script.parameters[indexB] = itemA;

						serializedObject.Update();
					}

					//Delete
					if (GUILayout.Button("Delete"))
					{
						parameters.DeleteArrayElementAtIndex(selected);
						SelectParam(-1);
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
			

			//Labels
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("    Name", GUILayout.MinWidth(100));
				EditorGUILayout.LabelField("    Type", GUILayout.Width(100));
				EditorGUILayout.LabelField("Default", GUILayout.Width(64));
				EditorGUILayout.LabelField("Saved", GUILayout.Width(64));
			}
			EditorGUILayout.EndHorizontal();

			//Parameters
			int count = parameters.arraySize;
			for(int paramIter=0; paramIter< parameters.arraySize; paramIter++)
			{
				DrawExpressionParameter(parameters, paramIter);
					
				/*var item = parameters.GetArrayElementAtIndex(paramIter);
				var name = item.FindPropertyRelative("name");
				var valueType = item.FindPropertyRelative("valueType");

				//Draw
				EditorGUI.indentLevel += 1;
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.PropertyField(name, new GUIContent(""));
					EditorGUILayout.PropertyField(valueType, new GUIContent(""));
					if(GUILayout.Button("X", GUILayout.Width(32)))
					{
						parameters.DeleteArrayElementAtIndex(paramIter);
						paramIter -= 1;
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel -= 1;*/
			}

			//Cost
			int cost = (target as ExpressionParameters).CalcTotalCost();
			if(cost <= ExpressionParameters.MAX_PARAMETER_COST)
				EditorGUILayout.HelpBox($"Total Memory: {cost}/{ExpressionParameters.MAX_PARAMETER_COST}", MessageType.Info);
			else
				EditorGUILayout.HelpBox($"Total Memory: {cost}/{ExpressionParameters.MAX_PARAMETER_COST}\nParameters use too much memory.  Remove parameters or use bools which use less memory.", MessageType.Error);

			//Info
			EditorGUILayout.HelpBox("Only parameters defined here can be used by expression menus, sync between all playable layers and sync across the network to remote clients.", MessageType.Info);
			EditorGUILayout.HelpBox("The parameter name and type should match a parameter defined on one or more of your animation controllers.", MessageType.Info);
			EditorGUILayout.HelpBox("Parameters used by the default animation controllers (Optional)\nVRCEmote, Int\nVRCFaceBlendH, Float\nVRCFaceBlendV, Float", MessageType.Info);

			//Clear
			if (GUILayout.Button("Clear Parameters"))
			{
				if (EditorUtility.DisplayDialogComplex("Warning", "Are you sure you want to clear all expression parameters?", "Clear", "Cancel", "") == 0)
				{
					InitExpressionParameters(false);
				}
			}
			if (GUILayout.Button("Default Parameters"))
			{
				if (EditorUtility.DisplayDialogComplex("Warning", "Are you sure you want to reset all expression parameters to default?", "Reset", "Cancel", "") == 0)
				{
					InitExpressionParameters(true);
				}
			}
		}
		serializedObject.ApplyModifiedProperties();
	}
	void DrawExpressionParameter(SerializedProperty parameters, int index)
	{
		if (parameters.arraySize < index + 1)
			parameters.InsertArrayElementAtIndex(index);
		var item = parameters.GetArrayElementAtIndex(index);

		var name = item.FindPropertyRelative("name");
		var valueType = item.FindPropertyRelative("valueType");
		var defaultValue = item.FindPropertyRelative("defaultValue");
		var saved = item.FindPropertyRelative("saved");

		bool isSelected = selected == index;

		EditorGUI.indentLevel += 1;
		var rect = EditorGUILayout.BeginHorizontal(isSelected ? boxSelected : boxNormal);
		{
			EditorGUILayout.PropertyField(name, new GUIContent(""), GUILayout.MinWidth(100));
			EditorGUILayout.PropertyField(valueType, new GUIContent(""), GUILayout.Width(100));
			var type = (ExpressionParameters.ValueType)valueType.intValue;
			switch(type)
			{
				case ExpressionParameters.ValueType.Int:
					defaultValue.floatValue = Mathf.Clamp(EditorGUILayout.IntField((int)defaultValue.floatValue, GUILayout.Width(64)), 0, 255);
					break;
				case ExpressionParameters.ValueType.Float:
					defaultValue.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(defaultValue.floatValue, GUILayout.Width(64)), -1f, 1f);
					break;
				case ExpressionParameters.ValueType.Bool:
					defaultValue.floatValue = EditorGUILayout.Toggle(defaultValue.floatValue != 0 ? true : false, GUILayout.Width(64)) ? 1f : 0f;
					break;
			}
			EditorGUILayout.PropertyField(saved, new GUIContent(""), GUILayout.Width(64));
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel -= 1;

		//Select
		if(Event.current.type == EventType.MouseDown)
		{
			if(rect.Contains(Event.current.mousePosition))
			{
				SelectParam(index);
				Event.current.Use();
			}
		}
	}
	void InitExpressionParameters(bool populateWithDefault)
	{
		var expressionParameters = target as ExpressionParameters;
		serializedObject.Update();
		{
			if (populateWithDefault)
			{
				expressionParameters.parameters = new ExpressionParameter[3];

				expressionParameters.parameters[0] = new ExpressionParameter();
				expressionParameters.parameters[0].name = "VRCEmote";
				expressionParameters.parameters[0].valueType = ExpressionParameters.ValueType.Int;

				expressionParameters.parameters[1] = new ExpressionParameter();
				expressionParameters.parameters[1].name = "VRCFaceBlendH";
				expressionParameters.parameters[1].valueType = ExpressionParameters.ValueType.Float;

				expressionParameters.parameters[2] = new ExpressionParameter();
				expressionParameters.parameters[2].name = "VRCFaceBlendV";
				expressionParameters.parameters[2].valueType = ExpressionParameters.ValueType.Float;
			}
			else
			{
				//Empty
				expressionParameters.parameters = new ExpressionParameter[0];
			}
		}
		serializedObject.ApplyModifiedProperties();
	}
}