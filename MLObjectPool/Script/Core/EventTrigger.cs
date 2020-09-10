using UnityEngine;
using UnityEngine.Events;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public class EventTrigger : MonoBehaviour, IPoolObjectHandler, IPoolObjectBeforeHandler, IPoolObjectAfterHandler
    {
        public UnityEvent onAllocation;
        public UnityEvent onRecycle;
        public UnityEvent onBeforeAllocation;
        public UnityEvent onBeforeRecycle;
        public UnityEvent onAfterAllocation;
        public UnityEvent onAfterRecycle;

        public void OnAfterAllocation()
        {
            onAfterAllocation.Invoke();
        }

        public void OnAfterRecycle()
        {
            onAfterRecycle.Invoke();
        }

        public void OnAllocation()
        {
            onAllocation.Invoke();
        }

        public void OnBeforeAllocation()
        {
            onBeforeAllocation.Invoke();
        }

        public void OnBeforeRecycle()
        {
            onBeforeRecycle.Invoke();
        }

        public void OnRecycle()
        {
            onRecycle.Invoke();
        }
    }
}
