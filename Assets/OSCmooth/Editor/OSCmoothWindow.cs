using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using OSCTools.OSCmooth.Animation;
using OSCTools.OSCmooth.Types;
using OSCTools.OSCmooth.Util;
using System.IO;
using System.Linq;

namespace OSCTools.OSCmooth
{
    public class OSCmoothWindow : EditorWindow
    {
        private VRCAvatarDescriptor _avDescriptor;
        private AnimatorController _animatorController;
        private int _layerSelect = 4;
        private OSCmoothLayer _parameterAsset;
        private OSCmoothParameter _basisConfigurationParameter;
        private bool _showParameters = true;
        private bool _showGlobalConfiguration = false;
        private bool _writeDefaults = false;
        private Vector2 paramMenuScroll;

        readonly private string[] _animatorSelection = new string[]
        {
            "Base", "Additive", "Gesture", "Action", "FX"
        };

        readonly private string[] oscmBlacklist = new string[]
        {
            "OSCm_", "IsLocal", "Smooth", "Proxy", "_Float", "_Normalizer", "_FTI"
        };

        [MenuItem("Tools/OSCmooth")]
        public static void ShowWindow()
        {
            AssetDatabase.Refresh();
            var window = EditorWindow.GetWindow<OSCmoothWindow>("OSCmooth");
            window.maxSize = new Vector2(512, 1024);
            window.minSize = new Vector2(368, 480);
            window.Show();
        }

        private void OnGUI()
        {
            _avDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField
            (
                new GUIContent
                (
                    "Avatar",
                    "The VRC Avatar that will have the smoothing animation layers set up on. " +
                    "The Avatar must have a VRCAvatarDescriptor to show up in this field."
                ),
                _avDescriptor,
                typeof(VRCAvatarDescriptor),
                true
            );

            if (_avDescriptor != null)
            {
                _layerSelect = EditorGUILayout.Popup
                (
                    new GUIContent
                    (
                        "Layer",
                        "This selects what VRChat Playable Layer you would like to set up " +
                        "the following Binary Animation Layer into. A layer must be populated " +
                        "in order for the tool to properly set up an Animation Layer."
                    ),
                    _layerSelect,
                    _animatorSelection
                );

                EditorGUILayout.Space(10f);

                _parameterAsset = (OSCmoothLayer)EditorGUILayout.ObjectField
                (
                    new GUIContent
                    (
                        "Config",
                        "A preset configuration that stores Parameter Configuration data. " +
                        "This is intended for saving configurations for use later or sharing."
                    ),
                    _parameterAsset,
                    typeof(OSCmoothLayer),
                    false
                );

                if (_parameterAsset == null)
                    _parameterAsset = ScriptableObject.CreateInstance<OSCmoothLayer>();

                if (_basisConfigurationParameter == null)
                    _basisConfigurationParameter = new OSCmoothParameter();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button
                (
                    new GUIContent
                    (
                        "Save Config",
                        "Saves Parameter configuration into a JSON readable text file."
                    ),
                    GUILayout.MaxWidth((float)Screen.width - 159f)
                ))
                {
                    if (AssetDatabase.GetAssetPath(_parameterAsset) == string.Empty)
                        AssetDatabase.CreateAsset(_parameterAsset, EditorUtility.SaveFilePanelInProject("Save OSCmooth Configuration", "OSCmoothConfig", "asset", ""));

                    EditorUtility.SetDirty(_parameterAsset);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.EndHorizontal();

                _writeDefaults = EditorGUILayout.Toggle
                (
                    new GUIContent
                    (
                        "Write Defaults",
                        "Sets whether the generated OSCmooth layer will have write defaults on or off. Set true for WD on, false for WD off"
                    ),
                    _writeDefaults
                );

                _animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(_avDescriptor.baseAnimationLayers[_layerSelect].animatorController));

                if (_animatorController == null)
                {
                    EditorGUILayout.HelpBox("This Playable Layer must have an Animator Controller in order for OSCmooth to function.", MessageType.Warning, true);
                    return;
                }

                EditorGUILayout.Space();
                if (GUILayout.Button
                (
                    new GUIContent
                    (
                        "Use Playable Layer Parameters",
                        "Populates the parameter list with exising float parameters in the applied Playable Layer controller."
                    )
                ))
                {
                    _parameterAsset = ScriptableObject.CreateInstance<OSCmoothLayer>();

                    foreach (AnimatorControllerParameter parameter in _animatorController.parameters)
                    {
                        if (parameter.type == AnimatorControllerParameterType.Float && !oscmBlacklist.Any(parameter.name.Contains))
                        {
                            _parameterAsset.parameters.Add(new OSCmoothParameter
                            {
                                paramName = parameter.name,
                                localSmoothness = _basisConfigurationParameter.localSmoothness,
                                remoteSmoothness = _basisConfigurationParameter.remoteSmoothness,
                                flipInputOutput = _basisConfigurationParameter.flipInputOutput,
                                convertToProxy = _basisConfigurationParameter.convertToProxy
                            });
                        }
                    }

                    EditorUtility.SetDirty(_parameterAsset);
                    AssetDatabase.SaveAssets();

                }
                EditorGUILayout.Space();

                _showGlobalConfiguration = EditorGUILayout.Foldout(_showGlobalConfiguration, "Default Parameter Values");
                if (_showGlobalConfiguration)
                {
                    DrawParameterConfiguration(_basisConfigurationParameter, false);
                }

                _parameterAsset.configuration = _basisConfigurationParameter;

                EditorGUI.indentLevel = 0;

                _showParameters = EditorGUILayout.Foldout(_showParameters, "Parameter Configuration");

                EditorGUI.indentLevel = 0;

                paramMenuScroll = EditorGUILayout.BeginScrollView(paramMenuScroll);
                if (_showParameters && _parameterAsset != null)
                {                    
                    foreach (OSCmoothParameter parameter in _parameterAsset.parameters)
                    {
                        EditorGUI.indentLevel = 1;
                        parameter.isVisible = EditorGUILayout.Foldout(parameter.isVisible, parameter.paramName);

                        EditorGUI.indentLevel = 2;
                        if (parameter.isVisible)
                        {
                            DrawParameterConfiguration(parameter);
                        }
                    }
                }

                EditorGUILayout.Space();

                if (GUILayout.Button
                (
                    new GUIContent
                    (
                        "Add New Parameter",
                        "Adds a new Parameter configuration."
                    )
                ))
                {
                    OSCmoothParameter param = new OSCmoothParameter
                    {
                        paramName = _basisConfigurationParameter.paramName,
                        localSmoothness = _basisConfigurationParameter.localSmoothness,
                        remoteSmoothness = _basisConfigurationParameter.remoteSmoothness,
                        flipInputOutput = _basisConfigurationParameter.flipInputOutput,
                        convertToProxy = _basisConfigurationParameter.convertToProxy
                    };
                    _parameterAsset.parameters.Add(param);

                    EditorUtility.SetDirty(_parameterAsset);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(20);

                if (GUILayout.Button
                (
                    new GUIContent
                    (
                        "Apply OSCmooth to Animator",
                        "Creates a new Layer in the selected Animator Controller that will apply smoothing " +
                        "to the listed configured parameters."
                    )
                ))
                {
                    OSCmoothAnimationHandler animHandler = new OSCmoothAnimationHandler();

                    string animatorGUID;
                    long id;

                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_animatorController, out animatorGUID, out id);

                    animHandler._animatorController = _animatorController;
                    animHandler._parameters = _parameterAsset.parameters;
                    animHandler._writeDefaults = _writeDefaults;
                    animHandler._animExportDirectory = "Assets/OSCmooth/Generated/Anims/" + "Animator_" + animatorGUID + "/";

                    animHandler.CreateSmoothAnimationLayer();
                }

                EditorGUILayout.Space(20);
            }
        }

