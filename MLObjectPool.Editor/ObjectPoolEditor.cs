﻿using UnityEditor;

namespace MLObjectPool.Editor
{
    [CustomEditor(typeof(ObjectPoolManager))]
    public class ObjectPoolEditor : UnityEditor.Editor
    {
        private bool foldout = true;
        private ObjectPoolManager script = null;

        private void OnEnable()
        {
            script = script ?? target as ObjectPoolManager;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            script._persistent = EditorGUILayout.Toggle("Dont Destory", script._persistent);

            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Pools");
            if (foldout)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    foreach (var item in script.PoolNames)
                    {
                        var pool = script.FindPoolByName(item);
                        EditorGUILayout.LabelField($"[{item} Pool]: ");
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField($"Size:{ pool.Size}");
                        EditorGUILayout.LabelField($"Allocation Count:{pool.Size - pool.GetSpawnedObjectCount()}");
                        OnInsoectorInfoGUI(pool);
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        protected virtual void OnInsoectorInfoGUI(PoolBase pool)
        {
            EditorGUILayout.LabelField($"Auto Expand:{ pool.AutoExpand}");
        }
    }
}
