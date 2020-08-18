namespace MLObjectPool
{
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
