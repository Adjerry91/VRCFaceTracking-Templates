using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using OSCTools.OSCmooth.Types;
using System.Collections.Generic;

namespace OSCTools.OSCmooth.Util
{
    public class AnimUtil
    {

        public static void CleanAnimatorBlendTreeBloat(AnimatorController animatorController, string filter)
        {
            Object[] animatorAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animatorController)); 

            foreach (Object asset in animatorAssets)
            {
                if (asset?.GetType() == typeof(BlendTree))
                {
                    if (((BlendTree)asset).name.Contains(filter))
                    {
                        Object.DestroyImmediate(asset, true);
                    }
                }
            }
        }
        public static void RenameAllStateMachineInstancesOfBlendParameter(AnimatorController animatorController, string initParameter, string newParameter)
        {
            Object[] animatorAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animatorController));

            foreach (Object asset in animatorAssets)
            {
                if (asset?.GetType() == typeof(BlendTree))
                {
                    if(((BlendTree)asset).blendParameter == initParameter)
                        ((BlendTree)asset).blendParameter = newParameter;

                    if (((BlendTree)asset).blendParameterY == initParameter)
                        ((BlendTree)asset).blendParameterY = newParameter;

                    for (int i = 0; i < ((BlendTree)asset).children.Length; i++)
                    {
                        if (((BlendTree)asset).children[i].directBlendParameter == initParameter)
                            ((BlendTree)asset).children[i].directBlendParameter = newParameter;
                    }

                    continue;
                }

                if (asset?.GetType() == typeof(AnimatorState))
                {
                    if (((AnimatorState)asset).timeParameter == initParameter)
                        ((AnimatorState)asset).timeParameter = newParameter;

                    if (((AnimatorState)asset).speedParameter == initParameter)
                        ((AnimatorState)asset).speedParameter = newParameter;

                    if (((AnimatorState)asset).cycleOffsetParameter == initParameter)
                        ((AnimatorState)asset).cycleOffsetParameter = newParameter;

                    if (((AnimatorState)asset).mirrorParameter == initParameter)
                        ((AnimatorState)asset).mirrorParameter = newParameter;
                }
            }
        }

        public static AnimatorControllerLayer CreateAnimLayerInController(string layerName, AnimatorController animatorController)
        {
            for (int i = 0; i < animatorController.layers.Length; i++)
            {
                if (animatorController.layers[i].name == layerName)
                {
                    animatorController.RemoveLayer(i);
                }
            }

            AnimatorControllerLayer layer = new AnimatorControllerLayer
            {
                name = layerName,
                stateMachine = new AnimatorStateMachine
                {
                    name = layerName,
                    hideFlags = HideFlags.HideInHierarchy
                },
                defaultWeight = 1f
            };

            CleanAnimatorBlendTreeBloat(animatorController,  "OSCm_");

            // Store Layer into Animator Controller, as creating a Layer object is not serialized unless we store it inside an asset.
            if (AssetDatabase.GetAssetPath(animatorController) != string.Empty)
            {
                AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(animatorController));
            }

            animatorController.AddLayer(layer);

            return layer;
        }

        public static AnimationClip[] CreateFloatSmootherAnimation(string paramName, string smoothSuffix, string proxySuffix, float initThreshold = 0, float finalThreshold = 1, bool driveBase = false)
        {
            AnimationClip _animationClip1 = new AnimationClip();
            AnimationClip _animationClip2 = new AnimationClip();

            AnimationCurve _curve1 = new AnimationCurve(new Keyframe(0.0f, initThreshold));
            AnimationCurve _curve2 = new AnimationCurve(new Keyframe(0.0f, finalThreshold));

            _animationClip1.SetCurve("", typeof(Animator), driveBase ? paramName : paramName + proxySuffix, _curve1);
            _animationClip2.SetCurve("", typeof(Animator), driveBase ? paramName : paramName + proxySuffix, _curve2);

            if (!Directory.Exists("Assets/OSCmooth/Generated/Anims/"))
            {
                Directory.CreateDirectory("Assets/OSCmooth/Generated/Anims/");
            }

            string[] guid = (AssetDatabase.FindAssets(paramName + initThreshold + "Smoother.anim"));

            if (guid.Length == 0)
            {
                AssetDatabase.CreateAsset(_animationClip1, "Assets/OSCmooth/Generated/Anims/" + paramName + initThreshold + smoothSuffix + ".anim");
                AssetDatabase.SaveAssets();
            }

            else
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid[0]));
                AssetDatabase.CreateAsset(_animationClip1, "Assets/OSCmooth/Generated/Anims/" + paramName + initThreshold + smoothSuffix + ".anim");
                AssetDatabase.SaveAssets();
            }

            guid = (AssetDatabase.FindAssets(paramName + finalThreshold + smoothSuffix + ".anim"));

            if (guid.Length == 0)
            {
                AssetDatabase.CreateAsset(_animationClip2, "Assets/OSCmooth/Generated/Anims/" + paramName + finalThreshold + smoothSuffix + ".anim");
                AssetDatabase.SaveAssets();
            }

            else
            {

                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid[0]));
                AssetDatabase.CreateAsset(_animationClip2, "Assets/OSCmooth/Generated/Anims/" + paramName + finalThreshold + smoothSuffix + ".anim");
                AssetDatabase.SaveAssets();
            }

            return new AnimationClip[] { _animationClip1, _animationClip2 };
        }

        public static BlendTree CreateSmoothingBlendTree(AnimatorController animatorController, AnimatorStateMachine stateMachine, float smoothness, string paramName, bool driveBase, string smoothnessSuffix = "Smoother", string proxySuffix = "Proxy")
        {
            AnimatorControllerParameter smootherParam = ParameterUtil.CheckAndCreateParameter(paramName + smoothnessSuffix, animatorController, AnimatorControllerParameterType.Float, smoothness);
            ParameterUtil.CheckAndCreateParameter(paramName + proxySuffix, animatorController, AnimatorControllerParameterType.Float);
            ParameterUtil.CheckAndCreateParameter(paramName, animatorController, AnimatorControllerParameterType.Float);

            // Creating 3 blend trees to create the feedback loop
            BlendTree rootTree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                blendParameter = paramName + smoothnessSuffix,
                name = "OSCm_" + paramName + " Root",
                useAutomaticThresholds = false
            };
            BlendTree falseTree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                blendParameter = driveBase ? paramName + proxySuffix : paramName,
                name = "OSCm_ProxyBlend",
                useAutomaticThresholds = false
            }; ;
            BlendTree trueTree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                blendParameter = driveBase ? paramName: paramName + proxySuffix,
                name = "OSCm_TrueBlend",
                useAutomaticThresholds = false
            }; ;

            // Create smoothing anims
            AnimationClip[] driverAnims = AnimUtil.CreateFloatSmootherAnimation(paramName, smoothnessSuffix, proxySuffix, -1f, 1, driveBase);

            rootTree.AddChild(falseTree, driveBase ? 1 : 0);
            rootTree.AddChild(trueTree, driveBase ? 0 : 1);

            falseTree.AddChild(driverAnims[0], -1);
            falseTree.AddChild(driverAnims[1], 1);

            trueTree.AddChild(driverAnims[0], -1);
            trueTree.AddChild(driverAnims[1], 1);

            AssetDatabase.AddObjectToAsset(rootTree, AssetDatabase.GetAssetPath(animatorController));
            AssetDatabase.AddObjectToAsset(falseTree, AssetDatabase.GetAssetPath(animatorController));
            AssetDatabase.AddObjectToAsset(trueTree, AssetDatabase.GetAssetPath(animatorController));

            return rootTree;
        }
    }
}