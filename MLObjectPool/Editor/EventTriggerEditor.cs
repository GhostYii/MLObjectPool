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

        private bool onAllocationEnabled = false;
        private bool onRecycleEnabled = false;
        private bool onBAllocationEnabled = false;
        private bool onBRecycleEnabled = false;
        private bool onAAllocationEnabled = false;
        private bool onARecycleEnabled = false;

        private List<EventTriggerType> eventNames = new List<EventTriggerType>();
        private List<EventTriggerType> enabledEvents = new List<EventTriggerType>();
        //private Dictionary<EventTriggerType, bool> eventMap = new Dictionary<EventTriggerType, bool>();

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

            Log.Print(eventNames.Count.ToString());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!onAllocationEnabled || !onRecycleEnabled || !onBAllocationEnabled
                || !onBRecycleEnabled || !onAAllocationEnabled || !onARecycleEnabled)
            {
                if (GUILayout.Button("Add Pool Event"))
                {
                    GenericMenu menu = new GenericMenu();
                    if (!onAllocationEnabled)
                        menu.AddItem(new GUIContent("OnAllocaton"), false, () =>  onAllocationEnabled= true);
                    if (!onRecycleEnabled)
                        menu.AddItem(new GUIContent("OnRecycle"), false, () => onRecycleEnabled = true);
                    if (!onBAllocationEnabled)
                        menu.AddItem(new GUIContent("OnBeforeAllocation"), false, () => onBAllocationEnabled = true);
                    if (!onBRecycleEnabled)
                        menu.AddItem(new GUIContent("OnBeforeRecycle"), false, () => onBRecycleEnabled = true);
                    if (!onAAllocationEnabled)
                        menu.AddItem(new GUIContent("OnAfterAllocation"), false, () => onAAllocationEnabled = true);
                    if (!onARecycleEnabled)
                        menu.AddItem(new GUIContent("OnAfterRecycle"), false, () => onARecycleEnabled = true);

                    menu.ShowAsContext();
                }
            }

            if (onAllocationEnabled)
            {
                DrawProperty("onAllocation", () =>
                {
                    RemoveAllListener(script.onAllocation);
                    onAllocationEnabled = false;
                });
            }

            if (onRecycleEnabled)
            {
                DrawProperty("onRecycle", () =>
                {
                    RemoveAllListener(script.onRecycle);
                    onRecycleEnabled = false;
                });
            }

            if (onBAllocationEnabled)
            {
                DrawProperty("onBeforeAllocation", () =>
                {
                    RemoveAllListener(script.onBeforeAllocation);
                    onBAllocationEnabled = false;
                });
            }

            if (onBRecycleEnabled)
            {
                DrawProperty("onBeforeRecycle", () =>
                {
                    RemoveAllListener(script.onBeforeRecycle);
                    onBRecycleEnabled = false;
                });
            }

            if (onAAllocationEnabled)
            {
                DrawProperty("onAfterAllocation", () =>
                {
                    RemoveAllListener(script.onAfterAllocation);
                    onAAllocationEnabled = false;
                });
            }

            if (onARecycleEnabled)
            {
                DrawProperty("onAfterRecycle", () =>
                {
                    RemoveAllListener(script.onAfterRecycle);
                    onARecycleEnabled = false;
                });
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperty(string name, System.Action onButton)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.position = new Vector2() { x = rect.position.x + rect.width - 20, y = rect.position.y };
            rect.width = 20;
            rect.height = 20;
            if (GUI.Button(rect, iconToolbarMinus, GUIStyle.none))
            {
                onButton();
                Repaint();
            }
        }

        private void RemoveAllListener(UnityEvent uEvent)
        {
            uEvent.RemoveAllListeners();

            while (uEvent.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(uEvent, 0);

        }
    }
}
