namespace MLObjectPool
{
    public abstract class PoolBase
    {
        protected bool autoExpand = true;
        protected int size = 0;

        /// <summary>
        /// 自动拓展对象池容量
        /// </summary>
        public bool AutoExpand { get => autoExpand; set => autoExpand = value; }
        /// <summary>
        /// 对象池当前容量
        /// </summary>
        public int Size { get => size; }

        public abstract object Allocation(bool isExpand);
        public abstract bool Recycle(object obj, System.Type type);
        public abstract int GetSpawnedObjectCount();
    }
}
