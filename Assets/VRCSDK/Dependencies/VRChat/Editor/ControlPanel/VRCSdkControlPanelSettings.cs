using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Core;
using VRC.SDKBase.Editor;

public partial class VRCSdkControlPanel : EditorWindow
{
    bool UseDevApi
    {
        get
        {
            return VRC.Core.API.GetApiUrl() == VRC.Core.API.devApiUrl;
        }
    }

    string clientVersionDate;
    string sdkVersionDate;
    Vector2 settingsScroll;

    private void Awake()
    {
        GetClientSdkVersionInformation();
    }

    public void GetClientSdkVersionInformation()
    {
        clientVersionDate = VRC.Core.SDKClientUtilities.GetTestClientVersionDate();
        sdkVersionDate = VRC.Core.SDKClientUtilities.GetSDKVersionDate();
    }

    public void OnConfigurationChanged()
    {
        GetClientSdkVersionInformation();
    }

    void ShowSettings()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.Width(SdkWindowWidth));

        EditorGUILayout.BeginVertical(boxGuiStyle);
        EditorGUILayout.LabelField("Developer", EditorStyles.boldLabel);

        VRCSettings.DisplayAdvancedSettings = EditorGUILayout.ToggleLeft("Show Extra Options on build page and account page", VRCSettings.DisplayAdvancedSettings);
        bool prevDisplayHelpBoxes = VRCSettings.DisplayHelpBoxes;
        VRCSettings.DisplayHelpBoxes = EditorGUILayout.ToggleLeft("Show Help Boxes on SDK components", VRCSettings.DisplayHelpBoxes);
        if (VRCSettings.DisplayHelpBoxes != prevDisplayHelpBoxes)
        {
            Editor[] editors = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < editors.Length; i++)
            {
                editors[i].Repaint();
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        ShowSdk23CompatibilitySettings();
        EditorGUILayout.Separator();

        ShowSettingsOptionsForBuilders();
        

        // debugging
        if (APIUser.CurrentUser != null && APIUser.CurrentUser.hasSuperPowers)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(boxGuiStyle);

            EditorGUILayout.LabelField("Logging", EditorStyles.boldLabel);

            // API logging
            {
                bool isLoggingEnabled = UnityEditor.EditorPrefs.GetBool("apiLoggingEnabled");
                bool enableLogging = EditorGUILayout.ToggleLeft("API Logging Enabled", isLoggingEnabled);
                if (enableLogging != isLoggingEnabled)
                {
                    if (enableLogging)
                        VRC.Core.Logger.AddDebugLevel(DebugLevel.API);
                    else
                        VRC.Core.Logger.RemoveDebugLevel(DebugLevel.API);

                    UnityEditor.EditorPrefs.SetBool("apiLoggingEnabled", enableLogging);
                }
            }

            // All logging
            {
                bool isLoggingEnabled = UnityEditor.EditorPrefs.GetBool("allLoggingEnabled");
                bool enableLogging = EditorGUILayout.ToggleLeft("All Logging Enabled", isLoggingEnabled);
                if (enableLogging != isLoggingEnabled)
                {
                    if (enableLogging)
                        VRC.Core.Logger.AddDebugLevel(DebugLevel.All);
                    else
                        VRC.Core.Logger.RemoveDebugLevel(DebugLevel.All);

                    UnityEditor.EditorPrefs.SetBool("allLoggingEnabled", enableLogging);
                }
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            if (UnityEditor.EditorPrefs.GetBool("apiLoggingEnabled"))
                UnityEditor.EditorPrefs.SetBool("apiLoggingEnabled", false);
            if (UnityEditor.EditorPrefs.GetBool("allLoggingEnabled"))
                UnityEditor.EditorPrefs.SetBool("allLoggingEnabled", false);
        }

        // Future proof upload
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(boxGuiStyle);

            EditorGUILayout.LabelField("Publish", EditorStyles.boldLabel);
            bool futureProofPublish = UnityEditor.EditorPrefs.GetBool("futureProofPublish", DefaultFutureProofPublishEnabled);

            futureProofPublish = EditorGUILayout.ToggleLeft("Future Proof Publish", futureProofPublish);

            if (UnityEditor.EditorPrefs.GetBool("futureProofPublish", DefaultFutureProofPublishEnabled) != futureProofPublish)
            {
                UnityEditor.EditorPrefs.SetBool("futureProofPublish", futureProofPublish);
            }
            EditorGUILayout.LabelField("Client Version Date", clientVersionDate);
            EditorGUILayout.LabelField("SDK Version Date", sdkVersionDate);

            EditorGUILayout.EndVertical();
        }


        if (APIUser.CurrentUser != null)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(boxGuiStyle);

            // custom vrchat install location
            OnVRCInstallPathGUI();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    static void OnVRCInstallPathGUI()
    {
        EditorGUILayout.LabelField("VRChat Client", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Installed Client Path: ", clientInstallPath);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("");
        if (GUILayout.Button("Edit"))
        {
            string initPath = "";
            if (!string.IsNullOrEmpty(clientInstallPath))
                initPath = clientInstallPath;

            clientInstallPath = EditorUtility.OpenFilePanel("Choose VRC Client Exe", initPath, "exe");
            SDKClientUtilities.SetVRCInstallPath(clientInstallPath);
            window.OnConfigurationChanged();
        }
        if (GUILayout.Button("Revert to Default"))
        {
            clientInstallPath = SDKClientUtilities.LoadRegistryVRCInstallPath();
            window.OnConfigurationChanged();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
    }

    static void ShowSdk23CompatibilitySettings()
    {
        return;

//        EditorGUILayout.BeginVertical(boxGuiStyle);
//        EditorGUILayout.LabelField("VRCSDK2 & VRCSDK3 Compatibility", EditorStyles.boldLabel);

//#if !VRC_CLIENT
//        bool sdk2Present = VRCSdk3Analysis.GetSDKInScene(VRCSdk3Analysis.SdkVersion.VRCSDK2).Count > 0;
//        bool sdk3Present = VRCSdk3Analysis.GetSDKInScene(VRCSdk3Analysis.SdkVersion.VRCSDK3).Count > 0;
//        bool sdk2DllActive = VRCSdk3Analysis.IsSdkDllActive(VRCSdk3Analysis.SdkVersion.VRCSDK2);
//        bool sdk3DllActive = VRCSdk3Analysis.IsSdkDllActive(VRCSdk3Analysis.SdkVersion.VRCSDK3);

//        if ( sdk2DllActive && sdk3DllActive)
//        {
//            GUILayout.TextArea("You have not yet configured this project for development with VRCSDK2 and Triggers or VRCSDK3 and Udon. ");
//            if (sdk2Present && sdk3Present)
//            {
//                GUILayout.TextArea("This scene contains both SDK2 and SDK3 elements. " +
//                    "Please modify this scene to contain only one type or the other before completing your configuration.");
//            }
//            else if (sdk2Present)
//            {
//                GUILayout.TextArea("This scene contains SDK2 scripts. " +
//                    "Check below to configure this project for use with VRCSDK2 or remove your VRCSDK2 scripts to upgrade to VRCSDK3");
//                bool downgrade = EditorGUILayout.ToggleLeft("Configure for use with VRCSDK2 and Triggers", false);
//                if (downgrade)
//                    VRCSdk3Analysis.SetSdkVersionActive(VRCSdk3Analysis.SdkVersion.VRCSDK2);
//            }
//            else if (sdk3Present)
//            {
//                GUILayout.TextArea("This scene contains only SDK3 scripts and it ready to upgrade. " +
//                    "Click below to get started.");
//                bool upgrade = EditorGUILayout.ToggleLeft("Configure for use with VRCSDK3 and Udon - Let's Rock!", false);
//                if (upgrade)
//                    VRCSdk3Analysis.SetSdkVersionActive(VRCSdk3Analysis.SdkVersion.VRCSDK3);
//            }
//            else
//            {
//                GUILayout.TextArea("This scene is a blank slate. " +
//                    "Click below to get started.");
//                bool upgrade = EditorGUILayout.ToggleLeft("Configure for use with VRCSDK3 and Udon - Let's Rock!", false);
//                if (upgrade)
//                    VRCSdk3Analysis.SetSdkVersionActive(VRCSdk3Analysis.SdkVersion.VRCSDK3);
//            }
//        }
//        else if (sdk2DllActive)
//        {
//            GUILayout.TextArea("This project has been configured to be built with VRCSDK2. " +
//                "To upgrade, VRCSDK3 must be enabled here.");
//            bool upgrade = EditorGUILayout.ToggleLeft("VRCSDK3 Scripts can be used", false);
//            if (upgrade)
//                VRCSdk3Analysis.SetSdkVersionActive(VRCSdk3Analysis.SdkVersion.VRCSDK3);
//        }
//        else if (sdk3DllActive)
//        {
//            GUILayout.TextArea("This project has been configured to be built with VRCSDK3. " +
//                "Congratulations, you're ready to go. " +
//                "You can still downgrade by activating VRCSDK2 here.");
//            bool downgrade = EditorGUILayout.ToggleLeft("VRCSDK2 Scripts can be used", false);
//            if (downgrade)
//                VRCSdk3Analysis.SetSdkVersionActive(VRCSdk3Analysis.SdkVersion.VRCSDK2);
//        }
//        else
//        {
//            GUILayout.TextArea("Somehow you have disabled both VRCSDK2 and VRCSDK3. Oops. " +
//                "Click here to begin development with VRCSDK3.");
//            bool begin = EditorGUILayout.ToggleLeft("VRCSDK3 Scripts can be used", false);
//            if (begin)
//                VRCSdk3Analysis.SetSdkVersionActive(VRCSdk3Analysis.SdkVersion.VRCSDK3);
//        }
//#else
//        GUILayout.TextArea("I think you're in the main VRChat project. " +
//            "You should not be enabling or disabling SDKs from here.");
//#endif

//        EditorGUILayout.EndVertical();
    }
}
