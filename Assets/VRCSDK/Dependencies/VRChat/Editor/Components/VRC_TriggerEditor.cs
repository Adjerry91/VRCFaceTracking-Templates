#if VRC_SDK_VRCSDK2 && UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;
using VRC.SDKBase;
using VRC.SDKBase.Editor;

namespace VRCSDK2
{
    [CustomPropertyDrawer(typeof(VRCSDK2.VRC_Trigger.CustomTriggerTarget))]
    public class CustomTriggerTargetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (Application.isPlaying)
            {
                EditorGUI.HelpBox(rect, "Trigger Editor disabled while application is running.", MessageType.Info);
                return;
            }

            if (property == null)
                return;

            SerializedProperty objectProperty = property.FindPropertyRelative("TriggerObject");
            SerializedProperty nameProperty = property.FindPropertyRelative("CustomName");

            EditorGUI.BeginProperty(rect, label, property);

            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);
            Rect objectRect = new Rect(rect.x, rect.y, rect.width / 2 - 5, rect.height);
            Rect nameRect = new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

            VRCSDK2.VRC_Trigger current = null;
            if (objectProperty.objectReferenceValue != null)
                current = (objectProperty.objectReferenceValue as GameObject).GetComponent<VRCSDK2.VRC_Trigger>();
            current = EditorGUI.ObjectField(objectRect, current, typeof(VRCSDK2.VRC_Trigger), true) as VRCSDK2.VRC_Trigger;
            objectProperty.objectReferenceValue = current == null ? null : current.gameObject;

            VRC_EditorTools.CustomTriggerPopup(nameRect, objectProperty, nameProperty);

            EditorGUI.EndProperty();
        }
    }

    [CustomEditor(typeof(VRCSDK2.VRC_Trigger)), CanEditMultipleObjects]
    public class VRC_TriggerEditor : UnityEditor.Editor
    {
        private List<VRCSDK2.VRC_Trigger.TriggerType> ActiveTypes
        {
            get
            {
                List<VRCSDK2.VRC_Trigger.TriggerType> activeTypes = new List<VRCSDK2.VRC_Trigger.TriggerType>();

                SerializedProperty triggers = triggersProperty.Copy();
                int triggersLength = triggers.arraySize;

                for (int idx = 0; idx < triggersLength; ++idx)
                {
                    VRCSDK2.VRC_Trigger.TriggerType triggerType = (VRCSDK2.VRC_Trigger.TriggerType)triggers.GetArrayElementAtIndex(idx).FindPropertyRelative("TriggerType").intValue;
                    activeTypes.Add(triggerType);
                }

                return activeTypes;
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private static List<VRCSDK2.VRC_Trigger.TriggerType> hiddenTriggerTypes = new List<VRCSDK2.VRC_Trigger.TriggerType> { /*VRCSDK2.VRC_Trigger.TriggerType.OnDataStorageAdd, VRCSDK2.VRC_Trigger.TriggerType.OnDataStorageRemove*/ };
        private static List<VRCSDK2.VRC_EventHandler.VrcEventType> hiddenEventTypes = new List<VRCSDK2.VRC_EventHandler.VrcEventType> { VRCSDK2.VRC_EventHandler.VrcEventType.MeshVisibility, VRCSDK2.VRC_EventHandler.VrcEventType.SendMessage, VRCSDK2.VRC_EventHandler.VrcEventType.RunConsoleCommand };
        private static List<VRCSDK2.VRC_EventHandler.VrcBroadcastType> unbufferedBroadcastTypes = new List<VRCSDK2.VRC_EventHandler.VrcBroadcastType> { VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysUnbuffered, VRCSDK2.VRC_EventHandler.VrcBroadcastType.MasterUnbuffered, VRCSDK2.VRC_EventHandler.VrcBroadcastType.OwnerUnbuffered, VRCSDK2.VRC_EventHandler.VrcBroadcastType.Local };
        private static List<VRCSDK2.VRC_EventHandler.VrcBroadcastType> bufferOneBroadcastTypes = new List<VRCSDK2.VRC_EventHandler.VrcBroadcastType> { VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysBufferOne, VRCSDK2.VRC_EventHandler.VrcBroadcastType.MasterBufferOne, VRCSDK2.VRC_EventHandler.VrcBroadcastType.OwnerBufferOne };
        private static List<VRCSDK2.VRC_EventHandler.VrcBroadcastType> hiddenBroadcastTypes = new List<VRCSDK2.VRC_EventHandler.VrcBroadcastType> { };
#pragma warning restore CS0618 // Type or member is obsolete

        private ReorderableList[] eventLists = new ReorderableList[0];
        private ReorderableList[] relayLists = new ReorderableList[0];
        private ReorderableList[] objectLists = new ReorderableList[0];
        private bool[] visible = new bool[0];

        private SerializedProperty triggersProperty;
        private SerializedProperty proximityProperty;
        private SerializedProperty interactTextProperty;
        private SerializedProperty ownershipProperty;
        private SerializedProperty drawLinesProperty;

        private Dictionary<string, object[]> rpcByteCache = new Dictionary<string, object[]>();

        private VRCSDK2.VRC_Trigger.TriggerType addTriggerSelectedType = VRCSDK2.VRC_Trigger.TriggerType.Custom;

        private void OnEnable()
        {
            rpcByteCache.Clear();
        }

        public override void OnInspectorGUI()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            bool showedOldWarning = false;
            foreach (VRCSDK2.VRC_Trigger t in targets.Cast<VRCSDK2.VRC_Trigger>().Where(tr => tr != null))
            {
                if (!showedOldWarning && (t.GetComponent<VRCSDK2.VRC_UseEvents>() != null || t.GetComponent<VRCSDK2.VRC_KeyEvents>() != null || t.GetComponent<VRCSDK2.VRC_TriggerColliderEventTrigger>() != null || t.GetComponent<VRCSDK2.VRC_TimedEvents>() != null))
                {
                    EditorGUILayout.HelpBox("Do not use VRC_Trigger in combination with deprecated event components.", MessageType.Error);
                    showedOldWarning = true;
                }
                VRCSDK2.VRC_EventHandler handler = t.GetComponent<VRCSDK2.VRC_EventHandler>();
                if (handler != null)
                    handler.Events = new List<VRCSDK2.VRC_EventHandler.VrcEvent>();
            }
#pragma warning restore CS0618 // Type or member is obsolete

            triggersProperty = serializedObject.FindProperty("Triggers");
            proximityProperty = serializedObject.FindProperty("proximity");
            interactTextProperty = serializedObject.FindProperty("interactText");
            ownershipProperty = serializedObject.FindProperty("TakesOwnershipIfNecessary");
            drawLinesProperty = serializedObject.FindProperty("DrawLines");

            serializedObject.Update();

            SerializedProperty triggers = triggersProperty.Copy();
            int triggersLength = triggers.arraySize;

            if (eventLists.Length != triggersLength)
                eventLists = new ReorderableList[triggersLength];

            if (relayLists.Length != triggersLength)
                relayLists = new ReorderableList[triggersLength];

            if (objectLists.Length != triggersLength)
                objectLists = new ReorderableList[triggersLength];

            if (visible.Length != triggersLength)
            {
                bool[] newVisible = new bool[triggersLength];
                for (int idx = 0; idx < visible.Length && idx < newVisible.Length; ++idx)
                    newVisible[idx] = visible[idx];
                for (int idx = visible.Length; idx < newVisible.Length && idx < newVisible.Length; ++idx)
                    newVisible[idx] = true;
                visible = newVisible;
            }

            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - 30));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ownershipProperty, new GUIContent("Take Ownership of Action Targets"));
            VRCSDK2.VRC_Trigger.EditorGlobalTriggerLineMode = (VRCSDK2.VRC_Trigger.EditorTriggerLineMode)EditorPrefs.GetInt("VRCSDK2_triggerLineMode", 0);
            if (VRCSDK2.VRC_Trigger.EditorGlobalTriggerLineMode == VRCSDK2.VRC_Trigger.EditorTriggerLineMode.PerTrigger)
                EditorGUILayout.PropertyField(drawLinesProperty, new GUIContent("Draw Lines"));

            EditorGUILayout.Space();

            RenderTriggers();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void RenderHelpBox(string message, MessageType messageType)
        {
            if (VRCSettings.DisplayHelpBoxes || messageType == MessageType.Error || messageType == MessageType.Warning)
                EditorGUILayout.HelpBox(message, messageType);
        }

        private void RenderTriggers()
        {
            GUIStyle triggerStyle = new GUIStyle(EditorStyles.helpBox);

            SerializedProperty triggers = triggersProperty.Copy();
            int triggersLength = triggers.arraySize;

            List<int> to_remove = new List<int>();
            for (int idx = 0; idx < triggersLength; ++idx)
            {
                SerializedProperty triggerProperty = triggers.GetArrayElementAtIndex(idx);
                SerializedProperty broadcastProperty = triggerProperty.FindPropertyRelative("BroadcastType");

                EditorGUILayout.BeginVertical(triggerStyle);

                if (RenderTriggerHeader(triggerProperty, ref visible[idx]))
                {
                    to_remove.Add(idx);
                    EditorGUILayout.EndVertical();

                    continue;
                }

                if (!visible[idx])
                {
                    EditorGUILayout.EndVertical();
                    continue;
                }

                if (!unbufferedBroadcastTypes.Contains((VRCSDK2.VRC_EventHandler.VrcBroadcastType)broadcastProperty.intValue) &&
                    !bufferOneBroadcastTypes.Contains((VRCSDK2.VRC_EventHandler.VrcBroadcastType)broadcastProperty.intValue) &&
                    ActiveEvents(triggerProperty).Any(e => e == VRCSDK2.VRC_EventHandler.VrcEventType.SendRPC))
                    RenderHelpBox("Consider using unbuffered broadcasts with RPCs.", MessageType.Error);

                EditorGUILayout.Separator();

                RenderTriggerEditor(triggerProperty, idx);

                if (eventLists.Length == triggersLength)
                {
                    EditorGUILayout.Separator();

                    if (triggerProperty.FindPropertyRelative("TriggerType").intValue != (int)VRCSDK2.VRC_Trigger.TriggerType.Relay)
                    {
                        RenderTriggerEventsEditor(triggerProperty, idx);

                        EditorGUILayout.Separator();
                    }
                }
                EditorGUILayout.EndVertical();
            }

            foreach (int idx in ((IEnumerable<int>)to_remove).Reverse())
                triggersProperty.Copy().DeleteArrayElementAtIndex(idx);

            RenderAddTrigger();
        }

        private void RenderTriggerEditor(SerializedProperty triggerProperty, int idx)
        {
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("AfterSeconds"), new GUIContent("Delay in Seconds"));

            VRC_Trigger.TriggerType triggerType = (VRC_Trigger.TriggerType)triggerProperty.FindPropertyRelative("TriggerType").intValue;
            switch (triggerType)
            {
                case VRCSDK2.VRC_Trigger.TriggerType.Custom:
                    RenderCustom(triggerProperty);
                    break;
                case VRCSDK2.VRC_Trigger.TriggerType.Relay:
                    RenderRelay(triggerProperty, idx);
                    break;
                case VRCSDK2.VRC_Trigger.TriggerType.OnEnterTrigger:
                case VRCSDK2.VRC_Trigger.TriggerType.OnExitTrigger:
                case VRCSDK2.VRC_Trigger.TriggerType.OnEnterCollider:
                case VRCSDK2.VRC_Trigger.TriggerType.OnExitCollider:
                    RenderCollider(triggerProperty);
                    break;
                case VRCSDK2.VRC_Trigger.TriggerType.OnKeyDown:
                case VRCSDK2.VRC_Trigger.TriggerType.OnKeyUp:
                    RenderKey(triggerProperty);
                    break;
                case VRCSDK2.VRC_Trigger.TriggerType.OnTimer:
                    RenderTimer(triggerProperty);
                    break;
                case VRCSDK2.VRC_Trigger.TriggerType.OnDataStorageChange:
                    // case VRCSDK2.VRC_Trigger.TriggerType.OnDataStorageAdd:
                    // case VRCSDK2.VRC_Trigger.TriggerType.OnDataStorageRemove:
                    RenderDataStorage(triggerProperty);
                    break;
                case VRCSDK2.VRC_Trigger.TriggerType.OnParticleCollision:
                    //RenderHelpBox("Triggers for each particle in attached particle system that collides with something.", MessageType.Info); 
                    RenderCollider(triggerProperty);
                    break;
                default:
                    if (VRCSDK2.VRC_Trigger.TypeCollections.InteractiveTypes.Contains(triggerType) || VRCSDK2.VRC_Trigger.TypeCollections.PickupTypes.Contains(triggerType))
                        RenderInteractableEditor();
                    else
                        RenderEmpty(triggerProperty);
                    break;
            }
        }

        private List<VRCSDK2.VRC_EventHandler.VrcEventType> ActiveEvents(SerializedProperty triggerProperty)
        {
            List<VRCSDK2.VRC_EventHandler.VrcEventType> activeTypes = new List<VRCSDK2.VRC_EventHandler.VrcEventType>();

            SerializedProperty events = triggerProperty.FindPropertyRelative("Events").Copy();
            int eventsLength = events.arraySize;

            for (int idx = 0; idx < eventsLength; ++idx)
            {
                VRCSDK2.VRC_EventHandler.VrcEventType eventType = (VRCSDK2.VRC_EventHandler.VrcEventType)events.GetArrayElementAtIndex(idx).FindPropertyRelative("EventType").intValue;
                activeTypes.Add(eventType);
            }

            return activeTypes;
        }

        private void RenderAddTrigger()
        {
            Rect rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(15f));
            EditorGUILayout.Space();

            Rect selectedRect = new Rect(rect.x, rect.y, rect.width / 4 * 3 - 5, rect.height);
            Rect addRect = new Rect(selectedRect.x + selectedRect.width + 5, rect.y, rect.width / 4, rect.height);

            bool showStationTypes = serializedObject.targetObjects.Any(o => (o as VRCSDK2.VRC_Trigger).GetComponent<VRCSDK2.VRC_Station>() != null);
            System.Func<VRCSDK2.VRC_Trigger.TriggerType, bool> predicate =
                v => hiddenTriggerTypes.Contains(v) == false && (showStationTypes || (v != VRCSDK2.VRC_Trigger.TriggerType.OnStationEntered && v != VRCSDK2.VRC_Trigger.TriggerType.OnStationExited));

            addTriggerSelectedType = VRC_EditorTools.FilteredEnumPopup(selectedRect, addTriggerSelectedType, predicate);

            if (GUI.Button(addRect, "Add"))
            {
                SerializedProperty triggersAry = triggersProperty;

                // hacks
                triggersAry.Next(true);
                triggersAry.Next(true);

                int triggersLength = triggersAry.intValue;
                triggersAry.intValue = triggersLength + 1;
                triggersAry.Next(true);

                for (int idx = 0; idx < triggersLength; ++idx)
                    triggersAry.Next(false);

                triggersAry.FindPropertyRelative("TriggerType").intValue = (int)addTriggerSelectedType;
                triggersAry.FindPropertyRelative("BroadcastType").intValue = (int)VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysBufferOne;
                triggersAry.FindPropertyRelative("TriggerIndividuals").boolValue = true;
                triggersAry.FindPropertyRelative("Layers").intValue = LayerMask.GetMask("Default");
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool RenderTriggerHeader(SerializedProperty triggerProperty, ref bool expand)
        {
            bool delete = false;

            if (!delete)
            {
                VRCSDK2.VRC_EventHandler.VrcBroadcastType? broadcast = null;

                Rect rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(15f));
                EditorGUILayout.Space();

                int baseWidth = (int)((rect.width - 40) / 4);

                Rect foldoutRect = new Rect(rect.x + 10, rect.y, 20, rect.height);
                Rect typeRect = new Rect(rect.x + 20, rect.y, baseWidth, rect.height);
                Rect broadcastRect = new Rect(rect.x + 25 + baseWidth, rect.y, baseWidth, rect.height);
                Rect randomRect = new Rect(rect.x + 30 + baseWidth * 2, rect.y, baseWidth, rect.height);
                Rect removeRect = new Rect(rect.x + 35 + baseWidth * 3, rect.y, baseWidth, rect.height);

                expand = EditorGUI.Foldout(foldoutRect, expand, GUIContent.none);

                SerializedProperty triggerTypeProperty = triggerProperty.FindPropertyRelative("TriggerType");
                VRCSDK2.VRC_Trigger.TriggerType currentType = (VRCSDK2.VRC_Trigger.TriggerType)triggerTypeProperty.intValue;

                SerializedProperty nameProperty = triggerProperty.FindPropertyRelative("Name");
                if (string.IsNullOrEmpty(nameProperty.stringValue))
                    nameProperty.stringValue = "Unnamed";

                bool showStationTypes = serializedObject.targetObjects.Any(o => (o as VRCSDK2.VRC_Trigger).GetComponent<VRCSDK2.VRC_Station>() != null);
                System.Func<string, string> rename = s => s == "Custom" ? s + " (" + nameProperty.stringValue + ")" : s;
                System.Func<VRCSDK2.VRC_Trigger.TriggerType, bool> predicate =
                    v => hiddenTriggerTypes.Contains(v) == false && (showStationTypes || (v != VRCSDK2.VRC_Trigger.TriggerType.OnStationEntered && v != VRCSDK2.VRC_Trigger.TriggerType.OnStationExited));

                triggerTypeProperty.intValue = (int)VRC_EditorTools.FilteredEnumPopup(typeRect, currentType, predicate, rename);
                currentType = (VRCSDK2.VRC_Trigger.TriggerType)triggerTypeProperty.intValue;

                SerializedProperty broadcastTypeProperty = triggerProperty.FindPropertyRelative("BroadcastType");
                List<VRCSDK2.VRC_EventHandler.VrcEventType> activeEvents = ActiveEvents(triggerProperty);
                if ((VRCSDK2.VRC_Trigger.TriggerType)triggerTypeProperty.intValue == VRCSDK2.VRC_Trigger.TriggerType.Relay || activeEvents.Contains(VRCSDK2.VRC_EventHandler.VrcEventType.SpawnObject))
                {
                    broadcast = VRCSDK2.VRC_EventHandler.VrcBroadcastType.Always;
                    broadcastTypeProperty.intValue = (int)broadcast;
                }
                else
                {
                    VRC_EditorTools.FilteredEnumPopup<VRCSDK2.VRC_EventHandler.VrcBroadcastType>(broadcastRect, broadcastTypeProperty, b => !hiddenBroadcastTypes.Contains(b));
                    broadcast = (VRCSDK2.VRC_EventHandler.VrcBroadcastType)broadcastTypeProperty.intValue;
                }

                SerializedProperty probabilitiesProperty = triggerProperty.FindPropertyRelative("Probabilities");
                SerializedProperty probabilityLockProperty = triggerProperty.FindPropertyRelative("ProbabilityLock");
                SerializedProperty eventsProperty = triggerProperty.FindPropertyRelative("Events");

                if (triggerProperty.FindPropertyRelative("Events").arraySize < 1)
                    GUI.enabled = false;
                if (GUI.Toggle(randomRect, probabilitiesProperty.arraySize > 0, new GUIContent(" Randomize")))
                    probabilityLockProperty.arraySize = probabilitiesProperty.arraySize = eventsProperty.arraySize;
                else
                    probabilityLockProperty.arraySize = probabilitiesProperty.arraySize = 0;
                GUI.enabled = true;

                if (GUI.Button(removeRect, "Remove"))
                    delete = true;

                EditorGUILayout.EndHorizontal();

                if (broadcast.HasValue && expand)
                {
                    string message = null;
                    switch (broadcast.Value)
                    {
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.Always:
                            message = "All are able to activate the trigger for everyone, and late-joiners will also trigger it.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysUnbuffered:
                            message = "All are able to activate the trigger for everyone, but late-joiners will not trigger it.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.Local:
                            message = "All are able to activate the trigger for themselves only.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.Master:
                            message = "Only the Master is able to activate the trigger for everyone, and late-joiners will also trigger it.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.MasterUnbuffered:
                            message = "Only the Master is able to activate the trigger for everyone, but late-joiners will not trigger it.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.Owner:
                            message = "Only the Owner is able to activate the trigger for everyone, and late-joiners will also trigger it.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.OwnerUnbuffered:
                            message = "Only the Owner is able to activate the trigger for everyone, but late-joiners will not trigger it.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysBufferOne:
                            message = "All are able to activate the trigger for everyone, and late-joiners will trigger the most recent one.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.MasterBufferOne:
                            message = "Only the Master is able to activate the trigger for everyone, and late-joiners will trigger the most recent one.";
                            break;
                        case VRCSDK2.VRC_EventHandler.VrcBroadcastType.OwnerBufferOne:
                            message = "Only the Owner is able to activate the trigger for everyone, and late-joiners will trigger the most recent one.";
                            break;
                    }
                    if (message != null)
                        RenderHelpBox(message, MessageType.Info);
                }
            }

            return delete;
        }

        private void RenderInteractableEditor()
        {
            EditorGUILayout.PropertyField(interactTextProperty, new GUIContent("Interaction Text"));
            proximityProperty.floatValue = EditorGUILayout.Slider("Proximity", proximityProperty.floatValue, 0f, 100f);
        }

        private void RenderTriggerEventsEditor(SerializedProperty triggerProperty, int idx)
        {
            if (eventLists[idx] == null)
            {
                ReorderableList newList = new ReorderableList(serializedObject, triggerProperty.FindPropertyRelative("Events"), true, true, true, true);
                newList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Actions");
                newList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty eventsListProperty = triggerProperty.FindPropertyRelative("Events");
                    SerializedProperty probabilitiesProperty = triggerProperty.FindPropertyRelative("Probabilities");
                    SerializedProperty probabilityLockProperty = triggerProperty.FindPropertyRelative("ProbabilityLock");
                    SerializedProperty shadowListProperty = triggerProperty.FindPropertyRelative("DataStorageShadowValues");

                    if (shadowListProperty != null && shadowListProperty.arraySize != eventsListProperty.arraySize)
                        shadowListProperty.arraySize = eventsListProperty.arraySize;

                    SerializedProperty shadowProperty = shadowListProperty == null ? null : shadowListProperty.GetArrayElementAtIndex(index);
                    SerializedProperty eventProperty = eventsListProperty.GetArrayElementAtIndex(index);
                    SerializedProperty eventTypeProperty = eventProperty.FindPropertyRelative("EventType");
                    SerializedProperty parameterStringProperty = eventProperty.FindPropertyRelative("ParameterString");

                    string label = ((VRCSDK2.VRC_EventHandler.VrcEventType)eventTypeProperty.intValue).ToString();
                    if (!string.IsNullOrEmpty(parameterStringProperty.stringValue))
                        label += " (" + parameterStringProperty.stringValue + ")";

                    if (probabilitiesProperty.arraySize == 0)
                        EditorGUI.LabelField(rect, label);
                    else
                    {
                        Rect labelRect = new Rect(rect.x, rect.y, rect.width / 2, rect.height);
                        Rect sliderLockRect = new Rect(rect.x + rect.width / 2, rect.y, 30, rect.height);
                        Rect sliderRect = new Rect(rect.x + rect.width / 2 + 30, rect.y, rect.width / 2 - 30, rect.height);

                        EditorGUI.LabelField(labelRect, label);

                        probabilityLockProperty.GetArrayElementAtIndex(index).boolValue = GUI.Toggle(sliderLockRect, probabilityLockProperty.GetArrayElementAtIndex(index).boolValue, new GUIContent());
                        probabilitiesProperty.GetArrayElementAtIndex(index).floatValue = EditorGUI.Slider(sliderRect, new GUIContent(), probabilitiesProperty.GetArrayElementAtIndex(index).floatValue, 0f, 1f);

                        bool allLocked = true;
                        for (int pIdx = 0; pIdx < probabilitiesProperty.arraySize; ++pIdx)
                            allLocked = allLocked && probabilityLockProperty.GetArrayElementAtIndex(pIdx).boolValue;
                        if (allLocked)
                            for (int pIdx = 0; pIdx < probabilitiesProperty.arraySize; ++pIdx)
                                if (pIdx != index)
                                    probabilitiesProperty.GetArrayElementAtIndex(pIdx).floatValue = probabilitiesProperty.GetArrayElementAtIndex(index).floatValue;

                        // Squish 'em down
                        float probabilitySum = 1f;
                        const int MAX_SCALE_PROBABILITIES_LOOP_ITERATIONS = 8;
                        const int PROBABILITY_VALUE_DECIMAL_PLACES = 3;
                        int loopIterations = 0;
                        do
                        {
                            if (probabilitySum > 1f)
                            {
                                float fixRatio = 1f / probabilitySum;
                                int countChanged = 0;
                                for (int pIdx = 0; pIdx < probabilitiesProperty.arraySize; ++pIdx)
                                {
                                    if (allLocked)
                                    {
                                        countChanged++;
                                        probabilitiesProperty.GetArrayElementAtIndex(pIdx).floatValue *= fixRatio;
                                    }
                                    else
                                    {
                                        if (pIdx == index || probabilityLockProperty.GetArrayElementAtIndex(pIdx).boolValue || probabilitiesProperty.GetArrayElementAtIndex(pIdx).floatValue == 0f)
                                            continue;
                                        countChanged++;
                                        probabilitiesProperty.GetArrayElementAtIndex(pIdx).floatValue *= fixRatio;
                                    }
                                }
                                if (countChanged == 0)
                                    probabilitiesProperty.GetArrayElementAtIndex(index).floatValue -= probabilitySum - 1f;
                                probabilitiesProperty.GetArrayElementAtIndex(index).floatValue = (float)Math.Round(probabilitiesProperty.GetArrayElementAtIndex(index).floatValue, PROBABILITY_VALUE_DECIMAL_PLACES);
                            }
                            probabilitySum = 0f;
                            for (int pIdx = 0; pIdx < probabilitiesProperty.arraySize; ++pIdx)
                                probabilitySum += probabilitiesProperty.GetArrayElementAtIndex(pIdx).floatValue;
                            loopIterations++;
                        } while ((probabilitySum > 1f) && (loopIterations < MAX_SCALE_PROBABILITIES_LOOP_ITERATIONS));
                    }

                    if (isFocused)
                        objectLists[idx] = null;
                    if (isActive)
                    {
                        EditorGUILayout.Space();

                        RenderEventEditor(shadowProperty, triggerProperty, eventProperty, idx);
                    }
                };
                newList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    SerializedProperty eventsList = triggerProperty.FindPropertyRelative("Events");
                    foreach (VRCSDK2.VRC_EventHandler.VrcEventType type in System.Enum.GetValues(typeof(VRCSDK2.VRC_EventHandler.VrcEventType)).Cast<VRCSDK2.VRC_EventHandler.VrcEventType>().Where(v => !hiddenEventTypes.Contains(v)).OrderBy(et => System.Enum.GetName(typeof(VRCSDK2.VRC_EventHandler.VrcEventType), et)))
                    {
                        menu.AddItem(new GUIContent("Basic Events/" + type.ToString()), false, (t) =>
                        {
                            eventsList.arraySize++;

                            SerializedProperty newEventProperty = eventsList.GetArrayElementAtIndex(eventsList.arraySize - 1);
                            newEventProperty.FindPropertyRelative("EventType").intValue = (int)(VRCSDK2.VRC_EventHandler.VrcEventType)t;
                            newEventProperty.FindPropertyRelative("ParameterObjects").arraySize = 0;
                            newEventProperty.FindPropertyRelative("ParameterInt").intValue = 0;
                            newEventProperty.FindPropertyRelative("ParameterFloat").floatValue = 0f;
                            newEventProperty.FindPropertyRelative("ParameterString").stringValue = null;

                            serializedObject.ApplyModifiedProperties();
                        }, type);
                    }
                    VRC.SDKBase.IVRCEventProvider[] providers = FindObjectsOfType<MonoBehaviour>().Where(b => b is VRC.SDKBase.IVRCEventProvider).Cast<VRC.SDKBase.IVRCEventProvider>().ToArray();
                    foreach (VRC.SDKBase.IVRCEventProvider provider in providers)
                    {
                        foreach (VRCSDK2.VRC_EventHandler.VrcEvent evt in provider.ProvideEvents())
                        {
                            string name = "Events from Scene/" + (provider as MonoBehaviour).name + "/" + evt.Name;
                            menu.AddItem(new GUIContent(name), false, (t) =>
                            {
                                eventsList.arraySize++;

                                VRCSDK2.VRC_EventHandler.VrcEvent e = (VRCSDK2.VRC_EventHandler.VrcEvent)t;

                                SerializedProperty newEventProperty = eventsList.GetArrayElementAtIndex(eventsList.arraySize - 1);
                                newEventProperty.FindPropertyRelative("Name").stringValue = e.Name;
                                newEventProperty.FindPropertyRelative("EventType").intValue = (int)e.EventType;
                                newEventProperty.FindPropertyRelative("ParameterInt").intValue = e.ParameterInt;
                                newEventProperty.FindPropertyRelative("ParameterFloat").floatValue = e.ParameterFloat;
                                newEventProperty.FindPropertyRelative("ParameterString").stringValue = e.ParameterString;
                                newEventProperty.FindPropertyRelative("ParameterObjects").arraySize = e.ParameterObjects.Length;
                                for (int obj_idx = 0; obj_idx < e.ParameterObjects.Length; ++obj_idx)
                                    newEventProperty.FindPropertyRelative("ParameterObjects").GetArrayElementAtIndex(obj_idx).objectReferenceValue = e.ParameterObjects[obj_idx];

#pragma warning disable CS0618 // Type or member is obsolete
                                newEventProperty.FindPropertyRelative("ParameterObject").objectReferenceValue = e.ParameterObject;
#pragma warning restore CS0618 // Type or member is obsolete

                                serializedObject.ApplyModifiedProperties();
                            }, evt);
                        }
                    }
                    menu.ShowAsContext();

                    eventLists = new ReorderableList[0];
                    objectLists = new ReorderableList[0];
                    relayLists = new ReorderableList[0];
                };

                eventLists[idx] = newList;
            }

            ReorderableList eventList = eventLists[idx];
            eventList.DoLayoutList();
        }

        private void RenderDataStorage(SerializedProperty triggerProperty)
        {
            if (triggerProperty.serializedObject.targetObjects.Any(obj => (obj as VRCSDK2.VRC_Trigger).gameObject.GetComponent<VRCSDK2.VRC_DataStorage>() == null))
                RenderHelpBox("Data Storage Triggers require a VRC_DataStorage Component.", MessageType.Warning);
            else
            {
                SerializedProperty idxProperty = triggerProperty.FindPropertyRelative("DataElementIdx");
                VRCSDK2.VRC_DataStorage ds = (target as VRCSDK2.VRC_Trigger).gameObject.GetComponent<VRCSDK2.VRC_DataStorage>();

                if (ds.data == null)
                {
                    ds.data = new VRCSDK2.VRC_DataStorage.VrcDataElement[0];
                    idxProperty.intValue = -1;
                }

                List<string> names = new List<string>();
                names.Add("Any Data Element");
                foreach (VRCSDK2.VRC_DataStorage.VrcDataElement element in ds.data)
                    names.Add(element.name);

                int selectedIdx = idxProperty.intValue;
                if (selectedIdx == -1)
                    selectedIdx = 0;
                else
                    selectedIdx += 1;

                selectedIdx = EditorGUILayout.Popup("Data Element", selectedIdx, names.ToArray());

                if (selectedIdx == 0)
                    idxProperty.intValue = -1;
                else
                    idxProperty.intValue = selectedIdx - 1;
            }
        }

        private void RenderKey(SerializedProperty triggerProperty)
        {
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("Key"));
        }

        private void RenderCollider(SerializedProperty triggerProperty)
        {
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("TriggerIndividuals"));
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("Layers"));
        }

        private void RenderTimer(SerializedProperty triggerProperty)
        {
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("Repeat"));
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("ResetOnEnable"));
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("LowPeriodTime"));
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("HighPeriodTime"));
        }

        private void RenderCustom(SerializedProperty triggerProperty)
        {
            SerializedProperty nameProperty = triggerProperty.FindPropertyRelative("Name");
            EditorGUILayout.PropertyField(triggerProperty.FindPropertyRelative("Name"));

            if (string.IsNullOrEmpty(nameProperty.stringValue))
                nameProperty.stringValue = "Unnamed";
        }

        private void RenderRelay(SerializedProperty triggerProperty, int idx)
        {
            if (relayLists[idx] == null)
            {
                ReorderableList newList = new ReorderableList(serializedObject, triggerProperty.FindPropertyRelative("Others"), true, true, true, true);
                newList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, new GUIContent("Targets"));
                newList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty target = newList.serializedProperty.GetArrayElementAtIndex(index);

                    EditorGUI.PropertyField(rect, target, GUIContent.none);

                    target.serializedObject.ApplyModifiedProperties();
                };
                relayLists[idx] = newList;
            }
            relayLists[idx].DoLayoutList();
        }

        private void RenderEmpty(SerializedProperty triggerProperty)
        {
        }

        private bool doesPropertyContainAnyNullReceivers(SerializedProperty objectsProperty)
        {
            bool containsNullReceivers = false;
            if (objectsProperty.arraySize > 0)
            {
                for (int i = 0; i < objectsProperty.arraySize; i++)
                {
                    SerializedProperty elem = objectsProperty.GetArrayElementAtIndex(i);
                    if (elem.objectReferenceValue == null) containsNullReceivers = true;
                }
            }
            return containsNullReceivers;
        }

        private void RenderTargetGameObjectList(SerializedProperty objectsProperty, int idx, bool receiverRequired = true)
        {
            if (objectLists[idx] == null)
            {
                objectLists[idx] = new ReorderableList(objectsProperty.serializedObject, objectsProperty, true, true, true, true);
                objectLists[idx].drawHeaderCallback = (Rect rect) =>
                {
                    string infoString = "Receivers";
                    if (objectsProperty.arraySize == 0)
                        infoString = "Receivers: This GameObject";
                    EditorGUI.LabelField(rect, new GUIContent(infoString));

                    Event evt = Event.current;
                    if (!rect.Contains(evt.mousePosition))
                        return;

                    if (evt.type == EventType.DragUpdated)
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        GameObject[] dragObjects = DragAndDrop.objectReferences.OfType<GameObject>().ToArray();
                        int startIndex = objectsProperty.arraySize;
                        objectsProperty.arraySize = objectsProperty.arraySize + dragObjects.Length;

                        for (int i = 0; i < dragObjects.Length; i++)
                        {
                            SerializedProperty newElem = objectsProperty.GetArrayElementAtIndex(startIndex + i);
                            newElem.objectReferenceValue = dragObjects[i];
                        }

                        DragAndDrop.AcceptDrag();
                        evt.Use();
                    }
                };
                objectLists[idx].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty target = objectLists[idx].serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, target, GUIContent.none);
                    target.serializedObject.ApplyModifiedProperties();
                };
            }
            objectLists[idx].DoLayoutList();
            if (objectsProperty.arraySize == 0)
                RenderHelpBox("This trigger will target the GameObject it's on, because the receivers list is empty.", MessageType.Error);
            else if (receiverRequired && doesPropertyContainAnyNullReceivers(objectsProperty))
                RenderHelpBox("Trigger with no object set will be ignored!", MessageType.Info);
        }

        private void RenderTargetComponentList<T>(SerializedProperty objectsProperty, int idx, string label = "Receivers") where T : Component
        {
            if (objectLists[idx] == null)
            {
                objectLists[idx] = new ReorderableList(objectsProperty.serializedObject, objectsProperty, true, true, true, true);
                objectLists[idx].drawHeaderCallback = (Rect rect) =>
                {
                    string infoString = label;
                    if (objectsProperty.arraySize == 0)
                        infoString = label + ": This " + typeof(T).Name;
                    EditorGUI.LabelField(rect, new GUIContent(infoString));

                    Event evt = Event.current;
                    if (!rect.Contains(evt.mousePosition))
                        return;

                    if (evt.type == EventType.DragUpdated)
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        GameObject[] dragObjects = DragAndDrop.objectReferences.OfType<GameObject>().ToArray();
                        int startIndex = objectsProperty.arraySize;
                        objectsProperty.arraySize = objectsProperty.arraySize + dragObjects.Length;

                        for (int i = 0; i < dragObjects.Length; i++)
                        {
                            SerializedProperty newElem = objectsProperty.GetArrayElementAtIndex(startIndex + i);
                            newElem.objectReferenceValue = dragObjects[i];
                        }

                        DragAndDrop.AcceptDrag();
                        evt.Use();
                    }
                };
                objectLists[idx].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty target = objectLists[idx].serializedProperty.GetArrayElementAtIndex(index);

                    T current = null;
                    if (target.objectReferenceValue != null)
                        current = (target.objectReferenceValue as GameObject).GetComponent<T>();

                    current = EditorGUI.ObjectField(rect, current, typeof(T), true) as T;
                    target.objectReferenceValue = current == null ? null : current.gameObject;
                    target.serializedObject.ApplyModifiedProperties();
                };
            }
            objectLists[idx].DoLayoutList();
            if (objectsProperty.arraySize == 0)
                RenderHelpBox("This trigger will target the GameObject it's on, because the receivers list is empty.", MessageType.Error);
            else if (doesPropertyContainAnyNullReceivers(objectsProperty))
                RenderHelpBox("Trigger with no object set will be ignored!", MessageType.Info);
        }

        public void RenderEventEditor(SerializedProperty shadowProperty, SerializedProperty triggerProperty, SerializedProperty eventProperty, int triggerIdx)
        {
            SerializedProperty eventTypeProperty = eventProperty.FindPropertyRelative("EventType");
            SerializedProperty parameterObjectProperty = eventProperty.FindPropertyRelative("ParameterObject");
            SerializedProperty parameterObjectsProperty = eventProperty.FindPropertyRelative("ParameterObjects");
            SerializedProperty parameterStringProperty = eventProperty.FindPropertyRelative("ParameterString");
            SerializedProperty parameterBoolOpProperty = eventProperty.FindPropertyRelative("ParameterBoolOp");
            SerializedProperty parameterFloatProperty = eventProperty.FindPropertyRelative("ParameterFloat");
            SerializedProperty parameterIntProperty = eventProperty.FindPropertyRelative("ParameterInt");
            SerializedProperty parameterBytesProperty = eventProperty.FindPropertyRelative("ParameterBytes");

            if (parameterObjectProperty.objectReferenceValue != null && parameterObjectsProperty.arraySize == 0)
            {
                parameterObjectsProperty.arraySize = 1;
                parameterObjectsProperty.GetArrayElementAtIndex(0).objectReferenceValue = parameterObjectProperty.objectReferenceValue;
            }
            parameterObjectProperty.objectReferenceValue = null;

            switch ((VRCSDK2.VRC_EventHandler.VrcEventType)eventTypeProperty.intValue)
            {
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationBool:
                    {
                        RenderTargetComponentList<Animator>(parameterObjectsProperty, triggerIdx);

                        RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("Variable"));
                        RenderPropertyEditor(shadowProperty, parameterBoolOpProperty, new GUIContent("Operation"), true);
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationFloat:
                    {
                        RenderTargetComponentList<Animator>(parameterObjectsProperty, triggerIdx);

                        RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("Variable"));
                        RenderPropertyEditor(shadowProperty, parameterFloatProperty, new GUIContent("Value"));
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationInt:
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationIntAdd:
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationIntDivide:
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationIntMultiply:
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationIntSubtract:
                    {
                        RenderTargetComponentList<Animator>(parameterObjectsProperty, triggerIdx);

                        RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("Variable"));
                        RenderPropertyEditor(shadowProperty, parameterIntProperty, new GUIContent("Value"));
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationTrigger:
                    {
                        RenderTargetComponentList<Animator>(parameterObjectsProperty, triggerIdx);

                        RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("Trigger"));
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.PlayAnimation:
                    {
                        RenderTargetComponentList<Animation>(parameterObjectsProperty, triggerIdx);

                        RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("Clip"));

                        for (int idx = 0; idx < parameterObjectsProperty.arraySize; ++idx)
                        {
                            GameObject obj = parameterObjectsProperty.GetArrayElementAtIndex(idx).objectReferenceValue as GameObject;
                            Animation anim = obj == null ? null : obj.GetComponent<Animation>();
                            if (anim != null && anim.GetClip(parameterStringProperty.stringValue) == null)
                            {
                                RenderHelpBox("Could not locate " + parameterStringProperty.stringValue + " on " + obj.name + "; is it legacy?", MessageType.Error);
                                break;
                            }
                        }
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.AudioTrigger:
                    {
                        RenderTargetComponentList<AudioSource>(parameterObjectsProperty, triggerIdx);

                        List<string> clipNames = new List<string>();
                        if (parameterObjectsProperty.arraySize > 0)
                        {
                            int idx = 0;
                            for (; idx < parameterObjectsProperty.arraySize; ++idx)
                            {
                                SerializedProperty prop = parameterObjectsProperty.GetArrayElementAtIndex(0);
                                GameObject obj = prop.objectReferenceValue != null ? prop.objectReferenceValue as GameObject : null;
                                if (obj != null)
                                {
                                    foreach (AudioSource source in obj.GetComponents<AudioSource>())
                                        if (source.clip != null && !string.IsNullOrEmpty(source.clip.name))
                                            clipNames.Add(source.clip.name);
                                    break;
                                }
                            }
                            for (; idx < parameterObjectsProperty.arraySize; ++idx)
                            {
                                SerializedProperty prop = parameterObjectsProperty.GetArrayElementAtIndex(0);
                                GameObject obj = prop.objectReferenceValue != null ? prop.objectReferenceValue as GameObject : null;
                                if (obj != null)
                                {
                                    List<string> thisClipNames = new List<string>();
                                    foreach (AudioSource source in obj.GetComponents<AudioSource>())
                                        if (source.clip != null && !string.IsNullOrEmpty(source.clip.name))
                                            thisClipNames.Add(source.clip.name);
                                    clipNames.RemoveAll(s => thisClipNames.Contains(s) == false);
                                }
                            }
                        }

                        clipNames.Insert(0, "");
                        int selectedIdx;
                        for (selectedIdx = clipNames.Count - 1; selectedIdx > 0; --selectedIdx)
                            if (parameterStringProperty.stringValue == clipNames[selectedIdx])
                                break;

                        parameterStringProperty.stringValue = clipNames[EditorGUILayout.Popup("Clip", selectedIdx, clipNames.ToArray())];
                        break;
                    }
#pragma warning disable CS0618 // Type or member is obsolete
                case VRCSDK2.VRC_EventHandler.VrcEventType.MeshVisibility:
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        RenderTargetComponentList<Renderer>(parameterObjectsProperty, triggerIdx);
                        RenderPropertyEditor(shadowProperty, parameterBoolOpProperty, new GUIContent("Operation"), true);
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.RunConsoleCommand:
                    {
                        RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("Command"));
                        break;
                    }
#pragma warning disable CS0618 // Type or member is obsolete
                case VRCSDK2.VRC_EventHandler.VrcEventType.SendMessage:
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);
                        if (parameterObjectsProperty.arraySize > 0)
                            RenderMethodSelector(eventProperty);
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetGameObjectActive:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);
                        RenderPropertyEditor(shadowProperty, parameterBoolOpProperty, new GUIContent("Operation"), true);
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetParticlePlaying:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);
                        RenderPropertyEditor(shadowProperty, parameterBoolOpProperty, new GUIContent("Operation"), true);
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.TeleportPlayer:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);
                        RenderPropertyEditor(shadowProperty, parameterBoolOpProperty, new GUIContent("Align Room To Destination"), true);
                        parameterIntProperty.intValue = EditorGUILayout.Toggle("Lerp On Remote Clients", (parameterIntProperty.intValue != 0)) ? 1 : 0;
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetWebPanelURI:
                    {
                        RenderTargetComponentList<VRCSDK2.VRC_WebPanel>(parameterObjectsProperty, triggerIdx);
                        RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("URI"));
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetWebPanelVolume:
                    {
                        RenderTargetComponentList<VRCSDK2.VRC_WebPanel>(parameterObjectsProperty, triggerIdx);
                        parameterFloatProperty.floatValue = EditorGUILayout.Slider("Volume", parameterFloatProperty.floatValue, 0f, 1f);
                        break;
                    }
                case VRCSDK2.VRC_EventHandler.VrcEventType.SendRPC:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);

                        if (parameterObjectsProperty.arraySize > 0)
                        {
                            RenderMethodSelector(eventProperty);
                            RenderRPCParameterEditor(eventProperty);
                        }
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.ActivateCustomTrigger:
                    {
                        RenderTargetComponentList<VRCSDK2.VRC_Trigger>(parameterObjectsProperty, triggerIdx);

                        VRC_EditorTools.CustomTriggerPopup("Name", parameterObjectsProperty, parameterStringProperty);
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.SpawnObject:
                    {
                        VRCSDK2.VRC_SceneDescriptor scene = FindObjectOfType<VRCSDK2.VRC_SceneDescriptor>();

                        string path = parameterStringProperty.stringValue;
                        GameObject found = scene != null ? scene.DynamicPrefabs.FirstOrDefault(p => AssetDatabase.GetAssetOrScenePath(p) == path) : null;
                        found = found == null ? AssetDatabase.LoadAssetAtPath<GameObject>(path) : found;
                        GameObject newFound = EditorGUILayout.ObjectField("Prefab", found, typeof(GameObject), false) as GameObject;
                        parameterStringProperty.stringValue = newFound == null ? null : AssetDatabase.GetAssetOrScenePath(newFound);

                        RenderTargetComponentList<Transform>(parameterObjectsProperty, triggerIdx, "Locations");
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.DestroyObject:
                    {
                        SerializedProperty broadcastTypeProperty = triggerProperty.FindPropertyRelative("BroadcastType");
                        VRCSDK2.VRC_EventHandler.VrcBroadcastType broadcast = (VRCSDK2.VRC_EventHandler.VrcBroadcastType)broadcastTypeProperty.intValue;
                        if (broadcast != VRCSDK2.VRC_EventHandler.VrcBroadcastType.Always && broadcast != VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysUnbuffered && broadcast != VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysBufferOne)
                            RenderHelpBox("Not all clients will destroy the object.", MessageType.Warning);

                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetLayer:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);
                        parameterIntProperty.intValue = (int)EditorGUILayout.LayerField("Layer", parameterIntProperty.intValue);
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetMaterial:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);

                        VRCSDK2.VRC_SceneDescriptor scene = FindObjectOfType<VRCSDK2.VRC_SceneDescriptor>();

                        string path = parameterStringProperty.stringValue;
                        Material found = scene != null ? scene.DynamicMaterials.FirstOrDefault(p => AssetDatabase.GetAssetOrScenePath(p) == path) : null;
                        found = found == null ? AssetDatabase.LoadAssetAtPath<Material>(path) : found;
                        Material newFound = EditorGUILayout.ObjectField("Material", found, typeof(Material), false) as Material;
                        parameterStringProperty.stringValue = newFound == null ? null : AssetDatabase.GetAssetOrScenePath(newFound);

                        if (scene != null && found != newFound)
                        {
                            scene.DynamicMaterials.Add(newFound);
                            scene.DynamicMaterials.Remove(found);
                        }
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.AddDamage:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx, false);
                        RenderPropertyEditor(shadowProperty, parameterFloatProperty, new GUIContent("Damage"));
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.AddHealth:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx, false);
                        RenderPropertyEditor(shadowProperty, parameterFloatProperty, new GUIContent("Health"));
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetComponentActive:
                    {
                        RenderTargetGameObjectList(parameterObjectsProperty, triggerIdx);
                        if (RenderTargetComponentEditor(parameterStringProperty, parameterObjectsProperty, triggerIdx))
                            RenderPropertyEditor(shadowProperty, parameterBoolOpProperty, new GUIContent("Enable"), true);
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.AddVelocity:
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetVelocity:
                    {
                        RenderTargetComponentList<Rigidbody>(parameterObjectsProperty, triggerIdx);
                        RenderVector3andSpacePropertyEditor(parameterBytesProperty, new GUIContent("Velocity"));
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.AddAngularVelocity:
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetAngularVelocity:
                    {
                        RenderTargetComponentList<Rigidbody>(parameterObjectsProperty, triggerIdx);
                        RenderVector3andSpacePropertyEditor(parameterBytesProperty, new GUIContent("Angular Velocity"));
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.AddForce:
                    {
                        RenderTargetComponentList<Rigidbody>(parameterObjectsProperty, triggerIdx);
                        RenderVector3andSpacePropertyEditor(parameterBytesProperty, new GUIContent("Force"));
                    }
                    break;
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetUIText:
                    {
                        RenderTargetComponentList<Text>(parameterObjectsProperty, triggerIdx);
                        RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("Text"));
                    }
                    break;

#if UDON
                case VRCSDK2.VRC_EventHandler.VrcEventType.CallUdonMethod:
                    //{
                    //    RenderTargetComponentList<VRC.Udon.UdonBehaviour>(parameterObjectsProperty, triggerIdx);
                    //    RenderPropertyEditor(shadowProperty, parameterStringProperty, new GUIContent("Method Name"));
                    //}
                    break;
#endif
                default:
                    RenderHelpBox("Unsupported event type", MessageType.Error);
                    break;
            }
        }

        private void RenderVector3andSpacePropertyEditor(SerializedProperty propertyBytes, GUIContent label)
        {
            object[] parameters = null;
            parameters = VRC.SDKBase.VRC_Serialization.ParameterDecoder(VRC_EditorTools.ReadBytesFromProperty(propertyBytes));
            if (parameters.Length == 0)
                parameters = new object[1] { new Vector4() };

            EditorGUI.BeginChangeCheck();
            Vector3 vec3Field = EditorGUILayout.Vector3Field(label, new Vector3(((Vector4)parameters[0]).x, ((Vector4)parameters[0]).y, ((Vector4)parameters[0]).z));
            bool spaceField = EditorGUILayout.Toggle("Use World Space", ((Vector4)parameters[0]).w > .5f ? true : false);
            parameters[0] = new Vector4(vec3Field.x, vec3Field.y, vec3Field.z, Convert.ToSingle(spaceField));
            if (EditorGUI.EndChangeCheck())
            {
                VRC_EditorTools.WriteBytesToProperty(propertyBytes, VRC.SDKBase.VRC_Serialization.ParameterEncoder(parameters));
            }
        }

        private void RenderVector3PropertyEditor(SerializedProperty propertyBytes, GUIContent label)
        {
            object[] parameters = null;
            parameters = VRC.SDKBase.VRC_Serialization.ParameterDecoder(VRC_EditorTools.ReadBytesFromProperty(propertyBytes));
            if (parameters.Length == 0)
                parameters = new object[1] { new Vector3() };

            EditorGUI.BeginChangeCheck();
            parameters[0] = EditorGUILayout.Vector3Field(label, (Vector3)parameters[0]);

            if (EditorGUI.EndChangeCheck())
            {
                VRC_EditorTools.WriteBytesToProperty(propertyBytes, VRC.SDKBase.VRC_Serialization.ParameterEncoder(parameters));
            }
        }

        bool RenderTargetComponentEditor(SerializedProperty componentNameProperty, SerializedProperty objectsProperty, int triggerIdx)
        {
            if (!objectsProperty.isArray || objectsProperty.arraySize == 0)
                return false;

            HashSet<Type> behaviours = new HashSet<Type>();
            bool isFirst = true;
            for (int objIdx = 0; objIdx < objectsProperty.arraySize; ++objIdx)
            {
                if (objectsProperty.GetArrayElementAtIndex(objIdx) == null || objectsProperty.GetArrayElementAtIndex(objIdx).objectReferenceValue == null || !(objectsProperty.GetArrayElementAtIndex(objIdx).objectReferenceValue is GameObject))
                    continue;

                if (isFirst)
                {
                    foreach (Component component in (objectsProperty.GetArrayElementAtIndex(0).objectReferenceValue as GameObject).GetComponents(typeof(Component)))
                    {
                        Type t = component.GetType();
                        if (t.GetProperty("enabled") != null)
                            behaviours.Add(component.GetType());
                    }
                    isFirst = false;
                }
                else
                {
                    HashSet<Type> thisBehaviours = new HashSet<Type>();
                    foreach (Component component in (objectsProperty.GetArrayElementAtIndex(objIdx).objectReferenceValue as GameObject).GetComponents(typeof(Component)))
                    {
                        Type t = component.GetType();
                        if (t.GetProperty("enabled") != null)
                            thisBehaviours.Add(component.GetType());
                    }
                    behaviours.RemoveWhere(s => thisBehaviours.Contains(s) == false);
                }
            }

            if (behaviours.Count == 0)
                return false;

            Type[] types = behaviours.ToArray();
            string[] options = behaviours.Select(t => t.FullName).ToArray();
            int selectedIdx = 0;
            for (int typeIdx = 0; typeIdx < types.Length; ++typeIdx)
                if (types[typeIdx].FullName == componentNameProperty.stringValue)
                {
                    selectedIdx = typeIdx;
                    break;
                }

            selectedIdx = EditorGUILayout.Popup("Component", selectedIdx, options);
            componentNameProperty.stringValue = types[selectedIdx].FullName;

            return true;
        }

        void RenderRPCParameterEditor(SerializedProperty eventProperty)
        {
            EditorGUI.BeginChangeCheck();

            SerializedProperty parameterIntProperty = eventProperty.FindPropertyRelative("ParameterInt");
            SerializedProperty parameterObjectsProperty = eventProperty.FindPropertyRelative("ParameterObjects");
            SerializedProperty parameterStringProperty = eventProperty.FindPropertyRelative("ParameterString");
            SerializedProperty parameterBytesProperty = eventProperty.FindPropertyRelative("ParameterBytes");

            if (parameterIntProperty.intValue == -1)
                parameterIntProperty.intValue = (int)(VRCSDK2.VRC_EventHandler.VrcTargetType.All);

            parameterIntProperty.intValue = (int)VRC_EditorTools.FilteredEnumPopup<VRCSDK2.VRC_EventHandler.VrcTargetType>("Targets", (VRCSDK2.VRC_EventHandler.VrcTargetType)parameterIntProperty.intValue, e => e != VRCSDK2.VRC_EventHandler.VrcTargetType.TargetPlayer);

            string message = null;
            switch ((VRCSDK2.VRC_EventHandler.VrcTargetType)parameterIntProperty.intValue)
            {
                case VRCSDK2.VRC_EventHandler.VrcTargetType.All:
                    message = "Will execute on all clients, except for those that join later.";
                    break;
                case VRCSDK2.VRC_EventHandler.VrcTargetType.AllBuffered:
                    message = "Will execute on all clients, including those that join later.";
                    break;
                case VRCSDK2.VRC_EventHandler.VrcTargetType.Local:
                    message = "Will execute for the instigator only.";
                    break;
                case VRCSDK2.VRC_EventHandler.VrcTargetType.Master:
                    message = "Will execute on the Master.";
                    break;
                case VRCSDK2.VRC_EventHandler.VrcTargetType.Others:
                    message = "Will execute for others but not locally, except for those that join later.";
                    break;
                case VRCSDK2.VRC_EventHandler.VrcTargetType.OthersBuffered:
                    message = "Will execute for others but not locally, including those that join later.";
                    break;
                case VRCSDK2.VRC_EventHandler.VrcTargetType.Owner:
                    message = "Will execute on the Owner.";
                    break;
                case VRCSDK2.VRC_EventHandler.VrcTargetType.AllBufferOne:
                    message = "Will execute on all clients, and only the most recent is executed for those that join later.";
                    break;
                case VRCSDK2.VRC_EventHandler.VrcTargetType.OthersBufferOne:
                    message = "Will execute for others but not locally, and only the most recent is executed for those that join later.";
                    break;
            }
            if (message != null)
                RenderHelpBox(message, MessageType.Info);

            Dictionary<string, List<MethodInfo>> methods = VRC_EditorTools.GetSharedAccessibleMethodsOnGameObjects(parameterObjectsProperty);
            if (methods.Count == 0)
            {
                RenderHelpBox("No applicable methods found.", MessageType.Error);
                return;
            }

            List<MethodInfo> methodInfos = methods.Values.Aggregate(new List<MethodInfo>(), (acc, lst) => { acc.AddRange(lst); return acc; });

            MethodInfo info = methodInfos.FirstOrDefault(m => m.Name == parameterStringProperty.stringValue);
            if (info == null)
                return;

            ParameterInfo[] paramInfo = info.GetParameters();

            // Editor-only
            foreach (var p in paramInfo)
                if (p.ParameterType.Namespace.StartsWith("VRCSDK2"))
                    VRC.SDKBase.VRC_Serialization.RegisterType(p.ParameterType);

            object[] parameters = null;
            if (rpcByteCache.ContainsKey(eventProperty.propertyPath))
                parameters = rpcByteCache[eventProperty.propertyPath];
            else
            {
                parameters = VRC.SDKBase.VRC_Serialization.ParameterDecoder(VRC_EditorTools.ReadBytesFromProperty(parameterBytesProperty));
                rpcByteCache.Add(eventProperty.propertyPath, parameters);
            }

            if (parameters == null)
                parameters = new object[paramInfo.Length];
            if (parameters.Length != paramInfo.Length)
                Array.Resize(ref parameters, paramInfo.Length);

            EditorGUI.BeginChangeCheck();

            bool finalParamIsPlayer = false;

            for (int idx = 0; idx < parameters.Length; ++idx)
            {
                Type paramType = paramInfo[idx].ParameterType;
                if (paramType == typeof(Color))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = Color.black;
                    parameters[idx] = EditorGUILayout.ColorField(paramInfo[idx].Name, (Color)parameters[idx]);
                }
                else if (paramType == typeof(bool))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = false;
                    parameters[idx] = EditorGUILayout.Toggle(paramInfo[idx].Name, (bool)parameters[idx]);
                }
                else if (paramType.IsEnum)
                {
                    // make an array of strings of the enum values
                    var values = Enum.GetValues(paramType);
                    string[] itemStrings = new string[values.Length];
                    int i = 0;
                    foreach (var item in Enum.GetValues(paramType))
                    {
                        itemStrings[i++] = item.ToString();
                    }
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = Enum.Parse(paramType, itemStrings[0]);
                    parameters[idx] = Enum.Parse(paramType, itemStrings[EditorGUILayout.Popup(paramInfo[idx].Name, (int)parameters[idx], itemStrings)]);
                }
                else if (paramType == typeof(double))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = 0d;
                    parameters[idx] = EditorGUILayout.DoubleField(paramInfo[idx].Name, (double)parameters[idx]);
                }
                else if (paramType == typeof(float))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = 0f;
                    parameters[idx] = EditorGUILayout.FloatField(paramInfo[idx].Name, (float)parameters[idx]);
                }
                else if (paramType == typeof(int))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = 0;
                    parameters[idx] = EditorGUILayout.IntField(paramInfo[idx].Name, (int)parameters[idx]);
                }
                else if (typeof(VRC.SDKBase.VRCPlayerApi).IsAssignableFrom(paramType))
                {
                    if (idx == parameters.Length - 1)
                        finalParamIsPlayer = true;
                    parameters[idx] = null;
                }
                else if (paramType == typeof(long))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = 0;
                    parameters[idx] = EditorGUILayout.LongField(paramInfo[idx].Name, (long)parameters[idx]);
                }
                else if (paramType == typeof(UnityEngine.Rect))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = new Rect();
                    parameters[idx] = EditorGUILayout.RectField(paramInfo[idx].Name, (UnityEngine.Rect)parameters[idx]);
                }
                else if (paramType == typeof(string))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = "";
                    parameters[idx] = EditorGUILayout.TextField(paramInfo[idx].Name, (string)parameters[idx]);
                }
                else if (paramType == typeof(Vector2))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = new Vector2();
                    parameters[idx] = EditorGUILayout.Vector2Field(paramInfo[idx].Name, (Vector2)parameters[idx]);
                }
                else if (paramType == typeof(Vector3))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = new Vector3();
                    parameters[idx] = EditorGUILayout.Vector3Field(paramInfo[idx].Name, (Vector3)parameters[idx]);
                }
                else if (paramType == typeof(Vector4))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = new Vector4();
                    parameters[idx] = EditorGUILayout.Vector4Field(paramInfo[idx].Name, (Vector4)parameters[idx]);
                }
                else if (paramType == typeof(Quaternion))
                {
                    if (parameters[idx] == null || parameters[idx].GetType() != paramType)
                        parameters[idx] = new Quaternion();
                    parameters[idx] = Quaternion.Euler(EditorGUILayout.Vector3Field(paramInfo[idx].Name, ((Quaternion)parameters[idx]).eulerAngles));
                }
                else if (paramType == typeof(UnityEngine.Object) || paramType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    if (parameters[idx] != null && parameters[idx].GetType() != paramType)
                        parameters[idx] = null;
                    parameters[idx] = EditorGUILayout.ObjectField(paramInfo[idx].Name, (UnityEngine.Object)parameters[idx], paramType, true);
                }
                else
                    EditorGUILayout.LabelField("Unable to handle " + paramType.Name, EditorStyles.boldLabel);
            }

            if (finalParamIsPlayer)
                Array.Resize(ref parameters, parameters.Length - 1);

            if (EditorGUI.EndChangeCheck())
            {
                VRC_EditorTools.WriteBytesToProperty(parameterBytesProperty, VRC.SDKBase.VRC_Serialization.ParameterEncoder(parameters));
                rpcByteCache[eventProperty.propertyPath] = parameters;
            }
        }

        void RenderMethodSelector(SerializedProperty eventProperty)
        {
            SerializedProperty parameterObjectsProperty = eventProperty.FindPropertyRelative("ParameterObjects");
            SerializedProperty parameterStringProperty = eventProperty.FindPropertyRelative("ParameterString");
            SerializedProperty parameterBytesProperty = eventProperty.FindPropertyRelative("ParameterBytes");

            Dictionary<string, List<MethodInfo>> methods = VRC_EditorTools.GetSharedAccessibleMethodsOnGameObjects(parameterObjectsProperty);
            if (methods.Count == 0)
                return;

            List<string> combined = methods
                .Select(pair => pair.Value.Select(s => pair.Key + "." + s.Name))
                .Aggregate((a, b) =>
                {
                    var v = new List<string>();
                    v.AddRange(a);
                    v.AddRange(b);
                    return v;
                }).ToList();
            combined.Insert(0, "Custom Method");

            int currentIndex = string.IsNullOrEmpty(parameterStringProperty.stringValue) ? 0 : combined.FindIndex(s =>
            {
                var split = s.Split('.');
                return split.Length > 1 && s.Split('.')[1] == parameterStringProperty.stringValue;
            });
            if (currentIndex < 0 || currentIndex >= combined.Count)
                currentIndex = 0;

            int newIndex = EditorGUILayout.Popup("Method", currentIndex, combined.ToArray());
            if (newIndex != currentIndex)
            {
                parameterStringProperty.stringValue = "";
                parameterBytesProperty.arraySize = 0;
            }
            currentIndex = newIndex;

            if (currentIndex == 0)
                EditorGUILayout.PropertyField(parameterStringProperty, new GUIContent("Custom Method"));
            else
                parameterStringProperty.stringValue = combined[currentIndex].Split('.')[1];
        }

        private void RenderPropertyEditor(SerializedProperty shadowProperty, SerializedProperty property, GUIContent label, bool isBoolOp = false)
        {
            VRCSDK2.VRC_DataStorage ds = (target as VRCSDK2.VRC_Trigger).gameObject.GetComponent<VRCSDK2.VRC_DataStorage>();
            if (ds != null && ds.data != null && ds.data.Length != 0 && shadowProperty != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);

                bool renderField = false;
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        {
                            SerializedProperty prop = shadowProperty.FindPropertyRelative("ParameterBoolOp");
                            List<string> vals = ds.data.Where(el => el.type == VRCSDK2.VRC_DataStorage.VrcDataType.Bool).Select(el => el.name).ToList();
                            renderField = !ListPopup(vals, prop);
                        }
                        break;
                    case SerializedPropertyType.Float:
                        {
                            SerializedProperty prop = shadowProperty.FindPropertyRelative("ParameterFloat");
                            List<string> vals = ds.data.Where(el => el.type == VRCSDK2.VRC_DataStorage.VrcDataType.Float).Select(el => el.name).ToList();
                            renderField = !ListPopup(vals, prop);
                        }
                        break;
                    case SerializedPropertyType.Integer:
                        {
                            SerializedProperty prop = shadowProperty.FindPropertyRelative("ParameterInt");
                            List<string> vals = ds.data.Where(el => el.type == VRCSDK2.VRC_DataStorage.VrcDataType.Int).Select(el => el.name).ToList();
                            renderField = !ListPopup(vals, prop);
                        }
                        break;
                    case SerializedPropertyType.String:
                        {
                            SerializedProperty prop = shadowProperty.FindPropertyRelative("ParameterString");
                            List<string> vals = ds.data.Where(el => el.type == VRCSDK2.VRC_DataStorage.VrcDataType.String).Select(el => el.name).ToList();
                            renderField = !ListPopup(vals, prop);
                        }
                        break;
                    default:
                        {
                            renderField = true;
                        }
                        break;
                }

                if (renderField)
                    EditorGUILayout.PropertyField(property, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (isBoolOp)
                    VRC_EditorTools.FilteredEnumPopup<VRCSDK2.VRC_EventHandler.VrcBooleanOp>(label.text, property, s => s != VRCSDK2.VRC_EventHandler.VrcBooleanOp.Unused);
                else
                    EditorGUILayout.PropertyField(property, label);
                return;
            }
        }

        private bool ListPopup(List<string> vals, SerializedProperty prop, bool custom = true)
        {
            if (vals.Count == 0)
                return false;

            if (custom)
                vals.Insert(0, "Custom");

            int selectedIdx = prop.stringValue == null ? 0 : vals.IndexOf(prop.stringValue);
            if (selectedIdx < 0 || selectedIdx > vals.Count)
                selectedIdx = 0;

            int idx = EditorGUILayout.Popup(selectedIdx, vals.ToArray());
            if (idx == 0 && custom)
            {
                prop.stringValue = null;
                return false;
            }
            else
            {
                prop.stringValue = vals[idx];
                return true;
            }
        }
    }
}
#endif