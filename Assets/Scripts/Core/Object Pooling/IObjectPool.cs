using UnityEngine;

namespace RPGPlatformer.Core
{
    public interface IObjectPool
    {
        public PoolableObject ReleaseObject(Vector3? position = null);

        public void ReturnObject(PoolableObject item);
    }
}
