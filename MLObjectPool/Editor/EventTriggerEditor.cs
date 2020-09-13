namespace MLObjectPool.Editor
{
    using MLObjectPool;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Events;
    using UnityEngine;
    using UnityEngine.Events;

    [CustomEditor(typeof(EventTrigger))]
    public class EventTriggerEditor : Editor
    {
        protected EventTrigger script = null;

        private List<EventTriggerType> eventNames = new List<EventTriggerType>();
        private List<EventTriggerType> enabledEvents = new List<EventTriggerType>();
        private List<EventTriggerType> removableEvents = new List<EventTriggerType>();

        private GUIContent iconToolbarMinus;

        protected virtual void OnEnable()
        {
            script = script ?? target as EventTrigger;
            iconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));

            int count = Enum.GetNames(typeof(EventTriggerType)).Length;
            if (eventNames.Count != count)
            {
                for (int i = 0; i < count; i++)
                {
                    if (!eventNames.Contains((EventTriggerType)i))
                        eventNames.Add((EventTriggerType)i);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (var e in removableEvents)            
                enabledEvents.Remove(e);
            removableEvents.Clear();

            if (enabledEvents.Count != eventNames.Count)
            {
                if (GUILayout.Button("Add Pool Event"))
                {
                    GenericMenu menu = new GenericMenu();
                    if (!enabledEvents.Contains(EventTriggerType.Allocation))
                        menu.AddItem(new GUIContent("Allocaton"), false, ()=>enabledEvents.Add(EventTriggerType.Allocation));
                    if (!enabledEvents.Contains(EventTriggerType.Recycle))
                        menu.AddItem(new GUIContent("Recycle"), false, () => enabledEvents.Add(EventTriggerType.Recycle));
                    if (!enabledEvents.Contains(EventTriggerType.BeforeAllocation))
                        menu.AddItem(new GUIContent("BeforeAllocation"), false, () => enabledEvents.Add(EventTriggerType.BeforeAllocation));
                    if (!enabledEvents.Contains(EventTriggerType.BeforeRecycle))
                        menu.AddItem(new GUIContent("BeforeRecycle"), false, () => enabledEvents.Add(EventTriggerType.BeforeRecycle));
                    if (!enabledEvents.Contains(EventTriggerType.AfterAllocation))
                        menu.AddItem(new GUIContent("AfterAllocation"), false, () => enabledEvents.Add(EventTriggerType.AfterAllocation));
                    if (!enabledEvents.Contains(EventTriggerType.AfterRecycle))
                        menu.AddItem(new GUIContent("AfterRecycle"), false, () => enabledEvents.Add(EventTriggerType.AfterRecycle));

                    menu.ShowAsContext();
                }
            }

            foreach (var e in enabledEvents)
            {
                DrawProperty($"on{e}", () =>
                {
                    RemoveAllListener(e);
                    removableEvents.Add(e);
                });
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperty(string name, Action onCloseButton)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.position = new Vector2() { x = rect.position.x + rect.width - 20, y = rect.position.y };
            rect.width = 20;
            rect.height = 20;
            if (GUI.Button(rect, iconToolbarMinus, GUIStyle.none))
            {
                onCloseButton();
                Repaint();
            }
        }

        private void RemoveAllListener(EventTriggerType type)
        {
            UnityEvent e = null;
            switch (type)
            {
                case EventTriggerType.Allocation:
                    e = script.onAllocation;
                    break;
                case EventTriggerType.Recycle:
                    e = script.onRecycle;
                    break;
                case EventTriggerType.BeforeAllocation:
                    e = script.onBeforeAllocation;
                    break;
                case EventTriggerType.BeforeRecycle:
                    e = script.onBeforeRecycle;
                    break;
                case EventTriggerType.AfterAllocation:
                    e = script.onAfterAllocation;
                    break;
                case EventTriggerType.AfterRecycle:
                    e = script.onAfterRecycle;
                    break;
                default:
                    break;
            }
            RemoveAllListener(e);
        }

        private void RemoveAllListener(UnityEvent uEvent)
        {
            uEvent.RemoveAllListeners();

            while (uEvent.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(uEvent, 0);

        }
    }
}
