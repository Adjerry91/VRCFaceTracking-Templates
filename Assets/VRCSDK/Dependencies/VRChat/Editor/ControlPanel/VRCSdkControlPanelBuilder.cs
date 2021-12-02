using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRC.SDKBase.Validation.Performance;
using Object = UnityEngine.Object;
using VRC.SDKBase.Editor;

public partial class VRCSdkControlPanel : EditorWindow
{
    public static System.Action _EnableSpatialization = null;   // assigned in AutoAddONSPAudioSourceComponents

    public const string AVATAR_OPTIMIZATION_TIPS_URL = "https://docs.vrchat.com/docs/avatar-optimizing-tips";
    public const string AVATAR_RIG_REQUIREMENTS_URL = "https://docs.vrchat.com/docs/rig-requirements";

    const string kCantPublishContent = "Before you can upload avatars or worlds, you will need to spend some time in VRChat.";
    const string kCantPublishAvatars = "Before you can upload avatars, you will need to spend some time in VRChat.";
    const string kCantPublishWorlds = "Before you can upload worlds, you will need to spend some time in VRChat.";
    private const string FIX_ISSUES_TO_BUILD_OR_TEST_WARNING_STRING = "You must address the above issues before you can build or test this content!";
    
    static Texture _perfIcon_Excellent;
    static Texture _perfIcon_Good;
    static Texture _perfIcon_Medium;
    static Texture _perfIcon_Poor;
    static Texture _perfIcon_VeryPoor;
    static Texture _bannerImage;

    public void ResetIssues()
    {
        GUIErrors.Clear();
        GUIInfos.Clear();
        GUIWarnings.Clear();
        GUILinks.Clear();
        GUIStats.Clear();
        CheckedForIssues = false;
    }

    public bool CheckedForIssues { get; set; } = false;

    class Issue
    {
        public string issueText;
        public System.Action showThisIssue;
        public System.Action fixThisIssue;
        public PerformanceRating performanceRating;

        public Issue(string text, System.Action show, System.Action fix, PerformanceRating rating = PerformanceRating.None)
        {
            issueText = text;
            showThisIssue = show;
            fixThisIssue = fix;
            performanceRating = rating;
        }

        public class Equality : IEqualityComparer<Issue>, IComparer<Issue>
        {
            public bool Equals(Issue b1, Issue b2)
            {
                return (b1.issueText == b2.issueText);
            }
            public int Compare(Issue b1, Issue b2)
            {
                return string.Compare(b1.issueText, b2.issueText);
            }
            public int GetHashCode(Issue bx)
            {
                return bx.issueText.GetHashCode();
            }
        }
    }

    Dictionary<Object, List<Issue>> GUIErrors = new Dictionary<Object, List<Issue>>();
    Dictionary<Object, List<Issue>> GUIWarnings = new Dictionary<Object, List<Issue>>();
    Dictionary<Object, List<Issue>> GUIInfos = new Dictionary<Object, List<Issue>>();
    Dictionary<Object, List<Issue>> GUILinks = new Dictionary<Object, List<Issue>>();
    Dictionary<Object, List<Issue>> GUIStats = new Dictionary<Object, List<Issue>>();

    public bool NoGuiErrors()
    {
        return GUIErrors.Count == 0;
    }

    public bool NoGuiErrorsOrIssues()
    {
        return GUIErrors.Count == 0 && CheckedForIssues;
    }
    
    void AddToReport(Dictionary<Object, List<Issue>> report, Object subject, string output, System.Action show, System.Action fix)
    {
        if (subject == null)
            subject = this;
        if (!report.ContainsKey(subject))
            report.Add(subject, new List<Issue>());

        var issue = new Issue(output, show, fix);
        if (!report[subject].Contains(issue, new Issue.Equality()))
        {
            report[subject].Add(issue);
            report[subject].Sort(new Issue.Equality());
        }
    }

    void BuilderAssemblyReload()
    {
        ResetIssues();
    }

    public void OnGUIError(Object subject, string output, System.Action show, System.Action fix)
    {
        AddToReport(GUIErrors, subject, output, show, fix);
    }

