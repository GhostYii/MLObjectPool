using System;
using UnityEngine;
using System.Collections.Generic;

namespace MLObjectPool
{
    [Serializable]
    public sealed class PrefabPool : PoolBase
    {
        private GameObject prefab = null;
        private List<GameObject> objects = new List<GameObject>();
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private Dictionary<GameObject, PoolObjectInfo> infoMap = new Dictionary<GameObject, PoolObjectInfo>();

        internal static Transform poolRoot = null;

        internal PrefabPool(GameObject go, int size = 10, bool isExpand = true)
        {
            autoExpand = isExpand;

            poolRoot = poolRoot ?? new GameObject("Prefab Pool").transform;
            prefab = go;

            for (int i = 0; i < size; i++)
            {
                AddGameObject(CreatePrefab());
            }
        }

        public GameObject Allocation()
        {
            GameObject go = GetGameObject();
            PrefabPoolObject script = null;

            IBeforeAllocationHandler beforeHandler = null;
            IAllocationHanlder handler = null;
            IAfterAllocationHandler afterHandler = null;

            if (go.TryGetComponent<PrefabPoolObject>(out script))
            {
                if (script.eventMap.ContainsKey(EventTriggerType.BeforeAllocation))
                    script.eventMap[EventTriggerType.BeforeAllocation].Invoke(this);
                else if (go.TryGetComponent<IBeforeAllocationHandler>(out beforeHandler))
                    beforeHandler.OnBeforeAllocation(this);

                if (infoMap.ContainsKey(go))
                    infoMap[go].Allocation();
                
                if (script.eventMap.ContainsKey(EventTriggerType.Allocation))
                    script.eventMap[EventTriggerType.Allocation].Invoke(this);
                else if (go.TryGetComponent<IAllocationHanlder>(out handler))
                    handler.OnAllocation(this);
                else
                    OnGameObjectSpawn(go);

                if (script.eventMap.ContainsKey(EventTriggerType.AfterAllocation))
                    script.eventMap[EventTriggerType.AfterAllocation].Invoke(this);
                else if (go.TryGetComponent<IAfterAllocationHandler>(out afterHandler))
                    afterHandler.OnAfterAllocation(this);
            }
            else
            {
                if (go.TryGetComponent<IBeforeAllocationHandler>(out beforeHandler))
                    beforeHandler.OnBeforeAllocation(this);

                if (infoMap.ContainsKey(go))
                    infoMap[go].Allocation();

                if (go.TryGetComponent<IAllocationHanlder>(out handler))
                    handler.OnAllocation(this);
                else
                    OnGameObjectSpawn(go);

                if (go.TryGetComponent<IAfterAllocationHandler>(out afterHandler))
                    afterHandler.OnAfterAllocation(this);
            }

            spawnedObjects.Add(go);

            return go;

        }

        public GameObject[] Allocation(int size)
        {
            if (size < 0)
            {
                Log.PrintError($"{prefab} pool can not allocation {size} object.\npool size must be positive interger.");
                return null;
            }

            List<GameObject> goLst = new List<GameObject>();
            for (int i = 0; i < size; i++)
                goLst.Add(Allocation());

            return goLst.ToArray();
        }

        public override object Allocation(bool isExpand)
        {
            autoExpand = isExpand;
            return Allocation();
        }

