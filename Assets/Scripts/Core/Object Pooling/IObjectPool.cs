namespace RPGPlatformer.Core
{
    public interface IObjectPool
    {
        public PoolableObject ReleaseObject();
        public void ReturnObject(PoolableObject item);
    }
}