    public void OnGUIWarning(Object subject, string output, System.Action show, System.Action fix)
    {
        AddToReport(GUIWarnings, subject, output, show, fix);
    }

    public void OnGUIInformation(Object subject, string output)
    {
        AddToReport(GUIInfos, subject, output, null, null);
    }

    public void OnGUILink(Object subject, string output, string link)
    {
        AddToReport(GUILinks, subject, output + "\n" + link, null, null);
    }

    public void OnGUIStat(Object subject, string output, PerformanceRating rating, System.Action show, System.Action fix)
    {
        if (subject == null)
            subject = this;
        if (!GUIStats.ContainsKey(subject))
            GUIStats.Add(subject, new List<Issue>());
        GUIStats[subject].Add(new Issue(output, show, fix, rating));
    }

    public int triggerLineMode
    {
        get { return EditorPrefs.GetInt("VRC.SDKBase_triggerLineMode", 0); }
        set { EditorPrefs.SetInt("VRC.SDKBase_triggerLineMode", value); }
    }

    private void ShowSettingsOptionsForBuilders()
    {
        if (_sdkBuilders == null)
        {
            PopulateSdkBuilders();
        }
        for (int i = 0; i < _sdkBuilders.Length; i++)
        {
            IVRCSdkControlPanelBuilder builder = _sdkBuilders[i];
            builder.ShowSettingsOptions();
            if (i < _sdkBuilders.Length - 1)
            {
                EditorGUILayout.Separator();
            }
        }
    }
    
    private IVRCSdkControlPanelBuilder[] _sdkBuilders;
    
    private static List<Type> GetSdkBuilderTypesFromAttribute()
    {
        Type sdkBuilderInterfaceType = typeof(IVRCSdkControlPanelBuilder);
        Type sdkBuilderAttributeType = typeof(VRCSdkControlPanelBuilderAttribute);

        List<Type> moduleTypesFromAttribute = new List<Type>();
        foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            VRCSdkControlPanelBuilderAttribute[] sdkBuilderAttributes;
            try
            {
                sdkBuilderAttributes = (VRCSdkControlPanelBuilderAttribute[])assembly.GetCustomAttributes(sdkBuilderAttributeType, true);
            }
            catch
            {
                sdkBuilderAttributes = new VRCSdkControlPanelBuilderAttribute[0];
            }

            foreach(VRCSdkControlPanelBuilderAttribute udonWrapperModuleAttribute in sdkBuilderAttributes)
            {
                if(udonWrapperModuleAttribute == null)
                {
                    continue;
                }

                if(!sdkBuilderInterfaceType.IsAssignableFrom(udonWrapperModuleAttribute.Type))
                {
                    continue;
                }

                moduleTypesFromAttribute.Add(udonWrapperModuleAttribute.Type);
            }
        }

