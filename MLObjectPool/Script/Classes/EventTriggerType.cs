using System;
using UnityEngine.Events;

namespace MLObjectPool
{
    public enum EventTriggerType
    {
        Allocation,
        Recycle,
        BeforeAllocation,
        BeforeRecycle,
        AfterAllocation,
        AfterRecycle
    }

    [Serializable]
    public class ObjectPoolEvent : UnityEvent<PoolBase> { }

}