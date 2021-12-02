using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace VRC.SDKBase.Editor
{
    public static class FixConstraintUpdateOrder
    {
        [RuntimeInitializeOnLoadMethod]
        private static void ApplyFix()
        {
            PlayerLoopSystem currentPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();

            // Search the current PlayerLoopSystem's sub-systems for the PreLateUpdate system.
            PlayerLoopSystem[] playerLoopSystems = currentPlayerLoopSystem.subSystemList;
            int preLateUpdateSystemIndex = Array.FindIndex(playerLoopSystems, system => system.type == typeof(PreLateUpdate));
            PlayerLoopSystem preLateUpdateSystem = playerLoopSystems[preLateUpdateSystemIndex];

            // Search the PreLateUpdate system's sub-systems for ScriptRunBehaviourLateUpdate and ConstraintManagerUpdate.
            List<PlayerLoopSystem> preLateUpdateSystemSubSystems = preLateUpdateSystem.subSystemList.ToList();
            PlayerLoopSystem scriptRunBehaviourLateUpdateSystem = preLateUpdateSystemSubSystems.Find(system => system.type == typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate));
            PlayerLoopSystem constraintManagerUpdateSystem = preLateUpdateSystemSubSystems.Find(system => system.type == typeof(PreLateUpdate.ConstraintManagerUpdate));

            // Move ScriptRunBehaviourLateUpdate to before ConstraintManagerUpdate.
            preLateUpdateSystemSubSystems.Remove(scriptRunBehaviourLateUpdateSystem);
            preLateUpdateSystemSubSystems.Insert(preLateUpdateSystemSubSystems.IndexOf(constraintManagerUpdateSystem), scriptRunBehaviourLateUpdateSystem);

            // Update the PlayerLoopSystem structs.
            preLateUpdateSystem.subSystemList = preLateUpdateSystemSubSystems.ToArray();
            playerLoopSystems[preLateUpdateSystemIndex] = preLateUpdateSystem;
            currentPlayerLoopSystem.subSystemList = playerLoopSystems;
            PlayerLoop.SetPlayerLoop(currentPlayerLoopSystem);
        }
    }
}
