using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLObjectPool
{
    [Serializable]
    public sealed class PrefabPool : PoolBase
    {
        private readonly Stack<GameObject> _available = new Stack<GameObject>();
        private readonly HashSet<GameObject> _active = new HashSet<GameObject>();
        private readonly HashSet<GameObject> _known = new HashSet<GameObject>();
        private readonly PrefabPoolRoot _root;
        private GameObject _prefab;

        internal PrefabPool(GameObject go, PrefabPoolRoot root, int size = 10, bool isExpand = true)
        {
            if (go == null)
                throw new ArgumentNullException(nameof(go));
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _prefab = go;
            _root = root;
            autoExpand = isExpand;

            for (int i = 0; i < size; i++)
                AddInstance(CreateInstance());
        }

        public override int AvailableObjectCount => _available.Count;
        public override int ActiveObjectCount => _active.Count;

        public GameObject Allocation()
        {
            GameObject go;
            return TryAllocation(out go) ? go : null;
        }

        public bool TryAllocation(out GameObject go)
        {
            if (_available.Count == 0)
            {
                if (autoExpand)
                    Expand(GetExpandCount());
                else
                {
                    Log.PrintWarning($"{_prefab.name} pool is too small.");
                    go = null;
                    return false;
                }
            }

            go = PopAvailable();
            return HandleAllocation(go) != null;
        }

        public override object Allocation(bool isExpand)
        {
            if (isExpand && _available.Count == 0)
                Expand(GetExpandCount());

            return Allocation();
        }

        public GameObject[] Allocation(int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (!autoExpand && _available.Count < size)
            {
                Log.PrintWarning($"{_prefab.name} pool is too small.");
                return null;
            }

            var result = new GameObject[size];
            for (int i = 0; i < size; i++)
            {
                if (!TryAllocation(out result[i]))
                    return null;
            }

            return result;
        }

        public void AllocationAsync(Action<GameObject> callback)
        {
            GameObject go;
            if (TryPopAvailable(out go))
            {
                callback?.Invoke(HandleAllocation(go));
                return;
            }

            if (!autoExpand)
            {
                Log.PrintWarning($"{_prefab.name} pool is too small.");
                callback?.Invoke(null);
                return;
            }

            StartCreateAsync(GetExpandCount(), gos =>
            {
                foreach (var obj in gos)
                    AddInstance(obj);

                callback?.Invoke(TryPopAvailable(out go) ? HandleAllocation(go) : null);
            });
        }

        public void AllocationAsync(int size, Action<GameObject[]> callback)
        {
            if (size < 0)
            {
                Log.PrintError($"{_prefab.name} pool can not allocation {size} object.");
                callback?.Invoke(null);
                return;
            }

            if (size == 0)
            {
                callback?.Invoke(new GameObject[0]);
                return;
            }

            if (!autoExpand && _available.Count < size)
            {
                Log.PrintWarning($"{_prefab.name} pool is too small.");
                callback?.Invoke(null);
                return;
            }

            var result = new List<GameObject>(size);
            while (result.Count < size && _available.Count > 0)
                result.Add(PopAvailable());

            if (result.Count == size)
            {
                InvokeBatchAllocation(result, callback);
                return;
            }

            StartCreateAsync(size - result.Count, gos =>
            {
                foreach (var obj in gos)
                    AddInstance(obj);

                while (result.Count < size && _available.Count > 0)
                    result.Add(PopAvailable());

                if (result.Count < size)
                {
                    Log.PrintWarning($"{_prefab.name} pool is too small.");
                    callback?.Invoke(null);
                    return;
                }

                InvokeBatchAllocation(result, callback);
            });
        }

        public override bool Recycle(object obj, Type type)
        {
            if (type == null || type != typeof(GameObject))
                return false;

            return Recycle((GameObject)obj);
        }

        public bool Recycle(GameObject obj)
        {
            if (obj == null)
            {
                Log.PrintWarning($"{_prefab.name} pool can not recycle null object.");
                return false;
            }

            if (!_known.Contains(obj))
            {
                Log.PrintWarning($"{obj} is not exist in {_prefab.name} pool.");
                return false;
            }

            if (!_active.Contains(obj))
            {
                Log.PrintWarning($"{obj} is not exist in {_prefab.name} pool cache.");
                return false;
            }

            var marker = obj.GetComponent<PrefabPoolObject>();
            InvokePoolEvent(marker, obj, EventTriggerType.BeforeRecycle);

            bool recycleHandled = InvokePoolEvent(marker, obj, EventTriggerType.Recycle);
            if (!recycleHandled)
                OnGameObjectDespawn(obj);

            InvokePoolEvent(marker, obj, EventTriggerType.AfterRecycle);

            _active.Remove(obj);
            _available.Push(obj);

            if (marker != null)
                marker.ClearEvents();

            return true;
        }

        public bool Recycle(GameObject[] objs)
        {
            if (objs == null)
                return false;

            bool allRecycled = true;
            foreach (var obj in objs)
                allRecycled = Recycle(obj) && allRecycled;

            return allRecycled;
        }

        public override bool RecycleAll()
        {
            bool allRecycled = true;
            var snapshot = new GameObject[_active.Count];
            _active.CopyTo(snapshot);

            foreach (var obj in snapshot)
                allRecycled = Recycle(obj) && allRecycled;

            return allRecycled;
        }

        public override void Clear()
        {
            RecycleAll();

            var snapshot = new GameObject[_known.Count];
            _known.CopyTo(snapshot);

            foreach (var go in snapshot)
            {
                if (go != null)
                    UnityEngine.Object.Destroy(go);
            }

            _available.Clear();
            _active.Clear();
            _known.Clear();
            size = 0;
        }

        private GameObject HandleAllocation(GameObject go)
        {
            if (go == null)
                return null;

            var marker = go.GetComponent<PrefabPoolObject>();
            InvokePoolEvent(marker, go, EventTriggerType.BeforeAllocation);

            bool allocationHandled = InvokePoolEvent(marker, go, EventTriggerType.Allocation);
            if (!allocationHandled)
                OnGameObjectSpawn(go);

            InvokePoolEvent(marker, go, EventTriggerType.AfterAllocation);

            _active.Add(go);
            return go;
        }

        private void InvokeBatchAllocation(List<GameObject> objects, Action<GameObject[]> callback)
        {
            foreach (var go in objects)
                HandleAllocation(go);

            callback?.Invoke(objects.ToArray());
        }

        private bool InvokePoolEvent(PrefabPoolObject marker, GameObject go, EventTriggerType type)
        {
            if (marker != null && marker.TryInvoke(type, this))
                return true;

            return InvokeInterfaceEvent(go, type);
        }

        private bool InvokeInterfaceEvent(GameObject go, EventTriggerType type)
        {
            switch (type)
            {
                case EventTriggerType.BeforeAllocation:
                    if (go.TryGetComponent<IBeforeAllocationHandler>(out var beforeAllocation))
                    {
                        beforeAllocation.OnBeforeAllocation(this);
                        return true;
                    }
                    break;
                case EventTriggerType.Allocation:
                    if (go.TryGetComponent<IAllocationHandler>(out var allocation))
                    {
                        allocation.OnAllocation(this);
                        return true;
                    }
                    break;
                case EventTriggerType.AfterAllocation:
                    if (go.TryGetComponent<IAfterAllocationHandler>(out var afterAllocation))
                    {
                        afterAllocation.OnAfterAllocation(this);
                        return true;
                    }
                    break;
                case EventTriggerType.BeforeRecycle:
                    if (go.TryGetComponent<IBeforeRecycleHandler>(out var beforeRecycle))
                    {
                        beforeRecycle.OnBeforeRecycle(this);
                        return true;
                    }
                    break;
                case EventTriggerType.Recycle:
                    if (go.TryGetComponent<IRecycleHandler>(out var recycle))
                    {
                        recycle.OnRecycle(this);
                        return true;
                    }
                    break;
                case EventTriggerType.AfterRecycle:
                    if (go.TryGetComponent<IAfterRecycleHandler>(out var afterRecycle))
                    {
                        afterRecycle.OnAfterRecycle(this);
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void StartCreateAsync(int count, Action<GameObject[]> callback)
        {
            _root.StartCoroutine(CreateInstancesAsync(count, callback));
        }

        private IEnumerator CreateInstancesAsync(int count, Action<GameObject[]> callback)
        {
            if (count <= 0)
            {
                callback?.Invoke(new GameObject[0]);
                yield break;
            }

            var operation = UnityEngine.Object.InstantiateAsync(_prefab, count);
            yield return operation;

            var results = operation.Result;
            foreach (var go in results)
            {
                var marker = go.AddComponent<PrefabPoolObject>();
                marker.Pool = this;
            }

            callback?.Invoke(results);
        }

        private GameObject CreateInstance()
        {
            var go = UnityEngine.Object.Instantiate(_prefab);
            var marker = go.AddComponent<PrefabPoolObject>();
            marker.Pool = this;
            return go;
        }

        private void AddInstance(GameObject obj)
        {
            if (obj == null)
                return;

            if (!_known.Add(obj))
            {
                Log.Print($"{obj.name} already exists in {_prefab.name} pool.");
                return;
            }

            PrepareObject(obj);
            _available.Push(obj);
            size = _known.Count;
        }

        private bool TryPopAvailable(out GameObject go)
        {
            while (_available.Count > 0)
            {
                go = _available.Pop();
                if (go != null)
                    return true;
            }

            go = null;
            return false;
        }

        private void Expand(int count)
        {
            for (int i = 0; i < count; i++)
                AddInstance(CreateInstance());
        }

        private int GetExpandCount()
        {
            return Math.Max(size, 1);
        }

        private void PrepareObject(GameObject obj)
        {
            obj.name = _prefab.name;
            obj.transform.SetParent(_root.transform, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
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

            obj.transform.SetParent(_root.transform, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(false);
        }
    }
}
