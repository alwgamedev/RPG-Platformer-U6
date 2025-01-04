using System;

namespace RPGPlatformer.Core
{
    [Serializable]
    public class ObjectPoolData
    {
        public PoolableObject pooledObject;
        public int poolSize;
    }
}