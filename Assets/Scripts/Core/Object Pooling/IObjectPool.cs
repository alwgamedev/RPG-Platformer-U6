namespace RPGPlatformer.Core
{
    public interface IObjectPool
    {
        public PoolableObject GetObject();
        public void ReturnObject(PoolableObject item);
    }
}
