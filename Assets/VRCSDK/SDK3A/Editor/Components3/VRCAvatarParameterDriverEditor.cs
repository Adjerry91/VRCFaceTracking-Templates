#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using static VRC.SDKBase.VRC_AvatarParameterDriver;
using Boo.Lang;
using System;

[CustomEditor(typeof(VRCAvatarParameterDriver))]
public class AvatarParameterDriverEditor : Editor
{
	VRCAvatarParameterDriver driver;
	string[] parameterNames;
	AnimatorControllerParameterType[] parameterTypes;

	enum ChangeTypeBool
	{
		Set = 0,
		Random = 2,
	}

	public void OnEnable()
	{
		var driver = target as VRCAvatarParameterDriver;

		//Build parameter names
		var controller = GetCurrentController();
		if (controller != null)
		{
			//Standard
			List<string> names = new List<string>();
			List<AnimatorControllerParameterType> types = new List<AnimatorControllerParameterType>();
			foreach (var item in controller.parameters)
			{
				names.Add(item.name);
				types.Add(item.type);
			}
			parameterNames = names.ToArray();
			parameterTypes = types.ToArray();
		}
	}

	static UnityEditor.Animations.AnimatorController GetCurrentController()
	{
		UnityEditor.Animations.AnimatorController controller = null;
		var toolType = Type.GetType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs");
		var tool = EditorWindow.GetWindow(toolType);
		var controllerProperty = toolType.GetProperty("animatorController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		if (controllerProperty != null)
		{
			controller = controllerProperty.GetValue(tool, null) as UnityEditor.Animations.AnimatorController;
		}
		else
			Debug.LogError("Unable to find animator window.", tool);
		return controller;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		serializedObject.Update();
		var driver = target as VRCAvatarParameterDriver;

		//Info
		EditorGUILayout.HelpBox("This behaviour modifies parameters on this and all other animation controllers referenced on the avatar descriptor.", MessageType.Info);
		EditorGUILayout.HelpBox("You should primarily be driving expression parameters as they are the only variables that sync across the network. Changes to any other parameter will not be synced across the network.", MessageType.Info);

		//Data
		driver.localOnly = EditorGUILayout.Toggle("Local Only", driver.localOnly);

		//Check for info message
		bool usesAddOrRandom = false;
		foreach(var param in driver.parameters)
		{
			if (param.type != ChangeType.Set)
				usesAddOrRandom = true;
		}
		if(usesAddOrRandom && !driver.localOnly)
			EditorGUILayout.HelpBox("Using Add & Random may not produce the same result when run on remote instance of the avatar.  When using these modes it's suggested you use a synced parameter and use the local only option.", MessageType.Warning);

		//Parameters
		for (int i = 0; i < driver.parameters.Count; i++)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				var param = driver.parameters[i];
				var index = IndexOf(parameterNames, param.name);

				//Name
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Name");
				if (parameterNames != null)
				{
					EditorGUI.BeginChangeCheck();
					index = EditorGUILayout.Popup(index, parameterNames);
					if (EditorGUI.EndChangeCheck() && index >= 0)
						param.name = parameterNames[index];
				}
				param.name = EditorGUILayout.TextField(param.name);
				EditorGUILayout.EndHorizontal();

				//Value
				if (index >= 0)
				{
					var type = parameterTypes[index];
					if (type == AnimatorControllerParameterType.Int)
					{
						//Type
						param.type = (VRCAvatarParameterDriver.ChangeType)EditorGUILayout.EnumPopup("Change Type", param.type);

						//Value
						if (param.type == ChangeType.Set)
							param.value = Mathf.Clamp(EditorGUILayout.IntField("Value", (int)param.value), 0, 255);
						else if (param.type == ChangeType.Add)
							param.value = Mathf.Clamp(EditorGUILayout.IntField("Value", (int)param.value), -255, 255);
						else if (param.type == ChangeType.Random)
						{
							param.valueMin = Mathf.Clamp(EditorGUILayout.IntField("Min Value", (int)param.valueMin), 0, 255);
							param.valueMax = Mathf.Clamp(EditorGUILayout.IntField("Max Value", (int)param.valueMax), param.valueMin, 255);
						}
					}
					else if (type == AnimatorControllerParameterType.Float)
					{
						//Type
						param.type = (VRCAvatarParameterDriver.ChangeType)EditorGUILayout.EnumPopup("Change Type", param.type);

						//Value
						if (param.type == ChangeType.Set || param.type == ChangeType.Add)
							param.value = Mathf.Clamp(EditorGUILayout.FloatField("Value", param.value), -1f, 1);
						else if (param.type == ChangeType.Random)
						{
							param.valueMin = Mathf.Clamp(EditorGUILayout.FloatField("Min Value", param.valueMin), -1f, 1);
							param.valueMax = Mathf.Clamp(EditorGUILayout.FloatField("Max Value", param.valueMax), param.valueMin, 1);
						}
					}
					else if (type == AnimatorControllerParameterType.Bool)
					{
						//Type
						param.type = (VRCAvatarParameterDriver.ChangeType)EditorGUILayout.EnumPopup("Change Type", (ChangeTypeBool)param.type);

						//Value
						if (param.type == ChangeType.Set)
							param.value = EditorGUILayout.Toggle("Value", param.value != 0) ? 1f : 0f;
						else
							param.chance = Mathf.Clamp(EditorGUILayout.FloatField("Chance", param.chance), 0f, 1f);
					}
					else if (type == AnimatorControllerParameterType.Trigger)
					{
						//Type
						param.type = (VRCAvatarParameterDriver.ChangeType)EditorGUILayout.EnumPopup("Change Type", (ChangeTypeBool)param.type);

						//Chance
						if (param.type == ChangeType.Random)
							param.chance = Mathf.Clamp(EditorGUILayout.FloatField("Chance", param.chance), 0f, 1f);
					}
				}
				else
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.EnumPopup(param.type);
					if(param.type == ChangeType.Random)
					{
						EditorGUILayout.FloatField("Min Value", param.valueMin);
						EditorGUILayout.FloatField("Max Value", param.valueMax);
					}
					else
					{
						EditorGUILayout.FloatField("Value", param.value);
					}
					EditorGUI.EndDisabledGroup();
					DrawInfoBox("WARNING: Parameter not found. Make sure you defined it on the animation controller.");
				}

				//Delete
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Delete"))
				{
					driver.parameters.RemoveAt(i);
					i--;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndHorizontal();
		}

		//Add
		if (GUILayout.Button("Add Parameter"))
		{
			var parameter = new Parameter();
			if (parameterNames != null && parameterNames.Length > 0)
				parameter.name = parameterNames[0];
			driver.parameters.Add(parameter);
		}

		int IndexOf(string[] array, string value)
		{
			if (array == null)
				return -1;
			for(int i=0; i<array.Length; i++)
			{
				if (array[i] == value)
					return i;
			}
			return -1;
		}

		void DrawInfoBox(string text)
		{
			EditorGUI.indentLevel += 2;
			EditorGUILayout.LabelField(text, EditorStyles.textArea);
			EditorGUI.indentLevel -= 2;
		}

		//End
		serializedObject.ApplyModifiedProperties();
		if (EditorGUI.EndChangeCheck())
			EditorUtility.SetDirty(this);
	}
}
#endif
