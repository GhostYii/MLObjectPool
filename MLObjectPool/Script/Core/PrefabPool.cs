using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace MLObjectPool
{
    [Serializable]
    public sealed class PrefabPool : PoolBase
    {
        private GameObject prefab = null;
        private List<GameObject> objects = new List<GameObject>();
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private Dictionary<GameObject, PoolObjectInfo> infoMap = new Dictionary<GameObject, PoolObjectInfo>();

        internal static PrefabPoolRoot poolRoot = null;

        internal PrefabPool(GameObject go, int size = 10, bool isExpand = true)
        {
            autoExpand = isExpand;
            poolRoot = poolRoot ?? new GameObject("Prefab Pool").AddComponent<PrefabPoolRoot>();
            prefab = go;

            for (int i = 0; i < size; i++)
            {
                AddGameObject(CreatePrefab());
            }
        }

        public GameObject Allocation()
        {
            return HandleGameObjectAllocation(GetGameObject());
        }

        public void AllocationAsync(Action<GameObject> callback)
        {
            GetGameObjectAsync(go =>
            {
                HandleGameObjectAllocation(go);
                callback?.Invoke(go);
            });
        }

        public GameObject[] Allocation(int size)
        {
            if (size < 0)
            {
                Log.PrintError($"{prefab} pool can not allocation {size} object.\npool size must be positive interger.");
                return null;
            }

            GameObject[] goArray = new GameObject[size];
            for (int i = 0; i < size; ++i)
                goArray[i] = Allocation();

            return goArray;
        }

        public void AllocationAsync(int size, Action<GameObject[]> callback)
        {
            if (size < 0)
            {
                Log.PrintError($"{prefab} pool can not allocation {size} object.\npool size must be positive interger.");
                return;
            }

            GetGameObjectAsync(size, gos =>
            {
                foreach (var go in gos)
                {
                    HandleGameObjectAllocation(go);
                }

                callback?.Invoke(gos);
            });
        }

        public override object Allocation(bool isExpand)
        {
            autoExpand = isExpand;
            return Allocation();
        }

        public bool Recycle(GameObject obj)
        {
            if (!obj)
                return false;

            if (objects.Contains(obj))
            {
                if (spawnedObjects.Contains(obj))
                {
                    if (obj.TryGetComponent<PrefabPoolObject>(out var script))
                    {
                        if (script.eventMap.ContainsKey(EventTriggerType.BeforeRecycle))
                            script.eventMap[EventTriggerType.BeforeRecycle].Invoke(this);
                        else if (obj.TryGetComponent<IBeforeRecycleHandler>(out var beforeHandler))
                            beforeHandler.OnBeforeRecycle(this);

                        if (infoMap.ContainsKey(obj))
                            infoMap[obj].Recycle();
                        spawnedObjects.Remove(obj);

                        if (script.eventMap.ContainsKey(EventTriggerType.Recycle))
                            script.eventMap[EventTriggerType.Recycle].Invoke(this);
                        else if (obj.TryGetComponent<IRecycleHandler>(out var handler))
                            handler.OnRecycle(this);
                        else
                            OnGameObjectDespawn(obj);

                        if (script.eventMap.ContainsKey(EventTriggerType.AfterRecycle))
                            script.eventMap[EventTriggerType.AfterRecycle].Invoke(this);
                        else if (obj.TryGetComponent<IAfterRecycleHandler>(out var afterHandler))
                            afterHandler.OnAfterRecycle(this);
                    }
                    else
                    {
                        if (obj.TryGetComponent<IBeforeRecycleHandler>(out var beforeHandler))
                            beforeHandler.OnBeforeRecycle(this);

                        if (infoMap.ContainsKey(obj))
                            infoMap[obj].Recycle();
                        spawnedObjects.Remove(obj);

                        if (obj.TryGetComponent<IRecycleHandler>(out var handler))
                            handler.OnRecycle(this);
                        else
                            OnGameObjectDespawn(obj);

                        if (obj.TryGetComponent<IAfterRecycleHandler>(out var afterHandler))
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
                int createSize = Math.Max(size, 1);
                for (int i = 0; i < createSize; i++)
                {
                    AddGameObject(CreatePrefab());
                }
                return objects[size - 1];
            }
            else
            {
                Log.PrintWarning($"{prefab} pool is too small.");
                return GameObject.Instantiate(CreatePrefab());
            }
        }

        private void GetGameObjectAsync(Action<GameObject> callback)
        {
            foreach (var obj in infoMap)
            {
                if (obj.Value.isAvalible)
                {
                    callback?.Invoke(obj.Key);
                    return;
                }
            }

            if (autoExpand)
            {
                poolRoot.StartCoroutine(CreatePrefabAsync(Math.Max(size, 1), gos =>
                {
                    foreach (var go in gos)
                    {
                        AddGameObject(go);
                    }

                    callback?.Invoke(objects[size - 1]);
                }));
            }
            else
            {
                Log.PrintWarning($"{prefab} pool is too small.");
                poolRoot.StartCoroutine(CreatePrefabAsync(1, gos => callback?.Invoke(gos[0])));
            }
        }

        private void GetGameObjectAsync(int size, Action<GameObject[]> callback)
        {
            List<GameObject> objs = new List<GameObject>();
            if (size == 0)
            {
                callback?.Invoke(objs.ToArray());
                return;
            }

            foreach (var obj in infoMap)
            {
                if (obj.Value.isAvalible)
                {
                    objs.Add(obj.Key);
                    if (objs.Count >= size)
                    {
                        callback?.Invoke(objs.ToArray());
                        return;
                    }
                }
            }

            poolRoot.StartCoroutine(CreatePrefabAsync(size - objs.Count, gos =>
            {
                foreach (var go in gos)
                {
                    AddGameObject(go);
                    objs.Add(go);
                }

                callback?.Invoke(objs.ToArray());
            }));
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

        private IEnumerator CreatePrefabAsync(int count, Action<GameObject[]> callback)
        {
            if (!prefab)
            {
                Log.PrintError($"{prefab} is null. Pool will return new GameObject.");
                callback?.Invoke(new GameObject[] { new GameObject() });
                yield return 0;
            }

            var asyncOp = GameObject.InstantiateAsync(prefab, count);
            yield return asyncOp;

            callback?.Invoke(asyncOp.Result);
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

        private GameObject HandleGameObjectAllocation(GameObject go)
        {
            if (go.TryGetComponent<PrefabPoolObject>(out var script))
            {
                if (script.eventMap.ContainsKey(EventTriggerType.BeforeAllocation))
                    script.eventMap[EventTriggerType.BeforeAllocation].Invoke(this);
                else if (go.TryGetComponent<IBeforeAllocationHandler>(out var beforeHandler))
                    beforeHandler.OnBeforeAllocation(this);

                if (infoMap.ContainsKey(go))
                    infoMap[go].Allocation();

                if (script.eventMap.ContainsKey(EventTriggerType.Allocation))
                    script.eventMap[EventTriggerType.Allocation].Invoke(this);
                else if (go.TryGetComponent<IAllocationHanlder>(out var handler))
                    handler.OnAllocation(this);
                else
                    OnGameObjectSpawn(go);

                if (script.eventMap.ContainsKey(EventTriggerType.AfterAllocation))
                    script.eventMap[EventTriggerType.AfterAllocation].Invoke(this);
                else if (go.TryGetComponent<IAfterAllocationHandler>(out var afterHandler))
                    afterHandler.OnAfterAllocation(this);
            }
            else
            {
                if (go.TryGetComponent<IBeforeAllocationHandler>(out var beforeHandler))
                    beforeHandler.OnBeforeAllocation(this);

                if (infoMap.ContainsKey(go))
                    infoMap[go].Allocation();

                if (go.TryGetComponent<IAllocationHanlder>(out var handler))
                    handler.OnAllocation(this);
                else
                    OnGameObjectSpawn(go);

                if (go.TryGetComponent<IAfterAllocationHandler>(out var afterHandler))
                    afterHandler.OnAfterAllocation(this);
            }

            spawnedObjects.Add(go);
            return go;
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
            obj.transform.SetParent(poolRoot.transform);
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
            if (obj == null)
                return;

            obj.transform.SetParent(poolRoot.transform);
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
