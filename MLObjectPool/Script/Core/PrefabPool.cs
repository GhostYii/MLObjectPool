using System;
using UnityEngine;
using System.Collections.Generic;

namespace MLObjectPool
{
    [Serializable]
    public sealed class PrefabPool : PoolBase
    {
        private GameObject prefab = null;
        private Transform poolRoot = null;
        private List<GameObject> objects = new List<GameObject>();
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private Dictionary<GameObject, PoolObjectInfo> infoMap = new Dictionary<GameObject, PoolObjectInfo>();

        public PrefabPool(GameObject go, int size = 10, bool isExpand = true)
        {
            autoExpand = isExpand;

            GameObject obj = new GameObject("Prefab Pool");
            poolRoot = obj.transform;

            prefab = go;

            for (int i = 0; i < size; i++)
            {
                GameObject tmp = GameObject.Instantiate<GameObject>(go);
                AddGameObject(tmp);
            }
        }

        public GameObject Allocation()
        {
            GameObject go = GetGameObject();

            IPoolObjectBeforeHandler beforeHandler = null;
            if (go.TryGetComponent<IPoolObjectBeforeHandler>(out beforeHandler))
                beforeHandler.OnBeforeAllocation();

            if (infoMap.ContainsKey(go))
                infoMap[go].Allocation();

            IPoolObjectHandler handler = null;
            if (go.TryGetComponent<IPoolObjectHandler>(out handler))
                handler.OnAllocation();
            else
                OnGameObjectSpawn(go);

            IPoolObjectAfterHandler afterHandler = null;
            if (go.TryGetComponent<IPoolObjectAfterHandler>(out afterHandler))
                afterHandler.OnAfterAllocation();

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
                    IPoolObjectBeforeHandler beforeHandler = null;
                    if (obj.TryGetComponent<IPoolObjectBeforeHandler>(out beforeHandler))
                        beforeHandler.OnBeforeRecycle();

                    if (infoMap.ContainsKey(obj))
                        infoMap[obj].Recycle();
                    spawnedObjects.Remove(obj);

                    IPoolObjectHandler handler = null;
                    if (obj.TryGetComponent<IPoolObjectHandler>(out handler))
                        handler.OnRecycle();
                    else
                        OnGameObjectDespawn(obj);

                    IPoolObjectAfterHandler afterHandler = null;
                    if (obj.TryGetComponent<IPoolObjectAfterHandler>(out afterHandler))
                        afterHandler.OnAfterRecycle();

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

        private GameObject GetGameObject()
        {
            foreach (var obj in infoMap)
                if (obj.Value.isAvalible)
                    return obj.Key;

            if (autoExpand)
            {
                int tmpSize = size + 1;
                int createSize = size;
                for (int i = 0; i < createSize; i++)
                {
                    var obj = GameObject.Instantiate(prefab);
                    AddGameObject(obj);
                }
                return objects[tmpSize];
            }
            else
            {
                Log.PrintWarning($"{prefab} pool is too small.");
                return GameObject.Instantiate(prefab);
            }
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
            obj.transform.parent = poolRoot;
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
            obj.transform.parent = poolRoot;
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