        public bool Recycle(GameObject obj)
        {
            if (objects.Contains(obj))
            {
                if (spawnedObjects.Contains(obj))
                {
                    PrefabPoolObject script = null;
                    IBeforeRecycleHandler beforeHandler = null;
                    IRecycleHandler handler = null;
                    IAfterRecycleHandler afterHandler = null;

                    if (obj.TryGetComponent<PrefabPoolObject>(out script))
                    {
                        if (script.eventMap.ContainsKey(EventTriggerType.BeforeRecycle))
                            script.eventMap[EventTriggerType.BeforeRecycle].Invoke(this);
                        else if (obj.TryGetComponent<IBeforeRecycleHandler>(out beforeHandler))
                            beforeHandler.OnBeforeRecycle(this);

                        if (infoMap.ContainsKey(obj))
                            infoMap[obj].Recycle();
                        spawnedObjects.Remove(obj);

                        if (script.eventMap.ContainsKey(EventTriggerType.Recycle))
                            script.eventMap[EventTriggerType.Recycle].Invoke(this);
                        else if (obj.TryGetComponent<IRecycleHandler>(out handler))
                            handler.OnRecycle(this);
                        else
                            OnGameObjectDespawn(obj);

                        if (script.eventMap.ContainsKey(EventTriggerType.AfterRecycle))
                            script.eventMap[EventTriggerType.AfterRecycle].Invoke(this);
                        else if (obj.TryGetComponent<IAfterRecycleHandler>(out afterHandler))
                            afterHandler.OnAfterRecycle(this);
                    }
                    else
                    {
                        if (obj.TryGetComponent<IBeforeRecycleHandler>(out beforeHandler))
                            beforeHandler.OnBeforeRecycle(this);

                        if (infoMap.ContainsKey(obj))
                            infoMap[obj].Recycle();
                        spawnedObjects.Remove(obj);

                        if (obj.TryGetComponent<IRecycleHandler>(out handler))
                            handler.OnRecycle(this);
                        else
                            OnGameObjectDespawn(obj);

                        if (obj.TryGetComponent<IAfterRecycleHandler>(out afterHandler))
                            afterHandler.OnAfterRecycle(this);
                    }

                    script?.eventMap.Clear();
                    return true;
                }
                else
                {
                    Log.PrintWarning($"{obj} is not exist in {prefab} pool cache.");
                    return false;
                }
            }
            else
            {
                Log.PrintWarning($"{obj} is not exist in {prefab} pool.");
                return false;
            }
        }

        public override bool Recycle(object obj, Type type)
        {
            if (!type.Equals(typeof(GameObject)))
                return false;

            return Recycle((GameObject)obj);
        }

        public bool Recycle(GameObject[] objs)
        {
            bool ack = true;
            foreach (var obj in objs)
                ack = ack && Recycle(obj);

            return ack;
        }

        public override bool RecycleAll()
        {
            bool ack = true;
            List<GameObject> tmpLst = new List<GameObject>(spawnedObjects);
            foreach (var obj in tmpLst)
                ack = Recycle(obj) && ack;

            return ack;
        }

        private GameObject GetGameObject()
        {
            foreach (var obj in infoMap)
            {
                if (obj.Value.isAvalible)
                    return obj.Key;
            }

            if (autoExpand)
            {
                int tmpSize = size + 1;
                int createSize = size;
                GameObject lastOne = null;
                for (int i = 0; i < createSize; i++)
                {
                    lastOne = CreatePrefab();
                    AddGameObject(lastOne);
                }
                return lastOne;
            }
            else
            {
                Log.PrintWarning($"{prefab} pool is too small.");
                return GameObject.Instantiate(CreatePrefab());
            }
        }

        private GameObject CreatePrefab()
        {
            if (!prefab)
            {
                Log.PrintError($"{prefab} is null. Pool will return new GameObject.");
                return new GameObject();
            }

            var obj = GameObject.Instantiate(prefab);
            obj.AddComponent<PrefabPoolObject>().Pool = this;
            return obj;
        }

        private void AddGameObject(GameObject obj)
        {
            if (objects.Contains(obj))
            {
                Log.Print($"{obj.name} already exist in {prefab} pool.");
                return;
            }
            else
            {
                objects.Add(obj);
                infoMap.Add(obj, new PoolObjectInfo());
                OnGameObjectAdded(obj);
                size++;
            }
        }

        private bool EnoughObject(int size)
        {
            if (autoExpand)
                return true;
            else
                return objects.Count - spawnedObjects.Count >= size;
        }

        private void OnGameObjectAdded(GameObject obj)
        {
            obj.name = prefab.name;
            obj.transform.SetParent(poolRoot);
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(false);
        }

        private void OnGameObjectSpawn(GameObject obj)
        {
            obj.transform.SetParent(null);
            obj.SetActive(true);
        }

        private void OnGameObjectDespawn(GameObject obj)
        {
            obj.transform.SetParent(poolRoot);
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(false);
        }

        public override int GetSpawnedObjectCount()
        {
            return size - spawnedObjects.Count;
        }
    }
}
