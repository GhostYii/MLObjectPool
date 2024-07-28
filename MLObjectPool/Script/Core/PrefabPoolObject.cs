using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public class PrefabPoolObject : MonoBehaviour
    {
        private PrefabPool pool = null;

        //所有事件会在该对象被回收时清空
        internal Dictionary<EventTriggerType, ObjectPoolEvent> eventMap = new Dictionary<EventTriggerType, ObjectPoolEvent>();

        public PrefabPool Pool
        {
            get => pool;
            internal set => pool = value;
        }

        public void AddEvent(EventTriggerType type, UnityAction<PoolBase> call)
        {
            if (!eventMap.ContainsKey(type))
                eventMap.Add(type, new ObjectPoolEvent());

            eventMap[type].AddListener(call);
        }        
    }
}
