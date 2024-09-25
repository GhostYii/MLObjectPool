using System.Collections.Generic;
using UnityEngine;

namespace MLObjectPool
{
    [DisallowMultipleComponent]
    public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private List<PoolBase> pools = new List<PoolBase>();
        private Dictionary<string, PoolBase> poolMap = new Dictionary<string, PoolBase>();

        /// <summary>
        /// 所有对象池
        /// </summary>
        public List<PoolBase> Pools { get => pools; }
        /// <summary>
        /// 所有对象池名称
        /// </summary>
        public List<string> PoolNames { get => new List<string>(poolMap.Keys); }

        /// <summary>
        /// 创建预制体对象池
        /// </summary>
        /// <param name="name">对象池名称（不可重复）</param>
        /// <param name="go">对象池预制体</param>
        /// <param name="size">对象池大小</param>
        /// <param name="autoExpand">是否自动扩容</param>
        public PrefabPool CreatePrefabPool(string name, GameObject go, int size = 0, bool autoExpand = true)
        {
            if (poolMap.ContainsKey(name))
            {
                Log.PrintError($"{name} has already exist. please type other name for ObjectPool");
                return null;
            }

            PrefabPool pool = new PrefabPool(go, size, autoExpand);
            pools.Add(pool);
            poolMap.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个普通对象池
        /// </summary>
        /// <typeparam name="T">对象池对象类型</typeparam>
        /// <param name="name">对象池名称（不可重复）</param>
        /// <param name="size">对象次初始大小</param>
        /// <param name="autoExpand">是否自动扩容</param>
        public Pool<T> CreatePool<T>(string name, int size = 0, bool autoExpand = true) where T : new()
        {
            if (poolMap.ContainsKey(name))
            {
                Log.PrintError($"{name} has already exist. please type other name for ObjectPool");
                return null;
            }

            Pool<T> pool = new Pool<T>(size, autoExpand);
            pools.Add(pool);
            poolMap.Add(name, pool);

            return pool;
        }

        /// <summary>
        /// 通过名称查找对象池
        /// </summary>
        /// <param name="name">对象池名称（不可重复）</param>
        public PoolBase FindPoolByName(string name)
        {
            if (!poolMap.ContainsKey(name))
            {
                Log.Print($"{name} pool does not exists.");
                return null;
            }

            return poolMap[name];
        }

        /// <summary>
        /// 添加对象池
        /// </summary>
        /// <param name="name">对象池名称（不可重复）</param>
        /// <param name="pool">需要添加的对象池对象</param>
        public void AddPool(string name, PoolBase pool)
        {
            if (poolMap.ContainsKey(name))
            {
                Log.PrintError($"{name} has already exist. please type other name for ObjectPool");
                return;
            }

            pools.Add(pool);
            poolMap.Add(name, pool);
        }

        /// <summary>
        /// 从指定对象池中分配对象
        /// </summary>
        /// <param name="name">对象池名称</param>
        public object AllocationFromPool(string name)
        {
            if (!poolMap.ContainsKey(name))
            {
                Log.PrintWarning($"{name} pool does not exists.");
                return null;
            }

            return poolMap[name].Allocation(true);
        }

        /// <summary>
        /// 从指定对象池中回收对象
        /// </summary>
        /// <typeparam name="T">对象类型（需与对象池内对象类型一致）</typeparam>
        /// <param name="name">对象池名称</param>
        /// <param name="obj">需要回收的对象</param>
        public bool RecycleFromPool<T>(string name, T obj)
        {
            if (!poolMap.ContainsKey(name))
            {
                Log.PrintWarning($"{name} pool does not exists.");
                return false;
            }

            return poolMap[name].Recycle(obj, typeof(T));
        }

        private void OnDestroy()
        {
            // do some clean
            PrefabPool.poolRoot = null;
        }
    }
}
