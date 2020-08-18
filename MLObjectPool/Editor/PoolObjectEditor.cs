namespace MLObjectPool.Editor
{
    using MLObjectPool;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(PoolObject))]
    public class PoolObjectEditor : Editor
    {
        protected PoolObject script = null;

        private bool onAllocationEnabled = false;
        private bool onRecycleEnabled = false;

        private GUIContent iconToolbarMinus;

        protected virtual void OnEnable()
        {
            script = script ?? target as PoolObject;
            iconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!onAllocationEnabled || !onRecycleEnabled)
            {
                if (GUILayout.Button("Add Pool Event"))
                {
                    GenericMenu menu = new GenericMenu();
                    if (!onAllocationEnabled)
                        menu.AddItem(new GUIContent("OnAllocaton"), false, () => onAllocationEnabled = true);
                    if (!onRecycleEnabled)
                        menu.AddItem(new GUIContent("OnRecycle"), false, () => onRecycleEnabled = true);

                    menu.ShowAsContext();
                }
            }

            if (onAllocationEnabled)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onAllocation"));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.position = new Vector2() { x = rect.position.x + rect.width - 20, y = rect.position.y };
                rect.width = 20;
                rect.height = 20;
                if (GUI.Button(rect, iconToolbarMinus, GUIStyle.none))
                {
                    script.onAllocation.RemoveAllListeners();
                    onAllocationEnabled = false;
                }
            }

            if (onRecycleEnabled)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onRecycle"));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.position = new Vector2() { x = rect.position.x + rect.width - 20, y = rect.position.y };
                rect.width = 20;
                rect.height = 20;
                if (GUI.Button(rect, iconToolbarMinus, GUIStyle.none))
                {
                    script.onRecycle.RemoveAllListeners();
                    onRecycleEnabled = false;
                }
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}
