// Original ShaderKeywordsUtility by ScruffyRules#0879
// Thank you to Xiexe and all that tested!
// Licensed under the MIT License (see https://vrchat.com/legal/attribution)

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShaderKeywordsUtility : EditorWindow
{
    private static Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, Dictionary<Material, bool>> avatars = new Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, Dictionary<Material, bool>>();
    private Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, bool> avatarsOpened = new Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, bool>();
    private Vector2 scrollPos;
    private static GUIStyle titleGuiStyle;
    public static HashSet<string> keywordBlacklist = new HashSet<string>(new string[]
    { 
        // Unity Keywords, these don't matter at all. (They should be loaded)
        // All Keywords that are in Standard Unity Shaders
        "_ALPHABLEND_ON",
        "_ALPHAMODULATE_ON",
        "_ALPHAPREMULTIPLY_ON",
        "_ALPHATEST_ON",
        "_COLORADDSUBDIFF_ON",
        "_COLORCOLOR_ON",
        "_COLOROVERLAY_ON",
        "_DETAIL_MULX2",
        "_EMISSION",
        "_FADING_ON",
        "_GLOSSYREFLECTIONS_OFF",
        "_GLOSSYREFLECTIONS_OFF",
        "_MAPPING_6_FRAMES_LAYOUT",
        "_METALLICGLOSSMAP",
        "_NORMALMAP",
        "_PARALLAXMAP",
        "_REQUIRE_UV2",
        "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
        "_SPECGLOSSMAP",
        "_SPECULARHIGHLIGHTS_OFF",
        "_SPECULARHIGHLIGHTS_OFF",
        "_SUNDISK_HIGH_QUALITY",
        "_SUNDISK_NONE",
        "_SUNDISK_SIMPLE",
        "_TERRAIN_NORMAL_MAP",
        "BILLBOARD_FACE_CAMERA_POS",
        "EFFECT_BUMP",
        "EFFECT_HUE_VARIATION",
        "ETC1_EXTERNAL_ALPHA",
        "GEOM_TYPE_BRANCH",
        "GEOM_TYPE_BRANCH_DETAIL",
        "GEOM_TYPE_FROND",
        "GEOM_TYPE_LEAF",
        "GEOM_TYPE_MESH",
        "LOD_FADE_CROSSFADE",
        "PIXELSNAP_ON",
        "SOFTPARTICLES_ON",
        "STEREO_INSTANCING_ON",
        "STEREO_MULTIVIEW_ON",
        "UNITY_HDR_ON",
        "UNITY_SINGLE_PASS_STEREO",
        "UNITY_UI_ALPHACLIP",
        "UNITY_UI_CLIP_RECT",
        // Post Processing Stack V1 and V2
        // This is mostly just safe keeping somewhere
        "FOG_OFF",
        "FOG_LINEAR",
        "FOG_EXP",
        "FOG_EXP2",
        "ANTI_FLICKER",
        "UNITY_COLORSPACE_GAMMA",
        "SOURCE_GBUFFER",
        "AUTO_KEY_VALUE",
        "GRAIN",
        "DITHERING",
        "TONEMAPPING_NEUTRAL",
        "TONEMAPPING_FILMIC",
        "CHROMATIC_ABERRATION",
        "DEPTH_OF_FIELD",
        "DEPTH_OF_FIELD_COC_VIEW",
        "BLOOM",
        "BLOOM_LENS_DIRT",
        "COLOR_GRADING",
        "COLOR_GRADING_LOG_VIEW",
        "USER_LUT",
        "VIGNETTE_CLASSIC",
        "VIGNETTE_MASKED",
        "FXAA",
        "FXAA_LOW",
        "FXAA_KEEP_ALPHA",
        "STEREO_INSTANCING_ENABLED",
        "STEREO_DOUBLEWIDE_TARGET",
        "TONEMAPPING_ACES",
        "TONEMAPPING_CUSTOM",
        "APPLY_FORWARD_FOG",
        "DISTORT",
        "CHROMATIC_ABERRATION_LOW",
        "BLOOM_LOW",
        "VIGNETTE",
        "FINALPASS",
        "COLOR_GRADING_HDR_3D",
        "COLOR_GRADING_HDR",
        "AUTO_EXPOSURE"
    });

    const string keywordDescription = "Unity has a global limit of 256 keywords. A lot (~60) are used internally by Unity.\n\nAny new keyword you encounter goes onto a global list, and will stay until you restart the client.\n\nKeywords are used to create compile time branches and remove code, to optimize a shader, however, because of the 256 keyword limit, using them in VRChat can cause other shaders which use keywords to break, as once you hit the limit, any new keyword will get ignored.\n\nIt's best in the confines of VRChat to stay away from using custom keywords if possible, as not to cause issues with (your) shaders breaking.\n\nFor the full list of internal keywords, see 'ShaderKeywordsUtility.cs'";

    private static bool avatarsDirty = true;
    private int loadedScenes = 0;

    [MenuItem("VRChat SDK/Utilities/Avatar Shader Keywords Utility", false, 990)]
    static void Init()
    {
        ShaderKeywordsUtility window = EditorWindow.GetWindow<ShaderKeywordsUtility>();
        window.titleContent = new GUIContent("Shader Keywords Utility");
        window.minSize = new Vector2(325, 410);
        window.Show();

        titleGuiStyle = new GUIStyle
        {
            fontSize = 15,
            fontStyle = FontStyle.BoldAndItalic,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };

        if (EditorGUIUtility.isProSkin)
            titleGuiStyle.normal.textColor = Color.white;
        else
            titleGuiStyle.normal.textColor = Color.black;
    }

    public static List<VRC.SDKBase.VRC_AvatarDescriptor> getADescs()
    {
        List<GameObject> GOs = new List<GameObject>();
        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
        {
            Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                GameObject[] GOs2 = scene.GetRootGameObjects();
                foreach (GameObject go in GOs2)
                {
                    GOs.Add(go);
                }
            }
        }

        List<VRC.SDKBase.VRC_AvatarDescriptor> descriptors = new List<VRC.SDKBase.VRC_AvatarDescriptor>();
        foreach (GameObject go in GOs)
        {
            var vrcdescs = go.GetComponentsInChildren<VRC.SDKBase.VRC_AvatarDescriptor>(true);
            foreach (VRC.SDKBase.VRC_AvatarDescriptor vrcdesc in vrcdescs)
            {
                descriptors.Add(vrcdesc);
            }
        }

        return descriptors;
    }

    public static bool DetectCustomShaderKeywords(VRC.SDKBase.VRC_AvatarDescriptor ad)
    {
        foreach (Renderer renderer in ad.transform.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat != null)
                {
                    foreach (string keyword in mat.shaderKeywords)
                    {
                        if (!keywordBlacklist.Contains(keyword))
                            return true;
                    }
                }
            }
        }
        return false;
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Shader Keywords Utility", titleGuiStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(15);

        bool showHelp = EditorPrefs.GetBool("VRCSDK_ShowShaderKeywordsHelp", true);
        if (showHelp)
        {
            GUILayout.Label(keywordDescription, EditorStyles.helpBox, GUILayout.ExpandWidth(true));
            GUILayout.Space(15);
        }

        int _loadedScenes = 0;
        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
        {
            Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
            if (scene.isLoaded)
                _loadedScenes += 1;
        }
        if (_loadedScenes != loadedScenes)
        {
            // Debug.Log("Loaded Scenes changed");
            loadedScenes = _loadedScenes;
            avatarsDirty = true;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Avatars", GUILayout.ExpandWidth(false)))
            avatarsDirty = true;
        GUILayout.FlexibleSpace();
        if (GUILayout.Button((showHelp ? "Hide" : "Show") + " Info about Keywords"))
        {
            showHelp = !showHelp;
            EditorPrefs.SetBool("VRCSDK_ShowShaderKeywordsHelp", showHelp);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        ListAvatars();
        EditorGUILayout.EndScrollView();

    }

    void ListAvatars()
    {

        if (avatarsDirty)
            CacheAvatars();

        Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, Dictionary<Material, bool>> avatarsE = new Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, Dictionary<Material, bool>>(avatars);
        foreach (VRC.SDKBase.VRC_AvatarDescriptor vrcAD in avatarsE.Keys)
        {
            List<string> keywords = new List<string>();
            foreach (Material mat in avatars[vrcAD].Keys)
            {
                foreach (string keyword in mat.shaderKeywords)
                {
                    if (!keywords.Contains(keyword) && !keywordBlacklist.Contains(keyword))
                        keywords.Add(keyword);
                }
            }
            if (keywords.Count == 0)
            {
                avatars.Remove(vrcAD);
                avatarsOpened.Remove(vrcAD);
                continue;
            }

            GUILayout.BeginHorizontal();
            bool avatarOpened = avatarsOpened[vrcAD];
            avatarOpened = EditorGUILayout.ToggleLeft("", avatarOpened, GUILayout.MaxWidth(15f));
            avatarsOpened[vrcAD] = avatarOpened;
            EditorGUILayout.ObjectField(vrcAD, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);
            GUILayout.EndHorizontal();

            if (avatarOpened)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(23.0879f);
                GUILayout.Label("Total Custom Keywords on Avatar: " + keywords.Count);
                GUILayout.EndHorizontal();

                Dictionary<Material, bool> materials = new Dictionary<Material, bool>(avatars[vrcAD]);
                foreach (KeyValuePair<Material, bool> matKeyVal in materials)
                {
                    Material material = matKeyVal.Key;
                    bool materialOpened = matKeyVal.Value;

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(23.0879f);
                    materialOpened = EditorGUILayout.ToggleLeft("", materialOpened, GUILayout.MaxWidth(15f));
                    avatars[vrcAD][material] = materialOpened;
                    EditorGUILayout.ObjectField(material, typeof(Material), false);
                    GUILayout.EndHorizontal();

                    if (materialOpened)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(23.0879f * 2f);
                        if (GUILayout.Button("Delete ALL Keywords on this Material"))
                        {
                            if (EditorUtility.DisplayDialog("Delete All Keywords on this Material", "Are you sure you want to delete all Shader Keywords on this material?\nSome shaders might use these!", "Yes", "No"))
                            {
                                foreach (string keyword in material.shaderKeywords)
                                {
                                    if (!keywordBlacklist.Contains(keyword))
                                        material.DisableKeyword(keyword);
                                }
                                avatars[vrcAD].Remove(material);
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(23.0879f * 2f);
                        GUILayout.Label("Keywords", EditorStyles.boldLabel);
                        GUILayout.EndHorizontal();

                        int keywordsCount = 0;
                        foreach (string keyword in material.shaderKeywords)
                        {
                            if (!keywordBlacklist.Contains(keyword))
                            {
                                keywordsCount++;
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(23.0879f * 2f);
                                GUILayout.Label(keyword);
                                if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
                                    material.DisableKeyword(keyword);
                                GUILayout.EndHorizontal();
                            }
                        }
                        if (keywordsCount == 0)
                            avatars[vrcAD].Remove(material);
                    }
                }
                GUILayout.Space(2f);
            }
        }

    }

    void CacheAvatars()
    {

        //Debug.Log("Caching avatars");
        avatars = new Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, Dictionary<Material, bool>>();

        List<VRC.SDKBase.VRC_AvatarDescriptor> avatarDescriptors = getADescs();

        foreach (VRC.SDKBase.VRC_AvatarDescriptor aD in avatarDescriptors)
        {
            if (!avatars.ContainsKey(aD))
                avatars.Add(aD, new Dictionary<Material, bool>());

            if (!avatarsOpened.ContainsKey(aD))
                avatarsOpened.Add(aD, false);

            foreach (Renderer renderer in aD.transform.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (mat != null)
                    {
                        if (!avatars[aD].ContainsKey(mat))
                        {
                            foreach (string keyword in mat.shaderKeywords)
                            {
                                if (!keywordBlacklist.Contains(keyword))
                                {
                                    avatars[aD].Add(mat, false);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (avatars[aD].Count == 0)
            {
                avatars.Remove(aD);
                avatarsOpened.Remove(aD);
            }
        }
        // prevent leaking
        Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, bool> avatarsOpenedE = new Dictionary<VRC.SDKBase.VRC_AvatarDescriptor, bool>();
        foreach (VRC.SDKBase.VRC_AvatarDescriptor aO in avatarsOpenedE.Keys)
        {
            if (!avatarDescriptors.Contains(aO))
                avatarsOpened.Remove(aO);
        }
        avatarsDirty = false;

    }

}
#endif