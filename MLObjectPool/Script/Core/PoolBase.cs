namespace MLObjectPool
{
    public abstract class PoolBase
    {
        protected int size = 0;
        protected bool autoExpand = false;

        /// <summary>
        /// 是否自动拓展对象池
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
