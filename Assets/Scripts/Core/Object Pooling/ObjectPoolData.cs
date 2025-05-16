using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [Serializable]
    public struct ObjectPoolData
    {
        [SerializeField] PoolableObject pooledObject;
        [SerializeField] int poolSize;
        [SerializeField] UnityEngine.Object configurationParameters;

        public PoolableObject PooledObject => pooledObject;
        public int PoolSize => poolSize;
        public UnityEngine.Object ConfigurationParameters => configurationParameters;
    }
}