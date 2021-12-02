#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

public partial class AvatarDescriptorEditor3 : Editor
{

    SkinnedMeshRenderer selectedMesh;
    List<string> blendShapeNames = null;

    bool shouldRefreshVisemes = false;

    bool lipsyncFoldout;

    public void DrawLipSync()
    {
        if (Foldout("VRCSDK3_AvatarDescriptorEditor3_LipSyncFoldout", "LipSync"))
        {

            avatarDescriptor.lipSync = (VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle)EditorGUILayout.EnumPopup("Mode", avatarDescriptor.lipSync);
            switch (avatarDescriptor.lipSync)
            {

                case VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.Default:
                    if (GUILayout.Button("Auto Detect!"))
                        AutoDetectLipSync();
                    break;

                case VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.JawFlapBlendShape:
                    avatarDescriptor.VisemeSkinnedMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", avatarDescriptor.VisemeSkinnedMesh, typeof(SkinnedMeshRenderer), true);
                    if (avatarDescriptor.VisemeSkinnedMesh != null)
                    {
                        DetermineBlendShapeNames();

                        int current = -1;
                        for (int b = 0; b < blendShapeNames.Count; ++b)
                            if (avatarDescriptor.MouthOpenBlendShapeName == blendShapeNames[b])
                                current = b;

                        string title = "Jaw Flap Blend Shape";
                        int next = EditorGUILayout.Popup(title, current, blendShapeNames.ToArray());
                        if (next >= 0)
                            avatarDescriptor.MouthOpenBlendShapeName = blendShapeNames[next];
                    }
                    break;

                case VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone:
                    DrawJawBone();
                    break;

                case VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape:
                    SkinnedMeshRenderer prev = avatarDescriptor.VisemeSkinnedMesh;
                    avatarDescriptor.VisemeSkinnedMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", avatarDescriptor.VisemeSkinnedMesh, typeof(SkinnedMeshRenderer), true);
                    if (avatarDescriptor.VisemeSkinnedMesh != prev)
                        shouldRefreshVisemes = true;
                    if (avatarDescriptor.VisemeSkinnedMesh != null)
                    {
                        DetermineBlendShapeNames();

                        if (avatarDescriptor.VisemeBlendShapes == null || avatarDescriptor.VisemeBlendShapes.Length != (int)VRC.SDKBase.VRC_AvatarDescriptor.Viseme.Count)
                            avatarDescriptor.VisemeBlendShapes = new string[(int)VRC.SDKBase.VRC_AvatarDescriptor.Viseme.Count];
                        for (int i = 0; i < (int)VRC.SDKBase.VRC_AvatarDescriptor.Viseme.Count; ++i)
                        {
                            int current = -1;
                            for (int b = 0; b < blendShapeNames.Count; ++b)
                                if (avatarDescriptor.VisemeBlendShapes[i] == blendShapeNames[b])
                                    current = b;

                            string title = "Viseme: " + ((VRC.SDKBase.VRC_AvatarDescriptor.Viseme)i).ToString();
                            int next = EditorGUILayout.Popup(title, current, blendShapeNames.ToArray());
                            if (next >= 0)
                                avatarDescriptor.VisemeBlendShapes[i] = blendShapeNames[next];
                        }

                        if (shouldRefreshVisemes)
                            AutoDetectVisemes();
                    }
                    break;

                case VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.VisemeParameterOnly:
                    break;
            }
            Separator();
        }
    }
    public void DrawSceneLipSync()
    {
        var transformProp = serializedObject.FindProperty("lipSyncJawBone");
        DrawRotationState(serializedObject.FindProperty("lipSyncJawClosed"));
        DrawRotationState(serializedObject.FindProperty("lipSyncJawOpen"));

        void DrawRotationState(SerializedProperty property)
        {
            //Check if active
            if (!IsActiveProperty(property))
                return;

            //Draw handle
            DrawRotationHandles(transformProp, property);
        }
    }

