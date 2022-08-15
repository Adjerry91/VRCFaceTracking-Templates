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
        public AnimatorController animatorController;
        public List<OSCmoothParameter> parameters;

        public OSCmoothAnimationHandler() { }
        public OSCmoothAnimationHandler(List<OSCmoothParameter> smoothLayer, AnimatorController animatorController)
        {
            this.parameters = smoothLayer;
            this.animatorController = animatorController;
        }
        public void CreateSmoothAnimationLayer()
        {
            AnimatorControllerLayer animLayer;

            // Looking for existing animation layer, and will delete it to replace with a new one. Will look into
            // creating a more thorough solution to much more effectively overwrite the existing layer for a future update.
            animLayer = AnimUtil.CreateAnimLayerInController("_OSCmooth_Smoothing_Gen_", animatorController);

            // Creating a Direct BlendTree that will hold all of the smooth driver animations. This is to effectively create a 'sublayer'
            // system within the Direct BlendTree to tidy up the animator base layers from bloating up visually.
            AnimatorState[] state = new AnimatorState[2];
            state[0] = animLayer.stateMachine.AddState("OSCmooth_Local", new Vector3(30, 170, 0));
            state[1] = animLayer.stateMachine.AddState("OSCmooth_Net", new Vector3(30, 170 + 60, 0));

            var toRemoteState = state[0].AddTransition(state[1]);
            var toLocalState = state[1].AddTransition(state[0]);
            toRemoteState.duration = 0;
            toLocalState.duration = 0;

            ParameterUtil.CheckAndCreateParameter("IsLocal", animatorController, AnimatorControllerParameterType.Bool);

            toRemoteState.AddCondition(AnimatorConditionMode.IfNot, 0, "IsLocal");
            toLocalState.AddCondition(AnimatorConditionMode.If, 0, "IsLocal");

            // Creating BlendTree objects to better customize them in the AC Editor
            var basisLocalBlendTree = new BlendTree()
            {
                blendType = BlendTreeType.Direct,
                hideFlags = HideFlags.HideInHierarchy,
                name = "OSCm_Local",
                useAutomaticThresholds = false
                
            };

            var basisRemoteBlendTree = new BlendTree()
            {
                blendType = BlendTreeType.Direct,
                hideFlags = HideFlags.HideInHierarchy,
                name = "OSCm_Remote",
                useAutomaticThresholds = false
            };

            // Stuffing the BlendTrees into their designated state. Also stuffing them so that they
            // retain serialization.
            state[0].motion = basisLocalBlendTree;
            state[1].motion = basisRemoteBlendTree;
            AssetDatabase.AddObjectToAsset(basisLocalBlendTree, AssetDatabase.GetAssetPath(animLayer.stateMachine));
            AssetDatabase.AddObjectToAsset(basisRemoteBlendTree, AssetDatabase.GetAssetPath(animLayer.stateMachine));

            // Creating a '1Set' parameter that holds a value of one at all times for the Direct BlendTree
            ParameterUtil.CheckAndCreateParameter("1Set", animatorController, AnimatorControllerParameterType.Float, 1);

            List<ChildMotion> localChildMotion = new List<ChildMotion>();
            List<ChildMotion> remoteChildMotion = new List<ChildMotion>();

            // Go through each parameter and create each child to eventually stuff into the Direct BlendTrees. 
            foreach (OSCmoothParameter p in parameters)
            {
                if (p.convertToProxy)
                {
                    AnimUtil.RenameAllStateMachineInstancesOfBlendParameter(animatorController, p.paramName, p.paramName + "OSCm_Proxy");
                }

                localChildMotion.Add(new ChildMotion 
                {
                    directBlendParameter = "1Set",
                    motion = AnimUtil.CreateSmoothingBlendTree(animatorController, animLayer.stateMachine, p.localSmoothness, p.paramName, p.flipInputOutput, "OSCm_Smoother", "OSCm_Proxy"),
                    timeScale = 1
                });

                remoteChildMotion.Add(new ChildMotion
                {
                    directBlendParameter = "1Set",
                    motion = AnimUtil.CreateSmoothingBlendTree(animatorController, animLayer.stateMachine, p.remoteSmoothness, p.paramName, p.flipInputOutput, "OSCm_SmootherRemote", "OSCm_Proxy"),
                    timeScale = 1,
                });
            }

            basisLocalBlendTree.children = localChildMotion.ToArray();
            basisRemoteBlendTree.children = remoteChildMotion.ToArray();
        }
    }
}