        public void DrawParameterConfiguration(OSCmoothParameter parameter, bool removable = true)
        {
            EditorGUIUtility.labelWidth = 210;

            parameter.paramName = EditorGUILayout.TextField
            (
                new GUIContent
                (
                    "",
                    "The specific float parameter that will have the smoothness layer have applied to."
                ),
                parameter.paramName
            );

            EditorGUILayout.LabelField
            (
                new GUIContent
                (
                    "Smoothness",
                    "How much of a percentage the previous float values influence the current one."
                )
            );

            EditorGUI.indentLevel = 3;
            EditorGUIUtility.labelWidth = 240;

            parameter.localSmoothness = EditorGUILayout.FloatField
            (
                new GUIContent
                (
                    "Local Smoothness",
                    "How much % smoothness you (locally) will see when a parameter " +
                    "changes value. Higher values represent more smoothness, and vice versa."
                ),
                parameter.localSmoothness
            );

            parameter.remoteSmoothness = EditorGUILayout.FloatField
            (
                new GUIContent
                (
                    "Remote Smoothness",
                    "How much % smoothness remote users will see when a parameter " +
                    "changes value. Higher values represent more smoothness, and vice versa."
                ),
                parameter.remoteSmoothness
            );

            parameter.convertToProxy = EditorGUILayout.Toggle
            (
                new GUIContent
                (
                    "Proxy Conversion",
                    "Automatically convert existing animations to use the Proxy (output) float."
                ),
                parameter.convertToProxy
            );

            parameter.flipInputOutput = EditorGUILayout.Toggle
            (
                new GUIContent
                (
                    "Flip Input/Output",
                    "Sets the Base parameter to be the output parameter from the smoother layer, and " +
                    "sets the Proxy parameter as the input driver parameter. Useful for apps that can " +
                    "drive the Proxy parameter like VRCFaceTracking binary parameters."
                ),
                parameter.flipInputOutput
            );

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (removable)
            {
                if (GUILayout.Button
                (
                    new GUIContent
                    (
                        "Remove Parameter",
                        "Removes specified parameter from smoothness creation."
                    ),
                    GUILayout.MaxWidth((float)Screen.width - 248f)
                ))
                {
                    _parameterAsset.parameters.Remove(parameter);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }
}