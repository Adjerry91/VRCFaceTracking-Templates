#if VRC_SDK_VRCSDK2

using UnityEngine;
using UnityEditor;

public class VRCPlayerModEditorWindow : EditorWindow {

	public delegate void AddModCallback();
	public static AddModCallback addModCallback;

	private static VRCSDK2.VRC_PlayerMods myTarget;

	private static VRCSDK2.VRCPlayerModFactory.PlayerModType type;

	public static void Init (VRCSDK2.VRC_PlayerMods target, AddModCallback callback) 
	{
		// Get existing open window or if none, make a new one:
		EditorWindow.GetWindow (typeof (VRCPlayerModEditorWindow));
		addModCallback = callback;
		myTarget = target;

		type = VRCSDK2.VRCPlayerModFactory.PlayerModType.Jump;
	}
	
	void OnGUI ()
	{
		type = (VRCSDK2.VRCPlayerModFactory.PlayerModType)EditorGUILayout.EnumPopup("Mods", type);
		if(GUILayout.Button("Add Mod"))
		{
			VRCSDK2.VRCPlayerMod mod = VRCSDK2.VRCPlayerModFactory.Create(type);
			myTarget.AddMod(mod);
			addModCallback();
		}
	}
}

#endif