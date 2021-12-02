#define COMMUNITY_LABS_SDK

using UnityEngine;
using UnityEditor;

namespace VRCSDK2
{
    [InitializeOnLoad]
    public class VRC_SdkSplashScreen : EditorWindow
    {

        static VRC_SdkSplashScreen()
        {
            EditorApplication.update -= DoSplashScreen;
            EditorApplication.update += DoSplashScreen;
        }

        private static void DoSplashScreen()
        {
            EditorApplication.update -= DoSplashScreen;
            if (EditorApplication.isPlaying)
                return;

            #if UDON
                if (!EditorPrefs.GetBool("VRCSDK_ShowedSplashScreenFirstTime", false))
                {
                    OpenSplashScreen();
                    EditorPrefs.SetBool("VRCSDK_ShowedSplashScreenFirstTime", true);
                }
                else
            #endif
                if (EditorPrefs.GetBool("VRCSDK_ShowSplashScreen", true))
                    OpenSplashScreen();
        }

        private static GUIStyle vrcSdkHeader;
        private static GUIStyle vrcSdkBottomHeader;
        private static GUIStyle vrcHeaderLearnMoreButton;
        private static GUIStyle vrcBottomHeaderLearnMoreButton;
        private static Vector2 changeLogScroll;
        [MenuItem("VRChat SDK/Splash Screen", false, 500)]
        public static void OpenSplashScreen()
        {
            GetWindow<VRC_SdkSplashScreen>(true);
        }
        
        public static void Open()
        {
            OpenSplashScreen();
        }

        public void OnEnable()
        {
            titleContent = new GUIContent("VRChat SDK");

#if UDON
            maxSize = new Vector2(400, 360);
#else
            maxSize = new Vector2(400, 600);
#endif
            minSize = maxSize;

            vrcSdkHeader = new GUIStyle
            {
                normal =
                    {
#if UDON
                            background = Resources.Load("vrcSdkSplashUdon1") as Texture2D,
#elif COMMUNITY_LABS_SDK
                            background = Resources.Load("vrcSdkHeaderWithCommunityLabs") as Texture2D,
#else
                            background = Resources.Load("vrcSdkHeader") as Texture2D,
#endif
                        textColor = Color.white
                    },
                fixedHeight = 200
            };

            vrcSdkBottomHeader = new GUIStyle
            {
                normal =
                {
#if UDON
                        background = Resources.Load("vrcSdkSplashUdon2") as Texture2D,
#else
                        background = Resources.Load("vrcSdkBottomHeader") as Texture2D,
#endif

                    textColor = Color.white
                },
                fixedHeight = 100
            };

        }

        public void OnGUI()
        {
            GUILayout.Box("", vrcSdkHeader);

                vrcHeaderLearnMoreButton = EditorStyles.miniButton;
                vrcHeaderLearnMoreButton.normal.textColor = Color.black;
                vrcHeaderLearnMoreButton.fontSize = 12;
                vrcHeaderLearnMoreButton.border = new RectOffset(10, 10, 10, 10);
                Texture2D texture = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Texture2D>("UI/Skin/UISprite.psd");
                vrcHeaderLearnMoreButton.normal.background = texture;
                vrcHeaderLearnMoreButton.active.background = texture;
#if UDON
            if (GUI.Button(new Rect(20, 165, 185, 25), "Get Started with Udon", vrcHeaderLearnMoreButton))
                    Application.OpenURL("https://docs.vrchat.com/docs/getting-started-with-udon");
#elif COMMUNITY_LABS_SDK
            if (GUI.Button(new Rect(20, 140, 180, 40), "Please Read", vrcHeaderLearnMoreButton))
                    Application.OpenURL(CommunityLabsConstants.COMMUNITY_LABS_DOCUMENTATION_URL);
#endif

#if !UDON
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button("SDK Docs"))
            {
                Application.OpenURL("https://docs.vrchat.com/");
            }
            if (GUILayout.Button("VRChat FAQ"))
            {
                Application.OpenURL("https://vrchat.com/developer-faq");
            }
            if (GUILayout.Button("Help Center"))
            {
                Application.OpenURL("http://help.vrchat.com");
            }
            if(GUILayout.Button("Examples"))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/vrchat-kits");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
#endif
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.gray;
#if UDON

