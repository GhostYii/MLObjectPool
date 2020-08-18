namespace MLObjectPool.Editor
{
    using MLObjectPool;
    using UnityEditor;

    [CustomEditor(typeof(ObjectPool))]
    public class ObjectPoolEditor : Editor
    {
        private bool foldout = true;
        private ObjectPool script = null;

        private void OnEnable()
        {
            script = script ?? target as ObjectPool;
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
                        EditorGUILayout.LabelField($"Auto Expand:{ pool.AutoExpand}");
                        EditorGUILayout.LabelField($"Allocation Count:{pool.Size - pool.GetSpawnedObjectCount()}");
                        EditorGUI.indentLevel--;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    }
}
