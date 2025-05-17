using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ObjectPoolCollection : MonoBehaviour
    {
        [SerializeField] ObjectPoolData[] poolsData;
        
        Dictionary<PoolableObject, ObjectPool> FindObjectPool = new();
        Dictionary<string, ObjectPool> FindObjectPoolByName = new();

        private void Awake()
        {
            CreatePools(poolsData);
        }

        public void CreatePools(ObjectPoolData[] poolsData)
        {
            if (poolsData == null)
                return;

            foreach (var data in poolsData)
            {
                //GameObject carrier = new($"Object Pool: {data.PooledObject.name}");
                //carrier.transform.parent = transform;
                //ObjectPool op = carrier.AddComponent<ObjectPool>();
                //op.poolData = data;
                var op = AddPoolAsChild(data, transform);
                op.GeneratePool();
                FindObjectPool[data.PooledObject] = op;
                FindObjectPoolByName[data.PooledObject.name] = op;
            }
        }

        public static ObjectPool AddPoolAsChild(ObjectPoolData data, Transform parent)
        {
            var container = new GameObject($"Object Pool: {data.PooledObject.name}");
            container.transform.parent = parent;
            ObjectPool op = container.AddComponent<ObjectPool>();
            op.poolData = data;
            return op;
        }

        public PoolableObject GetObject(PoolableObject prefab)
        {
            if (FindObjectPool.TryGetValue(prefab, out ObjectPool pool))
            {
                if (pool)
                {
                    return pool.ReleaseObject();
                }
            }
            Debug.Log($"Unable to find an object pool for prefab named {prefab.name}");
            return null;
        }

        public PoolableObject GetObject(string prefabName)
        {
            if (FindObjectPoolByName.TryGetValue(prefabName, out ObjectPool pool))
            {
                if (pool)
                {
                    return pool.ReleaseObject();
                }
            }
            Debug.Log($"Unable to find an object pool by name {prefabName}");
            return null;
        }
    }
}