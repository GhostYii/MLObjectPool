using System;
using System.Collections.Generic;

namespace MLObjectPool
{
    public sealed class Pool<T> : PoolBase where T : new()
    {
        private readonly Stack<T> _available = new Stack<T>();
        private readonly HashSet<T> _active = new HashSet<T>();
        private readonly HashSet<T> _known = new HashSet<T>();
        private readonly Func<T> _factory;

        public Pool(int defaultSize, bool autoExpand = false) : this(defaultSize, autoExpand, null)
        {
        }

        public Pool(int defaultSize, bool autoExpand, Func<T> factory)
        {
            if (defaultSize < 0)
                throw new ArgumentOutOfRangeException(nameof(defaultSize));

            size = defaultSize;
            this.autoExpand = autoExpand;
            _factory = factory ?? (() => new T());

            for (int i = 0; i < size; i++)
                AddPoolObject(_factory());
        }

        public override int AvailableObjectCount => _available.Count;
        public override int ActiveObjectCount => _active.Count;

        public void AddPoolObject(T obj)
        {
            if (ReferenceEquals(obj, null))
            {
                Log.PrintError($"{typeof(T)} pool can not add null object.");
                return;
            }

            if (!_known.Add(obj))
            {
                Log.Print($"{obj} already exists in {typeof(T)} pool.");
                return;
            }

            _available.Push(obj);
            size = _known.Count;
        }

        public override object Allocation(bool isExpand)
        {
            if (isExpand && _available.Count == 0)
                Expand(GetExpandCount());

            return Allocation();
        }

        public T Allocation()
        {
            T obj;
            return TryAllocation(out obj) ? obj : default(T);
        }

        public bool TryAllocation(out T obj)
        {
            if (_available.Count == 0)
            {
                if (autoExpand)
                    Expand(GetExpandCount());
                else
                {
                    Log.PrintWarning($"{typeof(T)} pool is too small.");
                    obj = default(T);
                    return false;
                }
            }

            obj = PopAvailable();
            _active.Add(obj);

            if (obj is IBeforeAllocationHandler before)
                before.OnBeforeAllocation(this);

            if (obj is IAllocationHandler allocation)
                allocation.OnAllocation(this);

            if (obj is IAfterAllocationHandler after)
                after.OnAfterAllocation(this);

            return true;
        }

        public T[] Allocation(int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (!autoExpand && _available.Count < size)
            {
                Log.PrintWarning($"{typeof(T)} pool is too small.");
                return null;
            }

            var result = new T[size];
            for (int i = 0; i < size; i++)
            {
                if (!TryAllocation(out result[i]))
                    return null;
            }

            return result;
        }

        public override bool Recycle(object obj, Type type)
        {
            if (type == null || !typeof(T).IsAssignableFrom(type))
                return false;

            return Recycle((T)obj);
        }

        public bool Recycle(T obj)
        {
            if (ReferenceEquals(obj, null))
            {
                Log.PrintWarning($"{typeof(T)} pool can not recycle null object.");
                return false;
            }

            if (!_known.Contains(obj))
            {
                Log.PrintWarning($"{obj} is not exist in {typeof(T)} pool.");
                return false;
            }

            if (!_active.Contains(obj))
            {
                Log.PrintWarning($"{obj} is not exist in {typeof(T)} pool cache.");
                return false;
            }

            if (obj is IBeforeRecycleHandler before)
                before.OnBeforeRecycle(this);

            if (obj is IRecycleHandler recycle)
                recycle.OnRecycle(this);

            _active.Remove(obj);
            _available.Push(obj);

            if (obj is IAfterRecycleHandler after)
                after.OnAfterRecycle(this);

            return true;
        }

        public bool Recycle(T[] objs)
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
            var snapshot = new T[_active.Count];
            _active.CopyTo(snapshot);

            foreach (var obj in snapshot)
                allRecycled = Recycle(obj) && allRecycled;

            return allRecycled;
        }

        public override void Clear()
        {
            RecycleAll();
            _available.Clear();
            _active.Clear();
            _known.Clear();
            size = 0;
        }

        private void Expand(int count)
        {
            for (int i = 0; i < count; i++)
                AddPoolObject(_factory());
        }

        private int GetExpandCount()
        {
            return Math.Max(size, 1);
        }

        private T PopAvailable()
        {
            while (_available.Count > 0)
            {
                var obj = _available.Pop();
                if (!ReferenceEquals(obj, null))
                    return obj;
            }

            return default(T);
        }
    }
}
