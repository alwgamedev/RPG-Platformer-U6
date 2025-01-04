using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ObjectPoolCollection : MonoBehaviour
    {
        public List<ObjectPoolData> poolData = new();
        public Dictionary<PoolableObject, ObjectPool> findObjectPool;
        public Dictionary<string, ObjectPool> findObjectPoolByName;

        private void Awake()
        {
            Configure();
        }

        void Configure()
        {
            findObjectPool = new();
            findObjectPoolByName = new();

            foreach (var data in poolData)
            {
                GameObject carrier = new();
                carrier.transform.parent = transform;
                ObjectPool op = carrier.AddComponent<ObjectPool>();
                op.pooledObject = data.pooledObject;
                op.poolSize = data.poolSize;
                op.GeneratePool();
                findObjectPool[data.pooledObject] = op;
                findObjectPoolByName[data.pooledObject.name] = op;
                //Debug.Log($"Added object pool dictionary key {data.pooledObject.name}");
            }
        }

        public PoolableObject GetObject(PoolableObject prefab)
        {
            if (findObjectPool.TryGetValue(prefab, out ObjectPool pool))
            {
                return pool?.GetObject();
            }
            Debug.Log($"Unable to find an object pool for prefab named {prefab.name}");
            return null;
        }

        public PoolableObject GetObject(string prefabName)
        {
            if (findObjectPoolByName.TryGetValue(prefabName, out ObjectPool pool))
            {
                return pool?.GetObject();
            }
            Debug.Log($"Unable to find an object pool by name {prefabName}");
            return null;
        }
    }
}