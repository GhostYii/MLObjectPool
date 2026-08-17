using System;

namespace MLObjectPool
{
    public abstract class PoolBase
    {
        protected int size = 0;
        protected bool autoExpand = false;

        public string Name { get; internal set; }

        /// <summary>
        /// 是否在对象池不足时自动扩容
        /// </summary>
        public bool AutoExpand
        {
            get => autoExpand;
            set => autoExpand = value;
        }

        /// <summary>
        /// 池内已经创建的对象总数（容量）
        /// </summary>
        public int Size => size;

        /// <summary>
        /// 当前可分配对象数量
        /// </summary>
        public abstract int AvailableObjectCount { get; }

        /// <summary>
        /// 当前已分配且未回收的对象数量
        /// </summary>
        public abstract int ActiveObjectCount { get; }

        /// <summary>
        /// 分配一个对象。isExpand 表示本次分配是否允许临时扩容，不会持久修改 AutoExpand。
        /// </summary>
        public abstract object Allocation(bool isExpand);

        public abstract bool Recycle(object obj, Type type);
        public abstract bool RecycleAll();

        /// <summary>
        /// 清空对象池。PrefabPool 会销毁池内 GameObject。
        /// </summary>
        public virtual void Clear()
        {
        }

        public int GetAvailableObjectCount()
        {
            return AvailableObjectCount;
        }

        public int GetSpawnedObjectCount()
        {
            return ActiveObjectCount;
        }
    }
}
