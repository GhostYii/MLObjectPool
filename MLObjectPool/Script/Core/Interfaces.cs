namespace MLObjectPool
{
    public interface IAllocationHanlder
    {
        void OnAllocation(PoolBase pool);
    }

    public interface IRecycleHandler
    {
        void OnRecycle(PoolBase pool);
    }

    public interface IBeforeAllocationHandler
    {
        void OnBeforeAllocation(PoolBase pool);
    }

    public interface IBeforeRecycleHandler
    {
        void OnBeforeRecycle(PoolBase pool);
    }

    public interface IAfterAllocationHandler
    {
        void OnAfterAllocation(PoolBase pool);
    }

    public interface IAfterRecycleHandler
    {
        void OnAfterRecycle(PoolBase pool);
    }

    public interface IPoolObjectBeforeHandler
    {
        void OnBeforeAllocation();
        void OnBeforeRecycle();
    }

    public interface IPoolObjectHandler
    {
        void OnAllocation();
        void OnRecycle();
    }

    public interface IPoolObjectAfterHandler
    {
        void OnAfterAllocation();
        void OnAfterRecycle();
    }
}