            if(GUILayout.Button("Udon Examples"))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/examples#udon--sdk3");
            };
#endif
            if (GUILayout.Button("Building VRChat Quest Content"))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/creating-content-for-the-oculus-quest");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
#if !UDON
            changeLogScroll = GUILayout.BeginScrollView(changeLogScroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(395));

            GUILayout.Label(
                @"Changelog:
2019.2.4-SDK1
- Removed upload-preventing polygon limits for avatars in SDK.
- This does not change behavior in the client. Although
you can upload content that goes beyond client limits 
(for example, the Quest polygon limit), it will not render
in VRChat according to the Minimum Performance Rank setting

2019.2.4
- Implemented Quest Avatar limits. If your avatar is
ranked as Very Poor, you will not be able to upload
the avatar.
- If you do upload an avatar beyond the Very Poor
limits, it will not render in VRChat Quest regardless

2019.2.3
- Added a VRChat/Mobile/Standard Lite for use on
Oculus Quest avatars
- Standard Lite offers slots for Diffuse, Normal maps,
Metallic+Smoothness maps, and optional Emission maps
- Standard Lite uses a simplified version of BRDF for
lighting

2019.2.2
- Added links to the splash screen leading users to our
Quest documentation
- Added Quest shaders to SDK, available under the VRChat
category
- Quest will only support these shaders on avatars. If 
you use any other shader, you'll get a warning in the
SDK. If you try to use the shader anyways, it will
fail to load in the client. You can read more about
these shaders in our documentation
- Added warnings when attempting to use unsupported
shaders on Quest
- Added errors when you attempt to upload content
too large for Quest (50mb for worlds, 10mb for
avatars). Build size is determined after package is
created. This will also be enforced in-client for Quest

2019.1.4p2
- Fixed an issue causing redundant error messaging when 
informing the user that there are objects that share the 
same path

2019.1.4
- Some additional tooltips in VRC_Mirror inspector for 
clarity
- Added a field for a custom shader on mirrors, allowing 
the mirror shader to be overridden without the need to 
swap materials 
using an animator
- Added a drop-down option for mirrors allowing for the 
setting of lower fixed resolutions

2019.1.3
- Implemented features to enable usage of Community Labs
    - Added Community Labs checkbox in the Publish World
screen
- Publishing a world no longer changes its release status.
If you update a Public World, it now remains public
- Changed and updated SDK UI in some places
- Mirrors should now display properly in the editor as 
we've moved the necessary shader into place

2019.1.1
- Content Manager now behaves better when resizing 
the window and scales the contents appropriately"
            );
            GUILayout.EndScrollView();
#endif
            GUILayout.Space(4);

            GUILayout.Box("", vrcSdkBottomHeader);
            vrcBottomHeaderLearnMoreButton = EditorStyles.miniButton;
            vrcBottomHeaderLearnMoreButton.normal.textColor = Color.black;
            vrcBottomHeaderLearnMoreButton.fontSize = 10;
            vrcBottomHeaderLearnMoreButton.border = new RectOffset(10, 10, 10, 10);
            vrcBottomHeaderLearnMoreButton.normal.background = texture;
            vrcBottomHeaderLearnMoreButton.active.background = texture;

#if UDON
            if (GUI.Button(new Rect(100, 270, 200, 60), "Join other Creators in our Discord", vrcBottomHeaderLearnMoreButton))
                Application.OpenURL("https://discord.gg/vrchat");
#else
            if (GUI.Button(new Rect(110, 525, 180, 42), "Click Here to see great\nassets for VRChat creation", vrcBottomHeaderLearnMoreButton))
                Application.OpenURL("https://assetstore.unity.com/lists/vrchat-picks-125734?aid=1101l7yuQ");
#endif

            //if (GUI.Button(new Rect(80, 540, 240, 30), "Learn how to create for VRChat Quest!", vrcBottomHeaderLearnMoreButton))
            //{
            //    Application.OpenURL("https://docs.vrchat.com/docs/creating-content-for-the-oculus-quest");
            //}

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            EditorPrefs.SetBool("VRCSDK_ShowSplashScreen", GUILayout.Toggle(EditorPrefs.GetBool("VRCSDK_ShowSplashScreen"), "Show at Startup"));

            GUILayout.EndHorizontal();
        }

    }
}