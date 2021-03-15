using UnityEngine;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public class PrefabPoolObject : MonoBehaviour
    {
        private PrefabPool pool = null;

        public PrefabPool Pool
        {
            get => pool;
            internal set => pool = value;
        }
    }
}
