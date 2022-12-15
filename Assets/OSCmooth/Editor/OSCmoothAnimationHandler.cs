using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Jobs;
using OSCTools.OSCmooth.Util;
using OSCTools.OSCmooth.Types;

namespace OSCTools.OSCmooth.Animation
{
    public class OSCmoothAnimationHandler
    {
        public AnimatorController _animatorController;
        public List<OSCmoothParameter> _parameters;
        public bool _writeDefaults;
        public string _animExportDirectory;

        public OSCmoothAnimationHandler() { }
        public OSCmoothAnimationHandler(List<OSCmoothParameter> smoothLayer, AnimatorController animatorController, bool writeDefaults = false)
        {
            _parameters = smoothLayer;
            _animatorController = animatorController;
            _writeDefaults = writeDefaults;
        }
        public void CreateSmoothAnimationLayer()
        {
            AnimatorControllerLayer animLayer;

            // Looking for existing animation layer, and will delete it to replace with a new one. Will look into
            // creating a more thorough solution to much more effectively overwrite the existing layer for a future update.
            if(_writeDefaults)
            {
                AnimUtil.RemoveAnimLayerInController("_OSCmooth_Smoothing_Gen", _animatorController);
                animLayer = AnimUtil.CreateAnimLayerInController("_OSCmooth_Smoothing_WD_Gen", _animatorController);
            }
            else
            {
                AnimUtil.RemoveAnimLayerInController("_OSCmooth_Smoothing_WD_Gen", _animatorController);
                animLayer = AnimUtil.CreateAnimLayerInController("_OSCmooth_Smoothing_Gen", _animatorController);              
            }

            // Creating a Direct BlendTree that will hold all of the smooth driver animations. This is to effectively create a 'sublayer'
            // system within the Direct BlendTree to tidy up the animator base layers from bloating up visually.
            AnimatorState[] state = new AnimatorState[2];

            if(_writeDefaults)
            {
                state[0] = animLayer.stateMachine.AddState("OSCmooth_Local_WD", new Vector3(30, 170, 0));
                state[1] = animLayer.stateMachine.AddState("OSCmooth_Net_WD", new Vector3(30, 170 + 60, 0));
            }
            else
            {
                state[0] = animLayer.stateMachine.AddState("OSCmooth_Local", new Vector3(30, 170, 0));
                state[1] = animLayer.stateMachine.AddState("OSCmooth_Net", new Vector3(30, 170 + 60, 0));
            }
            

            state[0].writeDefaultValues = _writeDefaults;
            state[1].writeDefaultValues = _writeDefaults;

            var toRemoteState = state[0].AddTransition(state[1]);
            toRemoteState.duration = 0;

            var toLocalState = state[1].AddTransition(state[0]);
            toLocalState.duration = 0;

            ParameterUtil.CheckAndCreateParameter("IsLocal", _animatorController, AnimatorControllerParameterType.Bool);

            toRemoteState.AddCondition(AnimatorConditionMode.IfNot, 0, "IsLocal");
            toLocalState.AddCondition(AnimatorConditionMode.If, 0, "IsLocal");

            // Creating BlendTree objects to better customize them in the AC Editor
            var nameLocalWD = "OSCm_Local";
            var nameRemoteWD = "OSCm_Remote";

            if (_writeDefaults)
            {
                nameLocalWD = "OSCm_Local_WD";
                nameRemoteWD = "OSCm_Remote_WD";                
            }

            var basisLocalBlendTree = new BlendTree()
            {
                blendType = BlendTreeType.Direct,
                hideFlags = HideFlags.HideInHierarchy,
                name = nameLocalWD,
                useAutomaticThresholds = false

            };

            var basisRemoteBlendTree = new BlendTree()
            {
                blendType = BlendTreeType.Direct,
                hideFlags = HideFlags.HideInHierarchy,
                name = nameRemoteWD,
                useAutomaticThresholds = false
            };


            // Stuffing the BlendTrees into their designated state. Also stuffing them so that they
            // retain serialization.
            state[0].motion = basisLocalBlendTree;
            state[1].motion = basisRemoteBlendTree;
            AssetDatabase.AddObjectToAsset(basisLocalBlendTree, AssetDatabase.GetAssetPath(animLayer.stateMachine));
            AssetDatabase.AddObjectToAsset(basisRemoteBlendTree, AssetDatabase.GetAssetPath(animLayer.stateMachine));

            // Creating a '1Set' parameter that holds a value of one at all times for the Direct BlendTree
            
            if (_writeDefaults)
            {
                ParameterUtil.CheckAndCreateParameter("OSCm/BlendSet", _animatorController, AnimatorControllerParameterType.Float, 1f);
            }
            else
            {
                ParameterUtil.CheckAndCreateParameter("OSCm/BlendSet", _animatorController, AnimatorControllerParameterType.Float, 1f / (float)_parameters.Count);
            }


            List<ChildMotion> localChildMotion = new List<ChildMotion>();
            List<ChildMotion> remoteChildMotion = new List<ChildMotion>();

            // Go through each parameter and create each child to eventually stuff into the Direct BlendTrees. 
            foreach (OSCmoothParameter p in _parameters)
            {
                if (p.convertToProxy)
                {
                    AnimUtil.RenameAllStateMachineInstancesOfBlendParameter(_animatorController, p.paramName, "OSCm/Proxy/" + p.paramName);
                }

                var motionLocal = AnimUtil.CreateSmoothingBlendTree(_animatorController, animLayer.stateMachine, p.localSmoothness, p.paramName, p.flipInputOutput, (float)_parameters.Count, _animExportDirectory, "OSCm/Local/", "Smoother", "OSCm/Proxy/", "Proxy");
                var motionRemote = AnimUtil.CreateSmoothingBlendTree(_animatorController, animLayer.stateMachine, p.remoteSmoothness, p.paramName, p.flipInputOutput, (float)_parameters.Count, _animExportDirectory, "OSCm/Remote/", "SmootherRemote", "OSCm/Proxy/", "Proxy");
                if (_writeDefaults)
                {
                    motionLocal = AnimUtil.CreateSmoothingBlendTree(_animatorController, animLayer.stateMachine, p.localSmoothness, p.paramName, p.flipInputOutput, 1f, _animExportDirectory, "OSCm/Local/", "SmootherWD", "OSCm/Proxy/", "Proxy");
                    motionRemote = AnimUtil.CreateSmoothingBlendTree(_animatorController, animLayer.stateMachine, p.remoteSmoothness, p.paramName, p.flipInputOutput, 1f, _animExportDirectory, "OSCm/Remote/", "SmootherRemoteWD", "OSCm/Proxy/", "Proxy");
                }              

                localChildMotion.Add(new ChildMotion 
                {
                    directBlendParameter = "OSCm/BlendSet",
                    motion = motionLocal,
                    timeScale = 1
                });

                remoteChildMotion.Add(new ChildMotion
                {
                    directBlendParameter = "OSCm/BlendSet",
                    motion = motionRemote,
                    timeScale = 1,
                });
            }

            basisLocalBlendTree.children = localChildMotion.ToArray();
            basisRemoteBlendTree.children = remoteChildMotion.ToArray();
        }
    }
}