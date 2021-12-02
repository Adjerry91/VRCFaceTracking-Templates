#if VRC_SDK_VRCSDK2 && UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using System;
using System.Linq;
using VRC.SDKBase.Editor;

namespace VRCSDK2
{
    [CustomEditor(typeof(VRCSDK2.VRC_WebPanel))]
    public class VRC_WebPanelEditor : UnityEditor.Editor
    {
        private void InspectorField(string propertyName, string humanName)
        {
            SerializedProperty propertyField = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(propertyField, new GUIContent(humanName), true);
        }
        
        bool showFiles = false;
        System.Collections.Generic.List<string> directories = null;
        System.Collections.Generic.List<string> files = null;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.HelpBox("Do not play any videos with Web Panels, use VRC_SyncVideoPlayer instead!", MessageType.Error);

            EditorGUILayout.Space();

            InspectorField("proximity", "Proximity for Interactivity");
            EditorGUILayout.Space();

            VRCSDK2.VRC_WebPanel web = (VRCSDK2.VRC_WebPanel)target;

            if (Application.isPlaying)
            {
                InspectorField("webRoot", "Web Root");
                InspectorField("defaultUrl", "URI");

                showFiles = web.webData != null && EditorGUILayout.Foldout(showFiles, web.webData.Count.ToString() + " files imported");
                if (showFiles)
                    foreach (var file in web.webData)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(file.path);
                        EditorGUILayout.LabelField(file.data.Length.ToString());
                        EditorGUILayout.EndHorizontal();
                    }
            }
            else
            {
                SerializedProperty webRoot = serializedObject.FindProperty("webRoot");
                RenderDirectoryList(serializedObject, "webRoot", "Path To Web Content");

                if (string.IsNullOrEmpty(webRoot.stringValue))
                {
                    InspectorField("defaultUrl", "Start URI");
                }
                else
                {
                    RenderWebRootSelector(serializedObject, "defaultUrl", "Start Page");

                    if (VRCSettings.DisplayHelpBoxes)
                    {
                        EditorGUILayout.HelpBox("Javascript API bindings are called with engine.call('methodName', ...), which returns a promise-like object.", MessageType.Info);
                        EditorGUILayout.HelpBox("Javascript may call ListBindings() to discover available API bindings.", MessageType.Info);
                        EditorGUILayout.HelpBox("Javascript may listen for the 'onBindingsReady' event to execute script when the page is fully loaded and API bindings are available.", MessageType.Info);
                    }
                }
            }

            EditorGUILayout.Space();

            InspectorField("cookiesEnabled", "Enable Cookies");

            InspectorField("interactive", "Is Interactive");

			InspectorField("localOnly", "Only Visible Locally");

            if (!web.localOnly)
            {
                InspectorField("syncURI", "Synchronize URI");
                InspectorField("syncInput", "Synchronize Mouse Position");
            }

            InspectorField("transparent", "Transparent Background");

            InspectorField("autoFormSubmit", "Input should Submit Forms");

            EditorGUILayout.Space();

            InspectorField("station", "Interaction Station");
            EditorGUILayout.Space();

			InspectorField("cursor", "Mouse Cursor Object");

            EditorGUILayout.Space();

			InspectorField("resolutionWidth", "Resolution Width");
			InspectorField("resolutionHeight", "Resolution Height");
			InspectorField("displayRegion", "Display Region");

            EditorGUILayout.Space();

            InspectorField("extraVideoScreens", "Duplicate Screens");
            
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void AddSubDirectories(ref System.Collections.Generic.List<string> l, string root)
        {
            if (!Directory.Exists(root))
            {
                return;
            }

            if (!root.StartsWith(Application.dataPath + Path.DirectorySeparatorChar + "VRCSDK")
                || root == Application.dataPath + Path.DirectorySeparatorChar + "VRCSDK" + Path.DirectorySeparatorChar + "Examples" + Path.DirectorySeparatorChar + "Sample Assets" + Path.DirectorySeparatorChar + "WebRoot")
                l.Add(root.Substring(Application.dataPath.Length));

            string[] subdirectories = Directory.GetDirectories(root);
            foreach (string dir in subdirectories)
                AddSubDirectories(ref l, dir);
        }

        private void RenderDirectoryList(SerializedObject obj, string propertyName, string humanName)
        {
            if (directories == null)
            {
                directories = new System.Collections.Generic.List<string>();
                directories.Add("No Web Content Directory");

                AddSubDirectories(ref directories, Application.dataPath + Path.DirectorySeparatorChar);
            }

            SerializedProperty target = serializedObject.FindProperty(propertyName);

            int selectedIdx = target.stringValue == null ? 0 : directories.IndexOf(target.stringValue);
            if (selectedIdx < 0 || selectedIdx >= directories.Count)
                selectedIdx = 0;

            selectedIdx = EditorGUILayout.Popup(humanName, selectedIdx, directories.ToArray());
            if (selectedIdx > 0 && selectedIdx < directories.Count)
                target.stringValue = directories[selectedIdx];
            else
                target.stringValue = null;
        }

        private void AddSubDirectoryFiles(ref System.Collections.Generic.List<string> l, string root)
        {
            if (!Directory.Exists(root))
                return;

            string[] files = Directory.GetFiles(root);
            foreach (string file in files.Where(f => f.ToLower().EndsWith(".html") || f.ToLower().EndsWith(".htm")))
                l.Add(file.Substring(Application.dataPath.Length));

            string[] subdirectories = Directory.GetDirectories(root);
            foreach (string dir in subdirectories)
                AddSubDirectoryFiles(ref l, dir);
        }

        private void RenderWebRootSelector(SerializedObject obj, string propertyName, string humanName)
        {
            SerializedProperty webRoot = obj.FindProperty("webRoot");
            SerializedProperty target = serializedObject.FindProperty(propertyName);

            if (files == null)
            {
                files = new System.Collections.Generic.List<string>();

                AddSubDirectoryFiles(ref files, Application.dataPath + webRoot.stringValue);
                if (files.Count == 0)
                {
                    EditorGUILayout.HelpBox("No suitable html files found in Web Content path.", MessageType.Error);
                    return;
                }
            }

            int selectedIdx = 0;

            try
            {
                System.Uri uri = string.IsNullOrEmpty(target.stringValue) ? null : new Uri(target.stringValue);

                selectedIdx = uri == null ? 0 : files.IndexOf(uri.AbsolutePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (selectedIdx < 0 || selectedIdx >= files.Count)
                    selectedIdx = 0;
            }
            catch { }

            selectedIdx = EditorGUILayout.Popup(humanName, selectedIdx, files.ToArray());
            if (selectedIdx >= 0 && selectedIdx < files.Count)
            {
                System.UriBuilder builder = new UriBuilder()
                {
                    Scheme = "file",
                    Path = files[selectedIdx].Replace(System.IO.Path.DirectorySeparatorChar, '/'),
                    Host = ""
                };
                target.stringValue = builder.Uri.ToString();
            }
        }
    }
}
#endif