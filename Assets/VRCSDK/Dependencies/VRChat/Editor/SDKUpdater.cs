using UnityEngine;
using System.Collections;
using UnityEditor;

public class SDKUpdater : MonoBehaviour
{
	static string GetCurrentVersion()
	{
		string currentVersion = "";
		string versionTextPath = Application.dataPath + "/VRCSDK/version.txt";
		if(System.IO.File.Exists(versionTextPath))
		{
			string[] versionFileLines = System.IO.File.ReadAllLines(versionTextPath);
			if(versionFileLines.Length > 0)
				currentVersion = versionFileLines[0];	
		}
		return currentVersion;
	}

	[MenuItem("VRChat SDK/Utilities/Check For Updates")]
	static void CheckForUpdatesWithProgressBar()
	{
		CheckForUpdates(false);
	}

	public static void CheckForUpdates(bool isSilent = true)
	{
		Debug.Log("Checking for VRChat SDK updates...");
		if(!isSilent)
			EditorUtility.DisplayProgressBar("SDK Updater", "Checking for updates...", 1f);
		
		VRC.Core.ConfigManager.RemoteConfig.Init(delegate() {
			string currentSdkVersion = GetCurrentVersion();
			string sdkVersion = VRC.Core.ConfigManager.RemoteConfig.GetString("devSdkVersion");
			string sdkUrl = VRC.Core.ConfigManager.RemoteConfig.GetString("devSdkUrl");
			EditorUtility.ClearProgressBar();

			if(sdkVersion == currentSdkVersion)
			{
				ShowDownloadUpdatePopup(false, currentSdkVersion, sdkUrl, isSilent);
			}
			else
			{
				ShowDownloadUpdatePopup(true, sdkVersion, sdkUrl, isSilent);
			}
		});
	}

	static void ShowDownloadUpdatePopup(bool updateAvailable, string latestVersion, string sdkUrl, bool isSilent)
	{
		if(!updateAvailable)
		{
			if(!isSilent)
				EditorUtility.DisplayDialog("VRChat SDK Updater", "SDK is up to date (version " + latestVersion + ")", "Okay");
		}
		else
		{
			if(EditorUtility.DisplayDialog("VRChat SDK Updater", "An update is available (version " + latestVersion + ")", "Download", "Cancel"))
			{
				DownloadUpdate(sdkUrl);
			}
		}
	}

	static void DownloadUpdate(string sdkUrl)
	{
		Application.OpenURL(sdkUrl);
	}

}
