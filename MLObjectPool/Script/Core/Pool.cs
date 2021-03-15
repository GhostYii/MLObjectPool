using System;
using System.Collections.Generic;

namespace MLObjectPool
{
    public sealed class Pool<T> : PoolBase where T : new()
    {
        private List<T> objects = new List<T>();
        private List<T> spawnedObjects = new List<T>();
        private Dictionary<T, PoolObjectInfo> infoMap = new Dictionary<T, PoolObjectInfo>();

        internal Pool(int defaultSize, bool autoExpand = false)
        {
            if (defaultSize < 0)
            {
                Log.PrintError($"{typeof(T)} pool can not allocation {size} object.\npool size must be positive interger.");
                return;
            }

            size = defaultSize;
            this.autoExpand = autoExpand;
            for (int i = 0; i < size; i++)
                AddPoolObject(new T());

            spawnedObjects = new List<T>();
        }

        public void AddPoolObject(T obj)
        {
            if (objects.Contains(obj))
            {
                Log.Print($"{obj.ToString()} already exist in {typeof(T)} pool.");
                return;
            }
            else
            {
                objects.Add(obj);
                infoMap.Add(obj, new PoolObjectInfo());
                size++;
            }
        }

        public override object Allocation(bool isExpand)
        {
            if (EnoughObject(1))
                return Allocation();
            else
            {
                T obj = new T();

                if (obj is IBeforeAllocationHandler)
                    (obj as IBeforeAllocationHandler).OnBeforeAllocation(this);

                if (obj is IAllocationHanlder)
                    (obj as IAllocationHanlder).OnAllocation(this);

                if (obj is IAfterAllocationHandler)
                    (obj as IAfterAllocationHandler).OnAfterAllocation(this);

                return obj;
            }
        }

        public override bool Recycle(object obj, System.Type type)
        {
            if (type.Equals(typeof(T)))
            {
                foreach (var o in objects)
                    if (System.Object.ReferenceEquals(obj, o))
                        return Recycle(o);
                return false;
            }
            else
                return false;
        }

        public T Allocation()
        {
            T obj = GetObject();

            if (obj is IBeforeAllocationHandler)
                (obj as IBeforeAllocationHandler).OnBeforeAllocation(this);

            if (infoMap.ContainsKey(obj))
                infoMap[obj].Allocation();
            if (obj is IAllocationHanlder)
                (obj as IAllocationHanlder).OnAllocation(this);

            if (obj is IAfterAllocationHandler)
                (obj as IAfterAllocationHandler).OnAfterAllocation(this);

            spawnedObjects.Add(obj);

            return obj;
        }

        public T[] Allocation(int size)
        {
            T[] objs = new T[size];
            for (int i = 0; i < size; i++)
                objs[i] = Allocation();

            return objs;
        }

        public bool Recycle(T obj)
        {
            if (objects.Contains(obj))
            {
                if (spawnedObjects.Contains(obj))
                {
                    if (obj is IBeforeRecycleHandler)
                        (obj as IBeforeRecycleHandler).OnBeforeRecycle(this);

                    if (infoMap.ContainsKey(obj))
                        infoMap[obj].Recycle();
                    if (obj is IRecycleHandler)
                        (obj as IRecycleHandler).OnRecycle(this);

                    spawnedObjects.Remove(obj);

                    if (obj is IAfterRecycleHandler)
                        (obj as IAfterRecycleHandler).OnAfterRecycle(this);

                    return true;
                }
                else
                {
                    Log.PrintWarning($"{obj} is not exist in {typeof(T)} pool cache.");
                    return false;
                }
            }
            else
            {
                Log.PrintWarning($"{obj} is not exist in {typeof(T)} pool.");
                return false;
            }
        }

        /// <summary>
        /// 批量回收对象
        /// </summary>
        /// <returns>所有对象是否回收成功</returns>
        public bool Recycle(T[] objs)
        {
            bool allRecylced = true;
            foreach (var obj in objs)
            {
                allRecylced = allRecylced && Recycle(obj);
            }

            return allRecylced;
        }

        public bool RecycleAll()
        {
            bool ack = true;
            List<T> tmpLst = new List<T>(spawnedObjects);
            foreach (var obj in tmpLst)
                ack = Recycle(obj) && ack;

            return ack;
        }

        private T GetObject()
        {
            foreach (var obj in infoMap)
                if (obj.Value.isAvalible)
                    return obj.Key;

            if (autoExpand)
            {
                int tmpSize = size + 1;
                for (int i = 0; i < size; i++)
                    AddPoolObject(new T());

                return objects[tmpSize];
            }
            else
            {
                Log.PrintWarning($"{typeof(T)} pool is too small.");
                return new T();
            }
        }

        private bool EnoughObject(int size)
        {
            if (autoExpand)
                return true;
            else
                return objects.Count - spawnedObjects.Count >= size;
        }

        public override int GetSpawnedObjectCount()
        {
            return size - spawnedObjects.Count;
        }
    }
}

