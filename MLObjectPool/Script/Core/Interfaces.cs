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
}
