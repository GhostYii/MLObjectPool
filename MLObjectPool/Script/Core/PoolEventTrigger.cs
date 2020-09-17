using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public class PoolEventTrigger : MonoBehaviour,
        IBeforeAllocationHandler,
        IAllocationHanlder,
        IAfterAllocationHandler,
        IBeforeRecycleHandler,
        IRecycleHandler,
        IAfterRecycleHandler
    {
        [Serializable]
        public class TriggerEvent : UnityEvent<PoolBase> { }

        [Serializable]
        public class Entry
        {
            public EventTriggerType eventID = EventTriggerType.Allocation;
            public TriggerEvent callback = new TriggerEvent();
        }

        [FormerlySerializedAs("delegates")]
        [SerializeField]
        private List<Entry> m_Delegates;

        public List<Entry> triggers
        {
            get
            {
                if (m_Delegates == null)
                    m_Delegates = new List<Entry>();
                return m_Delegates;
            }
            set { m_Delegates = value; }
        }

        private void Execute(EventTriggerType id, PoolBase data)
        {
            for (int i = 0, imax = triggers.Count; i < imax; ++i)
            {
                var ent = triggers[i];
                if (ent.eventID == id && ent.callback != null)
                    ent.callback.Invoke(data);
            }
        }


        public void OnAfterAllocation(PoolBase pool)
        {
            Execute(EventTriggerType.AfterAllocation, pool);
        }

        public void OnAfterRecycle(PoolBase pool)
        {
            Execute(EventTriggerType.AfterRecycle, pool);
        }

        public void OnAllocation(PoolBase pool)
        {
            Execute(EventTriggerType.Allocation, pool);
        }

        public void OnBeforeAllocation(PoolBase pool)
        {
            Execute(EventTriggerType.BeforeAllocation, pool);
        }

        public void OnBeforeRecycle(PoolBase pool)
        {
            Execute(EventTriggerType.BeforeRecycle, pool);
        }

        public void OnRecycle(PoolBase pool)
        {
            Execute(EventTriggerType.Recycle, pool);
        }
    }
}
