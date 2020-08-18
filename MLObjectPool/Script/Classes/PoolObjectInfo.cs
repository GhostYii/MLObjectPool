namespace MLObjectPool
{
    internal class PoolObjectInfo
    {
        public bool isAvalible;

        public PoolObjectInfo()
        {
            isAvalible = true;
        }

        public void Allocation()
        {
            isAvalible = false;
        }

        public void Recycle()
        {
            isAvalible = true;
        }
    }
}

