namespace MLObjectPool.Editor
{
    using MLObjectPool;
    using UnityEditor;
    using UnityEngine;

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

        private GUIContent iconToolbarMinus;

        protected virtual void OnEnable()
        {
            script = script ?? target as EventTrigger;
            iconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
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
                        menu.AddItem(new GUIContent("OnAllocaton"), false, () => onAllocationEnabled = true);
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
                    script.onAllocation.RemoveAllListeners();
                    onAllocationEnabled = false;
                });
            }

            if (onRecycleEnabled)
            {
                DrawProperty("onRecycle", () =>
                {
                    script.onRecycle.RemoveAllListeners();
                    onRecycleEnabled = false;
                });
            }

            if (onBAllocationEnabled)
            {
                DrawProperty("onBeforeAllocation", () =>
                {
                    script.onBeforeAllocation.RemoveAllListeners();
                    onBAllocationEnabled = false;
                });
            }

            if (onBRecycleEnabled)
            {
                DrawProperty("onBeforeRecycle", () =>
                {
                    script.onBeforeRecycle.RemoveAllListeners();
                    onBRecycleEnabled = false;
                });
            }

            if (onAAllocationEnabled)
            {
                DrawProperty("onAfterAllocation", () =>
                {
                    script.onAfterAllocation.RemoveAllListeners();
                    onAAllocationEnabled = false;
                });
            }

            if (onARecycleEnabled)
            {
                DrawProperty("onAfterRecycle", () =>
                {
                    script.onAfterRecycle.RemoveAllListeners();
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
    }
}
