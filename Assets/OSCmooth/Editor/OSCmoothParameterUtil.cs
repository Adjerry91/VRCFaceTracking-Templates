using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace OSCTools.OSCmooth.Util
{
    public class ParameterUtil
    {
        public static bool AddVRCParameter(VRCAvatarDescriptor avatarDescriptor, List<VRCExpressionParameters.Parameter> parameters)
        {
            // Metric to gauge additional Expression Parameter's storage cost
            int paramTotalCost = 0;
            int maxCost = VRCExpressionParameters.MAX_PARAMETER_COST;

            paramTotalCost += avatarDescriptor.expressionParameters.CalcTotalCost();

            // Make sure Parameters aren't null
            if (avatarDescriptor.expressionParameters == null)
            {
                Debug.Log("ExpressionsParameters not found!");
                return false;
            }

            // Checks to see if parameter already exists; calculate new total parameter cost
            foreach (VRCExpressionParameters.Parameter param in parameters)
                if (avatarDescriptor.expressionParameters.FindParameter(param.name) == null)
                    paramTotalCost += (param.valueType == VRCExpressionParameters.ValueType.Bool ? 1 : 8);

            // exit if expected total parameter cost exceeds max of the parameters'`4 bits
            if (paramTotalCost > maxCost)
            {
                Debug.Log("Additional Expression Parameters exceed maximum storage. : " + paramTotalCost);
                return false;
            }

            // Instantiate and Save to Database
            VRCExpressionParameters newParameters = avatarDescriptor.expressionParameters;
            string assetPath = AssetDatabase.GetAssetPath(avatarDescriptor.expressionParameters);
            if (assetPath != String.Empty)
            {
                AssetDatabase.RemoveObjectFromAsset(avatarDescriptor.expressionParameters);
                AssetDatabase.CreateAsset(newParameters, assetPath);
                avatarDescriptor.expressionParameters = newParameters;
            }

            foreach (VRCExpressionParameters.Parameter parameter in parameters)
            {
                // Make sure the parameter doesn't already exist
                VRCExpressionParameters.Parameter foundParameter = newParameters.FindParameter(parameter.name);
                if (foundParameter == null || foundParameter.valueType != parameter.valueType)
                {
                    // Add the parameter
                    List<VRCExpressionParameters.Parameter> betterParametersBecauseItsAListInstead =
                        newParameters.parameters.ToList();
                    betterParametersBecauseItsAListInstead.Add(parameter);
                    newParameters.parameters = betterParametersBecauseItsAListInstead.ToArray();
                }
            }
            return true;
        }

        public static bool RemoveVRCParameter(VRCAvatarDescriptor avatarDescriptor, List<VRCExpressionParameters.Parameter> parameters)
        {
            // Make sure Parameters aren't null
            if (avatarDescriptor.expressionParameters == null)
            {
                Debug.Log("ExpressionsParameters not found!");
                return false;
            }

            // Instantiate and Save to Database
            VRCExpressionParameters newParameters = avatarDescriptor.expressionParameters;
            string assetPath = AssetDatabase.GetAssetPath(avatarDescriptor.expressionParameters);
            if (assetPath != String.Empty)
            {
                AssetDatabase.RemoveObjectFromAsset(avatarDescriptor.expressionParameters);
                AssetDatabase.CreateAsset(newParameters, assetPath);
                avatarDescriptor.expressionParameters = newParameters;
            }


            foreach (VRCExpressionParameters.Parameter parameter in parameters)
            {
                // Check and see if parameter exists
                VRCExpressionParameters.Parameter foundParameter = newParameters.FindParameter(parameter.name);
                if (foundParameter == null || foundParameter.valueType != parameter.valueType)
                {
                    // Remove the parameter
                    List<VRCExpressionParameters.Parameter> betterParametersBecauseItsAListInstead =
                        newParameters.parameters.ToList();
                    betterParametersBecauseItsAListInstead.Remove(foundParameter);
                    newParameters.parameters = betterParametersBecauseItsAListInstead.ToArray();
                }
            }
            return true;
        }

        public static bool RemoveVRCParameter(VRCAvatarDescriptor avatarDescriptor, VRCExpressionParameters.Parameter parameter)
        {
            // Make sure Parameters aren't null
            if (avatarDescriptor.expressionParameters == null)
            {
                Debug.Log("ExpressionsParameters not found!");
                return false;
            }

            // Instantiate and Save to Database
            VRCExpressionParameters newParameters = avatarDescriptor.expressionParameters;
            string assetPath = AssetDatabase.GetAssetPath(avatarDescriptor.expressionParameters);
            if (assetPath != String.Empty)
            {
                AssetDatabase.RemoveObjectFromAsset(avatarDescriptor.expressionParameters);
                AssetDatabase.CreateAsset(newParameters, assetPath);
                avatarDescriptor.expressionParameters = newParameters;
            }

            // Check and see if parameter exists
            VRCExpressionParameters.Parameter foundParameter = newParameters.FindParameter(parameter.name);
            if (newParameters.FindParameter(parameter.name) != null)
            {
                // Remove the parameter
                List<VRCExpressionParameters.Parameter> betterParametersBecauseItsAListInstead =
                    newParameters.parameters.ToList();
                betterParametersBecauseItsAListInstead.Remove(newParameters.FindParameter(parameter.name));
                newParameters.parameters = betterParametersBecauseItsAListInstead.ToArray();
            }
            return true;
        }

        public static bool RemoveVRCParameter(VRCAvatarDescriptor avatarDescriptor, string parameter)
        {
            // Make sure Parameters aren't null
            if (avatarDescriptor.expressionParameters == null)
            {
                Debug.Log("ExpressionsParameters not found!");
                return false;
            }

            // Instantiate and Save to Database
            VRCExpressionParameters newParameters = avatarDescriptor.expressionParameters;
            string assetPath = AssetDatabase.GetAssetPath(avatarDescriptor.expressionParameters);
            if (assetPath != String.Empty)
            {
                AssetDatabase.RemoveObjectFromAsset(avatarDescriptor.expressionParameters);
                AssetDatabase.CreateAsset(newParameters, assetPath);
                avatarDescriptor.expressionParameters = newParameters;
            }

            // Check and see if parameter exists
            if (newParameters.FindParameter(parameter) != null)
            {
                // Remove the parameters with listed keyword
                List<VRCExpressionParameters.Parameter> betterParametersBecauseItsAListInstead =
                    newParameters.parameters.ToList();
                foreach (VRCExpressionParameters.Parameter p in betterParametersBecauseItsAListInstead)
                    if (p.name.Contains(parameter))
                        betterParametersBecauseItsAListInstead.Remove(p);
                newParameters.parameters = betterParametersBecauseItsAListInstead.ToArray();
            }
            return true;
        }

        public static AnimatorControllerParameter CheckAndCreateParameter(string paramName, AnimatorController animatorController, AnimatorControllerParameterType type, double defaultVal = 0)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter();

            parameter.name = paramName;
            parameter.type = type;
            parameter.defaultFloat = (float)defaultVal;
            parameter.defaultInt = (int)defaultVal;
            parameter.defaultBool = Convert.ToBoolean(defaultVal);

            if (animatorController.parameters.Length == 0)
                animatorController.AddParameter(paramName, type);

            for (int j = 0; j <= animatorController.parameters.Length - 1; j++)
            {

                if (animatorController.parameters[j].name == parameter.name)
                {
                    AnimatorControllerParameter[] parameters = animatorController.parameters;
                    parameters[j] = parameter;
                    animatorController.parameters = parameters;
                    break;
                }

                if (animatorController.parameters.Length - 1 == j)
                    animatorController.AddParameter(parameter);
            }

            return parameter;
        }

        public static void FlipBaseAndProxyParameters(string paramName, string proxySuffix, AnimatorController animatorController)
        {
            List<AnimatorControllerParameter> parameters = new List<AnimatorControllerParameter>(animatorController.parameters);

            animatorController.parameters = parameters.ToArray();
        }
    }
}