        return moduleTypesFromAttribute;
    }

    private void PopulateSdkBuilders()
    {
        if (_sdkBuilders != null)
        {
            return;
        }
        List<IVRCSdkControlPanelBuilder> builders = new List<IVRCSdkControlPanelBuilder>();
        foreach (Type type in GetSdkBuilderTypesFromAttribute())
        {
            IVRCSdkControlPanelBuilder builder = (IVRCSdkControlPanelBuilder)Activator.CreateInstance(type);
            builder.RegisterBuilder(this);
            builders.Add(builder);
        }
        _sdkBuilders = builders.ToArray();
    }
    
    void ShowBuilders()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        
        if (VRC.Core.ConfigManager.RemoteConfig.IsInitialized())
        {
            string sdkUnityVersion = VRC.Core.ConfigManager.RemoteConfig.GetString("sdkUnityVersion");
            if (Application.unityVersion != sdkUnityVersion)
            {
                OnGUIWarning(null, "You are not using the recommended Unity version for the VRChat SDK. Content built with this version may not work correctly. Please use Unity " + sdkUnityVersion,
                    null,
                    () => { Application.OpenURL("https://unity3d.com/get-unity/download/archive"); }
                );
            }
        }
        
        if (VRCSdk3Analysis.IsSdkDllActive(VRCSdk3Analysis.SdkVersion.VRCSDK2) && VRCSdk3Analysis.IsSdkDllActive(VRCSdk3Analysis.SdkVersion.VRCSDK3))
        {
            List<Component> sdk2Components = VRCSdk3Analysis.GetSDKInScene(VRCSdk3Analysis.SdkVersion.VRCSDK2);
            List<Component> sdk3Components = VRCSdk3Analysis.GetSDKInScene(VRCSdk3Analysis.SdkVersion.VRCSDK3);
            if (sdk2Components.Count > 0 && sdk3Components.Count > 0)
            {
                OnGUIError(null,
                    "This scene contains components from the VRChat SDK version 2 and version 3. Version two elements will have to be replaced with their version 3 counterparts to build with SDK3 and UDON.",
                    () => { Selection.objects = sdk2Components.ToArray(); },
                    null
                );
            }
        }
        
        if (Lightmapping.giWorkflowMode == Lightmapping.GIWorkflowMode.Iterative)
        {
            OnGUIWarning(null,
                "Automatic lightmap generation is enabled, which may stall the Unity build process. Before building and uploading, consider turning off 'Auto Generate' at the bottom of the Lighting Window.",
                () =>
                {
                    EditorWindow lightingWindow = GetLightingWindow();
                    if (lightingWindow)
                    {
                        lightingWindow.Show();
                        lightingWindow.Focus();
                    }
                },
                () =>
                {
                    Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
                    EditorWindow lightingWindow = GetLightingWindow();
                    if (!lightingWindow) return;
                    lightingWindow.Repaint();
                    Focus();
                }
            );
        }
        
        PopulateSdkBuilders();
        IVRCSdkControlPanelBuilder selectedBuilder = null;
        string errorMessage = null;
        foreach (IVRCSdkControlPanelBuilder sdkBuilder in _sdkBuilders)
        {
            if (!sdkBuilder.IsValidBuilder(out string message))
            {
                if (selectedBuilder == null)
                {
                    errorMessage = message;
                }
            }
            else
            {
                if (selectedBuilder == null)
                {
                    selectedBuilder = sdkBuilder;
                    errorMessage = null;
                }
                else
                {
                     errorMessage =
                         "A Unity scene cannot contain a VRChat Scene Descriptor and also contain VRChat Avatar Descriptors";
                }
            }
        }
        if (selectedBuilder == null)
        {
#if VRC_SDK_VRCSDK2
            EditorGUILayout.LabelField("A VRC_SceneDescriptor or VRC_AvatarDescriptor\nis required to build VRChat SDK Content", titleGuiStyle, GUILayout.Width(SdkWindowWidth));
#elif VRC_SDK_VRCSDK3
            EditorGUILayout.LabelField("A VRCSceneDescriptor or VRCAvatarDescriptor\nis required to build VRChat SDK Content", titleGuiStyle, GUILayout.Width(SdkWindowWidth));
#else
            EditorGUILayout.LabelField("The SDK did not load properly. Try this - In the Project window, navigate to Assets/VRCSDK/Plugins. Select all the DLLs, then right click and choose 'Reimport'");
#endif
        }
        else if (errorMessage != null)
        {
            OnGUIError(null,
                errorMessage,
                () => {
                    foreach (IVRCSdkControlPanelBuilder builder in _sdkBuilders)
                    {
                        builder.SelectAllComponents();
                    } },
                null
            );    
            OnGUIShowIssues();
        }
        else
        {
            selectedBuilder.ShowBuilder();
        }

        if (Event.current.type == EventType.Used) return;
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public bool showLayerHelp = false;
    
    bool ShouldShowLightmapWarning
    {
        get
        {
            const string GraphicsSettingsAssetPath = "ProjectSettings/GraphicsSettings.asset";
            SerializedObject graphicsManager = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(GraphicsSettingsAssetPath)[0]);
            SerializedProperty lightmapStripping = graphicsManager.FindProperty("m_LightmapStripping");
            return lightmapStripping.enumValueIndex == 0;
        }
    }

    bool ShouldShowFogWarning
    {
        get
        {
            const string GraphicsSettingsAssetPath = "ProjectSettings/GraphicsSettings.asset";
            SerializedObject graphicsManager = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(GraphicsSettingsAssetPath)[0]);
            SerializedProperty lightmapStripping = graphicsManager.FindProperty("m_FogStripping");
            return lightmapStripping.enumValueIndex == 0;
        }
    }

    void DrawIssueBox(MessageType msgType, Texture icon, string message, System.Action show, System.Action fix)
    {
        bool haveButtons = ((show != null) || (fix != null));

        GUIStyle style = new GUIStyle("HelpBox");
        style.fixedWidth = (haveButtons ? (SdkWindowWidth - 90) : SdkWindowWidth);
        float minHeight = 40;

        try
        {
            EditorGUILayout.BeginHorizontal();
            if (icon != null)
            {
                GUIContent c = new GUIContent(message, icon);
                float height = style.CalcHeight(c, style.fixedWidth);
                GUILayout.Box(c, style, GUILayout.MinHeight(Mathf.Max(minHeight, height)));
            }
            else
            {
                GUIContent c = new GUIContent(message);
                float height = style.CalcHeight(c, style.fixedWidth);
                Rect rt = GUILayoutUtility.GetRect(c, style, GUILayout.MinHeight(Mathf.Max(minHeight, height)));
                EditorGUI.HelpBox(rt, message, msgType);    // note: EditorGUILayout resulted in uneven button layout in this case
            }

            if (haveButtons)
            {
                EditorGUILayout.BeginVertical();
                float buttonHeight = ((show == null || fix == null) ? minHeight : (minHeight * 0.5f));
                if ((show != null) && GUILayout.Button("Select", GUILayout.Height(buttonHeight)))
                    show();
                if ((fix != null) && GUILayout.Button("Auto Fix", GUILayout.Height(buttonHeight)))
                {
                    fix();
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    CheckedForIssues = false;
                    Repaint();
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }
        catch
        {
            // mutes 'ArgumentException: Getting control 0's position in a group with only 0 controls when doing repaint'
        }
    }

    public void OnGuiFixIssuesToBuildOrTest()
    {
        GUIStyle s = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.Space();
        GUILayout.BeginVertical(boxGuiStyle, GUILayout.Height(WARNING_ICON_SIZE), GUILayout.Width(SdkWindowWidth));
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        var textDimensions = s.CalcSize(new GUIContent(FIX_ISSUES_TO_BUILD_OR_TEST_WARNING_STRING));
        GUILayout.Label(new GUIContent(warningIconGraphic), GUILayout.Width(WARNING_ICON_SIZE), GUILayout.Height(WARNING_ICON_SIZE));
        EditorGUILayout.LabelField(FIX_ISSUES_TO_BUILD_OR_TEST_WARNING_STRING, s, GUILayout.Width(textDimensions.x), GUILayout.Height(WARNING_ICON_SIZE));
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }

    public void OnGUIShowIssues(Object subject = null)
    {
        if (subject == null)
            subject = this;

        EditorGUI.BeginChangeCheck();

        GUIStyle style = GUI.skin.GetStyle("HelpBox");

        if (GUIErrors.ContainsKey(subject))
            foreach (Issue error in GUIErrors[subject].Where(s => !string.IsNullOrEmpty(s.issueText)))
                DrawIssueBox(MessageType.Error, null, error.issueText, error.showThisIssue, error.fixThisIssue);
        if (GUIWarnings.ContainsKey(subject))
            foreach (Issue error in GUIWarnings[subject].Where(s => !string.IsNullOrEmpty(s.issueText)))
                DrawIssueBox(MessageType.Warning, null, error.issueText, error.showThisIssue, error.fixThisIssue);

        if (GUIStats.ContainsKey(subject))
        {
            foreach (var kvp in GUIStats[subject].Where(k => k.performanceRating == PerformanceRating.VeryPoor))
                DrawIssueBox(MessageType.Warning, GetPerformanceIconForRating(kvp.performanceRating), kvp.issueText, kvp.showThisIssue, kvp.fixThisIssue);

            foreach (var kvp in GUIStats[subject].Where(k => k.performanceRating == PerformanceRating.Poor))
                DrawIssueBox(MessageType.Warning, GetPerformanceIconForRating(kvp.performanceRating), kvp.issueText, kvp.showThisIssue, kvp.fixThisIssue);

            foreach (var kvp in GUIStats[subject].Where(k => k.performanceRating == PerformanceRating.Medium))
                DrawIssueBox(MessageType.Warning, GetPerformanceIconForRating(kvp.performanceRating), kvp.issueText, kvp.showThisIssue, kvp.fixThisIssue);

            foreach (var kvp in GUIStats[subject].Where(k => k.performanceRating == PerformanceRating.Good || k.performanceRating == PerformanceRating.Excellent))
                DrawIssueBox(MessageType.Warning, GetPerformanceIconForRating(kvp.performanceRating), kvp.issueText, kvp.showThisIssue, kvp.fixThisIssue);
        }

        if (GUIInfos.ContainsKey(subject))
            foreach (Issue error in GUIInfos[subject].Where(s => !string.IsNullOrEmpty(s.issueText)))
                EditorGUILayout.HelpBox(error.issueText, MessageType.Info);
        if (GUILinks.ContainsKey(subject))
        {
            EditorGUILayout.BeginVertical(style);
            foreach (Issue error in GUILinks[subject].Where(s => !string.IsNullOrEmpty(s.issueText)))
            {
                var s = error.issueText.Split('\n');
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(s[0]);
                if (GUILayout.Button("Open Link", GUILayout.Width(100)))
                    Application.OpenURL(s[1]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(subject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }

    private Texture GetPerformanceIconForRating(PerformanceRating value)
    {
        if (_perfIcon_Excellent == null)
            _perfIcon_Excellent = Resources.Load<Texture>("PerformanceIcons/Perf_Great_32");
        if (_perfIcon_Good == null)
            _perfIcon_Good = Resources.Load<Texture>("PerformanceIcons/Perf_Good_32");
        if (_perfIcon_Medium == null)
            _perfIcon_Medium = Resources.Load<Texture>("PerformanceIcons/Perf_Medium_32");
        if (_perfIcon_Poor == null)
            _perfIcon_Poor = Resources.Load<Texture>("PerformanceIcons/Perf_Poor_32");
        if (_perfIcon_VeryPoor == null)
            _perfIcon_VeryPoor = Resources.Load<Texture>("PerformanceIcons/Perf_Horrible_32");

        switch (value)
        {
            case PerformanceRating.Excellent:
                return _perfIcon_Excellent;
            case PerformanceRating.Good:
                return _perfIcon_Good;
            case PerformanceRating.Medium:
                return _perfIcon_Medium;
            case PerformanceRating.Poor:
                return _perfIcon_Poor;
            case PerformanceRating.None:
            case PerformanceRating.VeryPoor:
                return _perfIcon_VeryPoor;
        }

        return _perfIcon_Excellent;
    }

    Texture2D CreateBackgroundColorImage(UnityEngine.Color color)
    {
        int w = 4, h = 4;
        Texture2D back = new Texture2D(w, h);
        UnityEngine.Color[] buffer = new UnityEngine.Color[w * h];
        for (int i = 0; i < w; ++i)
            for (int j = 0; j < h; ++j)
                buffer[i + w * j] = color;
        back.SetPixels(buffer);
        back.Apply(false);
        return back;
    }

    public static void DrawContentInfo(string name, string version, string description, string capacity, string releaseStatus, List<string> tags)
    {
        EditorGUILayout.LabelField("Name: " + name);
        EditorGUILayout.LabelField("Version: " + version.ToString());
        EditorGUILayout.LabelField("Description: " + description);
        if (capacity != null)
            EditorGUILayout.LabelField("Capacity: " + capacity);
        EditorGUILayout.LabelField("Release: " + releaseStatus);
        if (tags != null)
        {
            string tagString = "";
            for (int i = 0; i < tags.Count; i++)
            {
                if (i != 0) tagString += ", ";
                tagString += tags[i];
            }
            EditorGUILayout.LabelField("Tags: " + tagString);

        }
    }
    public static void DrawContentPlatformSupport(VRC.Core.ApiModel m)
    {
        if (m.supportedPlatforms == VRC.Core.ApiModel.SupportedPlatforms.StandaloneWindows || m.supportedPlatforms == VRC.Core.ApiModel.SupportedPlatforms.All)
            EditorGUILayout.LabelField("Windows Support: YES");
        else
            EditorGUILayout.LabelField("Windows Support: NO");

        if (m.supportedPlatforms == VRC.Core.ApiModel.SupportedPlatforms.Android || m.supportedPlatforms == VRC.Core.ApiModel.SupportedPlatforms.All)
            EditorGUILayout.LabelField("Android Support: YES");
        else
            EditorGUILayout.LabelField("Android Support: NO");
    }

    public static void DrawBuildTargetSwitcher()
    {
        EditorGUILayout.LabelField("Active Build Target: " + EditorUserBuildSettings.activeBuildTarget);
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 && GUILayout.Button("Switch Build Target to Android"))
        {
            if (EditorUtility.DisplayDialog("Build Target Switcher", "Are you sure you want to switch your build target to Android? This could take a while.", "Confirm", "Cancel"))
            {
                EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
            }
        }
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && GUILayout.Button("Switch Build Target to Windows"))
        {
            if (EditorUtility.DisplayDialog("Build Target Switcher", "Are you sure you want to switch your build target to Windows? This could take a while.", "Confirm", "Cancel"))
            {
                EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }
    }

    public static string GetBuildAndPublishButtonString()
    {
        string buildButtonString = "Build & Publish for UNSUPPORTED";
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            buildButtonString = "Build & Publish for Windows";
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            buildButtonString = "Build & Publish for Android";

        return buildButtonString;
    }

    public static Object[] GetSubstanceObjects(GameObject obj = null, bool earlyOut = false)
    {
        // if 'obj' is null we check entire scene
        // if 'earlyOut' is true we only return 1st object (to detect if substances are present)

        List<Object> objects = new List<Object>();
        if (obj == null) return objects.Count < 1 ? null : objects.ToArray();
        Renderer[] renderers = obj ? obj.GetComponentsInChildren<Renderer>(true) : FindObjectsOfType<Renderer>();

        if (renderers == null || renderers.Length < 1)
            return null;
        foreach (Renderer r in renderers)
        {
            if (r.sharedMaterials.Length < 1)
                continue;
            foreach (Material m in r.sharedMaterials)
            {
                if (!m)
                    continue;
                string path = AssetDatabase.GetAssetPath(m);
                if (string.IsNullOrEmpty(path))
                    continue;
                if (path.EndsWith(".sbsar", true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    objects.Add(r.gameObject);
                    if (earlyOut)
                        return objects.ToArray();
                }
            }
        }

        return objects.Count < 1 ? null : objects.ToArray();
    }

    public static bool HasSubstances(GameObject obj = null)
    {
        return (GetSubstanceObjects(obj, true) != null);
    }

    EditorWindow GetLightingWindow()
    {
        var editorAsm = typeof(UnityEditor.Editor).Assembly;
        return EditorWindow.GetWindow(editorAsm.GetType("UnityEditor.LightingWindow"));
    }

    public static void ShowContentPublishPermissionsDialog()
    {
        if (!VRC.Core.ConfigManager.RemoteConfig.IsInitialized())
        {
            VRC.Core.ConfigManager.RemoteConfig.Init(() => ShowContentPublishPermissionsDialog());
            return;
        }

        string message = VRC.Core.ConfigManager.RemoteConfig.GetString("sdkNotAllowedToPublishMessage");
        int result = UnityEditor.EditorUtility.DisplayDialogComplex("VRChat SDK", message, "Developer FAQ", "VRChat Discord", "OK");
        if (result == 0)
        {
            ShowDeveloperFAQ();
        }
        if (result == 1)
        {
            ShowVRChatDiscord();
        }
    }
}
