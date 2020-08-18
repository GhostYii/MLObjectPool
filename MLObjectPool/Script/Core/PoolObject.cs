using UnityEngine;
using UnityEngine.Events;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public class PoolObject : MonoBehaviour, IPoolObjectHandler
    {
        public UnityEvent onAllocation;
        public UnityEvent onRecycle;

        public void OnAllocation()
        {
            onAllocation.Invoke();
        }

        public void OnRecycle()
        {
            onRecycle.Invoke();
        }
    }
}
