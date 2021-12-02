#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using VRC.SDKBase;

namespace VRCSDK2
{
#if VRC_SDK_VRCSDK2
    [CustomEditor(typeof(VRCSDK2.VRC_EventHandler))]
    public class EventHandlerEditor : UnityEditor.Editor
    {
        bool showDeferredEvents = false;

        static VRCSDK2.VRC_EventHandler.VrcEventType lastAddedEventType = VRCSDK2.VRC_EventHandler.VrcEventType.SendMessage;

        public override void OnInspectorGUI()
        {
            VRCSDK2.VRC_EventHandler myTarget = (VRCSDK2.VRC_EventHandler)target;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID:");
            EditorGUILayout.EndHorizontal();

            if (myTarget.GetComponent<VRCSDK2.VRC_Trigger>() != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Add Events via the VRC_Trigger on this object.");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                RenderOldEditor(myTarget);

                if (EditorGUI.EndChangeCheck())
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            if (myTarget.deferredEvents.Count > 0)
            {
                showDeferredEvents = EditorGUILayout.Foldout(showDeferredEvents, "Deferred Events");
                if (showDeferredEvents)
                    RenderEvents(myTarget.deferredEvents);
            }
        }

        int[] sendMessageMethodIndicies;
        private void RenderOldEditor(VRCSDK2.VRC_EventHandler myTarget)
        {
            EditorGUILayout.HelpBox("Please use a VRC_Trigger in the future.", MessageType.Error);

            if (GUILayout.Button("Add Event Handler"))
                myTarget.Events.Add(new VRCSDK2.VRC_EventHandler.VrcEvent());

            bool first = true;
            int deleteEventIndex = -1;
            if (sendMessageMethodIndicies == null || sendMessageMethodIndicies.Length != myTarget.Events.Count)
                sendMessageMethodIndicies = new int[myTarget.Events.Count + 1];

            for (int i = 0; i < myTarget.Events.Count; ++i)
            {
                if (!first)
                    EditorGUILayout.Separator();
                first = false;

                EditorGUILayout.LabelField("Event " + (i + 1).ToString());

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Event Name");
                myTarget.Events[i].Name = EditorGUILayout.TextField(myTarget.Events[i].Name);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Event Type");
                myTarget.Events[i].EventType = (VRCSDK2.VRC_EventHandler.VrcEventType)EditorGUILayout.EnumPopup(myTarget.Events[i].EventType);
                EditorGUILayout.EndHorizontal();

                switch (myTarget.Events[i].EventType)
                {
                    case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationBool:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Variable");
                        myTarget.Events[i].ParameterString = EditorGUILayout.TextField(myTarget.Events[i].ParameterString);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Operation");
                        myTarget.Events[i].ParameterBoolOp = (VRCSDK2.VRC_EventHandler.VrcBooleanOp)EditorGUILayout.EnumPopup(myTarget.Events[i].ParameterBoolOp);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationFloat:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Variable");
                        myTarget.Events[i].ParameterString = EditorGUILayout.TextField(myTarget.Events[i].ParameterString);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Value");
                        myTarget.Events[i].ParameterFloat = EditorGUILayout.FloatField(myTarget.Events[i].ParameterFloat);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationTrigger:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Trigger");
                        myTarget.Events[i].ParameterString = EditorGUILayout.TextField(myTarget.Events[i].ParameterString);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.AudioTrigger:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("AudioSource");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.MeshVisibility:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Mesh");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Operation");
                        myTarget.Events[i].ParameterBoolOp = (VRCSDK2.VRC_EventHandler.VrcBooleanOp)EditorGUILayout.EnumPopup(myTarget.Events[i].ParameterBoolOp);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.PlayAnimation:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Target");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Animation");
                        myTarget.Events[i].ParameterString = EditorGUILayout.TextField(myTarget.Events[i].ParameterString);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.RunConsoleCommand:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Command");
                        myTarget.Events[i].ParameterString = EditorGUILayout.TextField(myTarget.Events[i].ParameterString);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.SendMessage:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Receiver");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Message");

                        // sorry for this shit show. Below allows us to show a list of public methods, but also allow custom messages
                        var methods = VRC_EditorTools.GetAccessibleMethodsOnGameObject(myTarget.Events[i].ParameterObject);
                        List<string> methodList = methods.Values.Aggregate(new List<string>(), (acc, lst) => { acc.AddRange(lst.Select(mi => mi.Name)); return acc; });
                        methodList.Add("Custom Message");

                        string[] _choices = methodList.ToArray();

                        int currentIndex = methodList.Count - 1;

                        if (methodList.Contains(myTarget.Events[i].ParameterString))
                            currentIndex = methodList.IndexOf(myTarget.Events[i].ParameterString);

                        sendMessageMethodIndicies[i] = EditorGUILayout.Popup(currentIndex, _choices);

                        if (sendMessageMethodIndicies[i] != methodList.Count - 1)
                        {
                            myTarget.Events[i].ParameterString = _choices[sendMessageMethodIndicies[i]];
                        }
                        else
                        {
                            if (methodList.Contains(myTarget.Events[i].ParameterString))
                                myTarget.Events[i].ParameterString = "";

                            myTarget.Events[i].ParameterString = EditorGUILayout.TextField(myTarget.Events[i].ParameterString);
                        }

                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.SetGameObjectActive:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("GameObject");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Operation");
                        myTarget.Events[i].ParameterBoolOp = (VRCSDK2.VRC_EventHandler.VrcBooleanOp)EditorGUILayout.EnumPopup(myTarget.Events[i].ParameterBoolOp);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.SetParticlePlaying:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Target");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Operation");
                        myTarget.Events[i].ParameterBoolOp = (VRCSDK2.VRC_EventHandler.VrcBooleanOp)EditorGUILayout.EnumPopup(myTarget.Events[i].ParameterBoolOp);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.TeleportPlayer:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Location");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Align Room To Destination");
						myTarget.Events[i].ParameterBoolOp = (VRCSDK2.VRC_EventHandler.VrcBooleanOp)EditorGUILayout.EnumPopup(myTarget.Events[i].ParameterBoolOp);
						EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.SetWebPanelURI:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("URI");
                        myTarget.Events[i].ParameterString = EditorGUILayout.TextField(myTarget.Events[i].ParameterString);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Panel");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcEventType.SetWebPanelVolume:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Volume");
                        myTarget.Events[i].ParameterFloat = EditorGUILayout.FloatField(myTarget.Events[i].ParameterFloat);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Panel");
                        myTarget.Events[i].ParameterObject = (GameObject)EditorGUILayout.ObjectField(myTarget.Events[i].ParameterObject, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        break;
                    default:
                        EditorGUILayout.BeginHorizontal();
                        GUIStyle redText = new GUIStyle();
                        redText.normal.textColor = Color.red;
                        EditorGUILayout.LabelField("Unsupported event type", redText);
                        EditorGUILayout.EndHorizontal();
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Delete " + myTarget.Events[i].Name + "?");
                if (GUILayout.Button("delete"))
                    deleteEventIndex = i;
                EditorGUILayout.EndHorizontal();

                if (myTarget.Events[i].ParameterObject == null)
                    myTarget.Events[i].ParameterObject = myTarget.gameObject;
            }


            if (deleteEventIndex != -1)
                myTarget.Events.RemoveAt(deleteEventIndex);
        }

        private void RenderEvents(IEnumerable<VRCSDK2.VRC_EventHandler.EventInfo> entries)
        {
            foreach (VRCSDK2.VRC_EventHandler.EventInfo entry in entries)
            {
                EditorGUILayout.PrefixLabel("Target");
                EditorGUILayout.ObjectField(entry.evt.ParameterObject, typeof(GameObject), true);

                EditorGUILayout.LabelField(string.Format("Name: {0}", entry.evt.Name));
                EditorGUILayout.LabelField(string.Format("Type: {0}", entry.evt.EventType));
                EditorGUILayout.LabelField(string.Format("Bool: {0}", entry.evt.ParameterBool));
                EditorGUILayout.LabelField(string.Format("Float: {0}", entry.evt.ParameterFloat));
                EditorGUILayout.LabelField(string.Format("Int: {0}", entry.evt.ParameterInt));
                EditorGUILayout.LabelField(string.Format("String: {0}", entry.evt.ParameterString));

                EditorGUILayout.Space();
            }
        }

        public static void RenderEditor(VRCSDK2.VRC_EventHandler myTarget)
        {
            bool first = true;
            int deleteEventIndex = -1;

            for (int i = 0; i < myTarget.Events.Count; ++i)
            {
                if (!first)
                    EditorGUILayout.Separator();
                first = false;

                if (RenderEventHeader(myTarget, myTarget.Events[i]))
                    deleteEventIndex = i;

                RenderEventHeader(myTarget, myTarget.Events[i]);

                if (myTarget.Events[i].ParameterObject == null)
                    myTarget.Events[i].ParameterObject = myTarget.gameObject;
            }

            if (deleteEventIndex != -1)
                myTarget.Events.RemoveAt(deleteEventIndex);
        }

        public static VRCSDK2.VRC_EventHandler.VrcEvent RenderAddEvent(VRCSDK2.VRC_EventHandler myTarget)
        {
            VRCSDK2.VRC_EventHandler.VrcEvent newEvent = null;

            EditorGUILayout.BeginHorizontal();
            lastAddedEventType = VRC_EditorTools.FilteredEnumPopup("New Event Type", lastAddedEventType, (v) => v != VRCSDK2.VRC_EventHandler.VrcEventType.SpawnObject && v != VRCSDK2.VRC_EventHandler.VrcEventType.SendMessage);
            if (GUILayout.Button("Add"))
            {
                newEvent = new VRCSDK2.VRC_EventHandler.VrcEvent
                {
                    EventType = lastAddedEventType,
                    ParameterObject = myTarget.gameObject
                };
                myTarget.Events.Add(newEvent);
                EditorUtility.SetDirty(myTarget);
            }
            EditorGUILayout.EndHorizontal();

            return newEvent;
        }

        public static bool RenderEventHeader(VRCSDK2.VRC_EventHandler myTarget, VRCSDK2.VRC_EventHandler.VrcEvent evt)
        {
            EditorGUILayout.BeginHorizontal();
            evt.EventType = VRC_EditorTools.FilteredEnumPopup("New Event Type", evt.EventType, (v) => v != VRCSDK2.VRC_EventHandler.VrcEventType.SpawnObject && v != VRCSDK2.VRC_EventHandler.VrcEventType.SendMessage);
            bool delete = GUILayout.Button("Remove");
            EditorGUILayout.EndHorizontal();

            return delete;
        }
    }

    [CustomEditor(typeof(VRC.SDKBase.VRC_EventHandler))]
    public class SDKBaseEventHandlerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Event Handlers are not supported in VRCSDK3.");
            if (GUILayout.Button("replace me with the correct VRC_EventHandler"))
            {
                var go = ((VRC.SDKBase.VRC_EventHandler)target).gameObject;
                DestroyImmediate(target);
                go.AddComponent<VRCSDK2.VRC_EventHandler>();
            }
        }
    }
#else

    [CustomEditor(typeof(VRC.SDKBase.VRC_EventHandler))]
    public class SDKBaseEventHandlerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Event Handlers are not supported in VRCSDK3.");
            if( GUILayout.Button("delete me") )
                DestroyImmediate(target);
        }
    }

#endif


}
#endif
