using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private readonly List<PoolBase> pools = new List<PoolBase>();
        private readonly Dictionary<string, PoolBase> poolMap = new Dictionary<string, PoolBase>();
        private readonly Dictionary<PoolBase, string> poolNames = new Dictionary<PoolBase, string>();
        private PrefabPoolRoot poolRoot;

        /// <summary>
        /// 所有对象池
        /// </summary>
        public List<PoolBase> Pools => pools;

        /// <summary>
        /// 所有对象池名称
        /// </summary>
        public List<string> PoolNames => new List<string>(poolMap.Keys);

        public PrefabPool CreatePrefabPool(string name, GameObject go, int size = 0, bool autoExpand = true)
        {
            if (go == null)
            {
                Log.PrintError($"Create prefab pool {name} failed: prefab is null.");
                return null;
            }

            if (size < 0)
            {
                Log.PrintError($"Create prefab pool {name} failed: size must be positive.");
                return null;
            }

            if (!CanRegister(name))
                return null;

            var pool = new PrefabPool(go, EnsurePoolRoot(), size, autoExpand);
            RegisterPool(name, pool);
            return pool;
        }

        public Pool<T> CreatePool<T>(string name, int size = 0, bool autoExpand = true) where T : new()
        {
            return CreatePool<T>(name, size, autoExpand, null);
        }

        public Pool<T> CreatePool<T>(string name, int size, bool autoExpand, Func<T> factory) where T : new()
        {
            if (size < 0)
            {
                Log.PrintError($"Create pool {name} failed: size must be positive.");
                return null;
            }

            if (!CanRegister(name))
                return null;

            var pool = new Pool<T>(size, autoExpand, factory);
            RegisterPool(name, pool);
            return pool;
        }

        public void AddPool(string name, PoolBase pool)
        {
            if (pool == null)
            {
                Log.PrintError($"Add pool {name} failed: pool is null.");
                return;
            }

            if (!CanRegister(name))
                return;

            if (poolNames.ContainsKey(pool))
            {
                Log.PrintError($"Pool {name} has already been registered as {poolNames[pool]}.");
                return;
            }

            RegisterPool(name, pool);
        }

        public PoolBase FindPoolByName(string name)
        {
            if (!poolMap.TryGetValue(name, out var pool))
            {
                Log.Print($"Pool {name} does not exist.");
                return null;
            }

            return pool;
        }

        public bool TryGetPool<T>(string name, out Pool<T> pool) where T : new()
        {
            if (poolMap.TryGetValue(name, out var basePool))
            {
                pool = basePool as Pool<T>;
                if (pool == null)
                    Log.PrintWarning($"Pool {name} is not a Pool<{typeof(T)}>.");

                return pool != null;
            }

            pool = null;
            return false;
        }

        public object AllocationFromPool(string name)
        {
            if (!poolMap.TryGetValue(name, out var pool))
            {
                Log.PrintWarning($"Pool {name} does not exist.");
                return null;
            }

            return pool.Allocation(true);
        }

        public bool RecycleFromPool<T>(string name, T obj)
        {
            if (!poolMap.TryGetValue(name, out var pool))
            {
                Log.PrintWarning($"Pool {name} does not exist.");
                return false;
            }

            return pool.Recycle(obj, typeof(T));
        }

        public void RemovePool(string name)
        {
            if (!poolMap.TryGetValue(name, out var pool))
                return;

            pool.RecycleAll();
            pool.Clear();

            pools.Remove(pool);
            poolMap.Remove(name);
            poolNames.Remove(pool);
        }

        public void RemovePool(PoolBase pool)
        {
            if (pool == null || !poolNames.TryGetValue(pool, out var name))
                return;

            RemovePool(name);
        }

        private bool CanRegister(string name)
        {
            if (poolMap.ContainsKey(name))
            {
                Log.PrintError($"Pool {name} already exists.");
                return false;
            }

            return true;
        }

        private void RegisterPool(string name, PoolBase pool)
        {
            pool.Name = name;
            pools.Add(pool);
            poolMap.Add(name, pool);
            poolNames.Add(pool, name);
        }

        private PrefabPoolRoot EnsurePoolRoot()
        {
            if (poolRoot != null)
                return poolRoot;

            var rootGo = new GameObject("Prefab Pool");
            rootGo.transform.SetParent(transform, false);
            poolRoot = rootGo.AddComponent<PrefabPoolRoot>();
            return poolRoot;
        }

        private void OnDestroy()
        {
            foreach (var pool in pools)
            {
                pool.RecycleAll();
                pool.Clear();
            }

            pools.Clear();
            poolMap.Clear();
            poolNames.Clear();

            if (poolRoot != null)
                Destroy(poolRoot.gameObject);

            poolRoot = null;
        }
    }
}
