using UnityEngine;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public class PrefabPoolObject : MonoBehaviour
    {
        public bool autoExpand = false;
        public int size = 10;
    }
}
