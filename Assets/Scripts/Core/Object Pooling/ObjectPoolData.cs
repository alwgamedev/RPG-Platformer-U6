using System;

namespace RPGPlatformer.Core
{
    [Serializable]
    public struct ObjectPoolData
    {
        public PoolableObject pooledObject;
        public int poolSize;
        public UnityEngine.Object configurationParameters;
    }
}