    void DrawJawBone()
    {
        var lipSyncJawClosed = serializedObject.FindProperty("lipSyncJawClosed");
        var lipSyncJawOpen = serializedObject.FindProperty("lipSyncJawOpen");

        //Transform
        EditorGUI.BeginChangeCheck();
        avatarDescriptor.lipSyncJawBone = (Transform)EditorGUILayout.ObjectField("Jaw Bone", avatarDescriptor.lipSyncJawBone, typeof(Transform), true);
        if(EditorGUI.EndChangeCheck())
        {
            if(avatarDescriptor.lipSyncJawBone != null)
            {
                lipSyncJawClosed.quaternionValue = avatarDescriptor.lipSyncJawBone.localRotation;
                lipSyncJawOpen.quaternionValue = avatarDescriptor.lipSyncJawBone.localRotation;
            }
        }

        //Rotation states
        EditorGUILayout.LabelField("Rotation States");
        EditorGUI.indentLevel += 1;
        GUI.enabled = avatarDescriptor.lipSyncJawBone != null;
        {
            DrawRotationState("Closed", lipSyncJawClosed);
            DrawRotationState("Open", lipSyncJawOpen);
        }
        GUI.enabled = true;
        EditorGUI.indentLevel -= 1;
        

        void DrawRotationState(string name, SerializedProperty property)
        {
            GUILayout.BeginHorizontal();
            {
                //Vector
                EditorGUI.BeginChangeCheck();
                var result = EditorGUILayout.Vector3Field(name, EditorQuaternionToVector3(property.quaternionValue));
                if (EditorGUI.EndChangeCheck())
                {
                    property.quaternionValue = Quaternion.Euler(result);
                    SetActive();
                }

                //Edit Button
                bool isActiveProperty = IsActiveProperty(property);
                GUI.backgroundColor = isActiveProperty ? _activeButtonColor : Color.white;
                if (GUILayout.Button(isActiveProperty ? "Return" : "Preview", EditorStyles.miniButton, GUILayout.MaxWidth(PreviewButtonWidth), GUILayout.Height(PreviewButtonHeight)))
                {
                    if (isActiveProperty)
                        SetActiveProperty(null);
                    else
                        SetActive();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();

            void SetActive()
            {
                //Set active
                SetActiveProperty(property);

                //Record restore point
                var prevBone = avatarDescriptor.lipSyncJawBone;
                var prevRotation = prevBone.transform.localRotation;
                System.Action restore = () =>
                {
                    if (prevBone != null)
                        prevBone.localRotation = prevRotation;
                };
                activePropertyRestore.Add(restore);

                //Set
                avatarDescriptor.lipSyncJawBone.transform.localRotation = property.quaternionValue;
            }
        }
    }

    void DetermineBlendShapeNames()
    {
        if (avatarDescriptor.VisemeSkinnedMesh != null &&
            avatarDescriptor.VisemeSkinnedMesh != selectedMesh)
        {
            blendShapeNames = new List<string>();
            blendShapeNames.Add("-none-");
            selectedMesh = avatarDescriptor.VisemeSkinnedMesh;
            for (int i = 0; i < selectedMesh.sharedMesh.blendShapeCount; ++i)
                blendShapeNames.Add(selectedMesh.sharedMesh.GetBlendShapeName(i));
        }
    }

    void AutoDetectVisemes()
    {
        // prioritize strict - but fallback to looser - naming and don't touch user-overrides

        List<string> blendShapes = new List<string>(blendShapeNames);
        blendShapes.Remove("-none-");

        for (int v = 0; v < avatarDescriptor.VisemeBlendShapes.Length; v++)
        {
            if (string.IsNullOrEmpty(avatarDescriptor.VisemeBlendShapes[v]))
            {
                string viseme = ((VRC.SDKBase.VRC_AvatarDescriptor.Viseme)v).ToString().ToLowerInvariant();

                foreach (string s in blendShapes)
                {
                    if (s.ToLowerInvariant() == "vrc.v_" + viseme)
                    {
                        avatarDescriptor.VisemeBlendShapes[v] = s;
                        goto next;
                    }
                }
                foreach (string s in blendShapes)
                {
                    if (s.ToLowerInvariant() == "v_" + viseme)
                    {
                        avatarDescriptor.VisemeBlendShapes[v] = s;
                        goto next;
                    }
                }
                foreach (string s in blendShapes)
                {
                    if (s.ToLowerInvariant().EndsWith(viseme))
                    {
                        avatarDescriptor.VisemeBlendShapes[v] = s;
                        goto next;
                    }
                }
                foreach (string s in blendShapes)
                {
                    if (s.ToLowerInvariant() == viseme)
                    {
                        avatarDescriptor.VisemeBlendShapes[v] = s;
                        goto next;
                    }
                }
                foreach (string s in blendShapes)
                {
                    if (s.ToLowerInvariant().Contains(viseme))
                    {
                        avatarDescriptor.VisemeBlendShapes[v] = s;
                        goto next;
                    }
                }
                next: { }
            }
            
        }

        shouldRefreshVisemes = false;
    }
    
    class SearchComparer : IComparer<string>
    {
        string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        
        public SearchComparer(string searchString)
        {
            _searchString = searchString;
        }
        private readonly string _searchString;
        public int Compare(string x, string y)
        {
            if (x == null || y == null)
            {
                return 0;
            }
            //-1 is they're out of order, 0 is order doesn't matter, 1 is they're in order

            x = ReplaceFirst(x, "const ", "");
            y = ReplaceFirst(y, "const ", "");
            
            int xIndex = x.IndexOf(_searchString, StringComparison.InvariantCultureIgnoreCase);
            int yIndex = y.IndexOf(_searchString, StringComparison.InvariantCultureIgnoreCase);
            int compareIndex = xIndex.CompareTo(yIndex);
            if (compareIndex != 0) return compareIndex;
            
            string xDiff = ReplaceFirst(x, _searchString, "");
            string yDiff = ReplaceFirst(y, _searchString, "");
            return string.Compare(xDiff, yDiff, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    private void AutoDetectLipSync()
    {
        SkinnedMeshRenderer[] renderers = avatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>();

        string[] baseVisemeNames = Enum.GetNames(typeof(VRC_AvatarDescriptor.Viseme));
        int visemeCount = baseVisemeNames.Length - 1;
        string[] reversedVisemeNames = new string[visemeCount];
        string[] reversedVVisemeNames = new string[visemeCount];
        for (int i = 0; i < visemeCount; i++)
        {
            string visemeName = baseVisemeNames[i];
            char[] tmpArray = visemeName.ToLowerInvariant().ToCharArray();
            Array.Reverse(tmpArray);
            reversedVisemeNames[i] = new string(tmpArray);
            reversedVVisemeNames[i] = $"{reversedVisemeNames[i]}_v";
        }

        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer.sharedMesh.blendShapeCount <= 0) continue;

            if (renderer.sharedMesh.blendShapeCount >= visemeCount)
            {
                string[] rendererBlendShapeNames = new string[renderer.sharedMesh.blendShapeCount];
                for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                {
                    rendererBlendShapeNames[i] = renderer.sharedMesh.GetBlendShapeName(i);
                }
                
                string[] visemeStrings = new string[visemeCount];
                int foundVisemes = 0;

                string[] reversedRendererNames = new string[rendererBlendShapeNames.Length];
                Dictionary<string, string> reverseMap = new Dictionary<string, string>();
                
                for (int i = 0; i < rendererBlendShapeNames.Length; i++)
                {
                    string rendererBlendShapeName = rendererBlendShapeNames[i];
                    char[] tmpArray = rendererBlendShapeName.ToLowerInvariant().ToCharArray();
                    Array.Reverse(tmpArray);
                    reversedRendererNames[i] = new string(tmpArray);
                    if (reverseMap.ContainsKey(reversedRendererNames[i]))
                    {
                        continue;
                    }
                    reverseMap.Add(reversedRendererNames[i], rendererBlendShapeName);
                }

                for (int i = 0; i < reversedVisemeNames.Length; i++)
                {
                    string visemeName = reversedVisemeNames[i];
                    string vVisemeName = reversedVVisemeNames[i];
                    
                    List<string> matchingStrings = new List<string>();
                    foreach (string reversedRendererName in reversedRendererNames)
                    {
                        if (reversedRendererName.Contains(vVisemeName))
                        {
                            matchingStrings.Add(reversedRendererName);
                        }
                    }
                    if (matchingStrings.Count == 0)
                    {
                        foreach (string reversedRendererName in reversedRendererNames)
                        {
                            if (reversedRendererName.Contains(visemeName))
                            {
                                matchingStrings.Add(reversedRendererName);
                            }
                        }
                    }

                    matchingStrings.Sort(new SearchComparer(visemeName));
        
                    if (matchingStrings.Count <= 0) continue;
                    
                    visemeStrings[i] = reverseMap[matchingStrings[0]];
                    foundVisemes++;
                }

                //Threshold to see if we did a good enough job to bother showing the user
                if (foundVisemes > 2)
                {
                    avatarDescriptor.lipSync = VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
                    avatarDescriptor.VisemeSkinnedMesh = renderer;
                    avatarDescriptor.VisemeBlendShapes = visemeStrings;
                    avatarDescriptor.lipSyncJawBone = null;
                    return;
                }
            }
            
            avatarDescriptor.lipSync = VRC_AvatarDescriptor.LipSyncStyle.JawFlapBlendShape;
            avatarDescriptor.VisemeSkinnedMesh = renderer;
            avatarDescriptor.lipSyncJawBone = null;
            return;
        }

        
        if (avatarDescriptor.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Jaw) == null) return;
        avatarDescriptor.lipSync = VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone;
        avatarDescriptor.lipSyncJawBone = avatarDescriptor.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Jaw);
        avatarDescriptor.VisemeSkinnedMesh = null;
    }
}
#endif
