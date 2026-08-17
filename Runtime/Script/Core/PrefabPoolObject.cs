using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public class PrefabPoolObject : MonoBehaviour
    {
        private readonly Dictionary<EventTriggerType, ObjectPoolEvent> _events =
            new Dictionary<EventTriggerType, ObjectPoolEvent>();

        private IBeforeAllocationHandler _beforeAllocation;
        private IAllocationHandler _allocation;
        private IAfterAllocationHandler _afterAllocation;
        private IBeforeRecycleHandler _beforeRecycle;
        private IRecycleHandler _recycle;
        private IAfterRecycleHandler _afterRecycle;
        private PrefabPool _pool;

        public PrefabPool Pool
        {
            get => _pool;
            internal set => _pool = value;
        }

        private void Awake()
        {
            CacheHandlers();
        }

        public void AddEvent(EventTriggerType type, UnityAction<PoolBase> call)
        {
            if (call == null)
                return;

            if (!_events.TryGetValue(type, out var evt))
            {
                evt = new ObjectPoolEvent();
                _events.Add(type, evt);
            }

            evt.AddListener(call);
        }

        public void RemoveEvent(EventTriggerType type, UnityAction<PoolBase> call)
        {
            if (call == null || !_events.TryGetValue(type, out var evt))
                return;

            evt.RemoveListener(call);
        }

        internal void ClearEvents()
        {
            foreach (var evt in _events.Values)
                evt.RemoveAllListeners();

            _events.Clear();
        }

        internal bool TryInvoke(EventTriggerType type, PoolBase pool)
        {
            if (_events.TryGetValue(type, out var evt))
            {
                evt.Invoke(pool);
                return true;
            }

            switch (type)
            {
                case EventTriggerType.BeforeAllocation:
                    if (_beforeAllocation != null)
                    {
                        _beforeAllocation.OnBeforeAllocation(pool);
                        return true;
                    }
                    break;
                case EventTriggerType.Allocation:
                    if (_allocation != null)
                    {
                        _allocation.OnAllocation(pool);
                        return true;
                    }
                    break;
                case EventTriggerType.AfterAllocation:
                    if (_afterAllocation != null)
                    {
                        _afterAllocation.OnAfterAllocation(pool);
                        return true;
                    }
                    break;
                case EventTriggerType.BeforeRecycle:
                    if (_beforeRecycle != null)
                    {
                        _beforeRecycle.OnBeforeRecycle(pool);
                        return true;
                    }
                    break;
                case EventTriggerType.Recycle:
                    if (_recycle != null)
                    {
                        _recycle.OnRecycle(pool);
                        return true;
                    }
                    break;
                case EventTriggerType.AfterRecycle:
                    if (_afterRecycle != null)
                    {
                        _afterRecycle.OnAfterRecycle(pool);
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void CacheHandlers()
        {
            _beforeAllocation = GetComponent<IBeforeAllocationHandler>();
            _allocation = GetComponent<IAllocationHandler>();
            _afterAllocation = GetComponent<IAfterAllocationHandler>();
            _beforeRecycle = GetComponent<IBeforeRecycleHandler>();
            _recycle = GetComponent<IRecycleHandler>();
            _afterRecycle = GetComponent<IAfterRecycleHandler>();
        }
    }
}
