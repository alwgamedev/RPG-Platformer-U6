using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ObjectPoolCollection : MonoBehaviour
    {
        [SerializeField] ObjectPoolData[] poolsData;

        //Dictionary<PoolableObject, ObjectPool> FindObjectPool = new();
        string[] keys;
        Dictionary<string, ObjectPool> FindObjectPoolByName = new();

        public int TotalReleased
        {
            get
            {
                int t = 0;
                foreach (var entry in FindObjectPoolByName)
                {
                    t += entry.Value.TotalReleased;
                }
                return t;
            }
        }

        public int TotalReturned
        {
            get
            {
                int t = 0;
                foreach (var entry in FindObjectPoolByName)
                {
                    t += entry.Value.TotalReturned;
                }
                return t;
            }
        }

        public int Active
        {
            get
            {
                int t = 0;
                foreach (var entry in FindObjectPoolByName)
                {
                    t += entry.Value.Active;
                }
                return t;
            }
        }

        private void Awake()
        {
            CreatePools(poolsData);
        }

        public void CreatePools(ObjectPoolData[] poolsData)
        {
            if (poolsData == null)
                return;

            keys = new string[poolsData.Length];

            for (int i = 0; i < poolsData.Length; i++)
            {
                //GameObject carrier = new($"Object Pool: {data.PooledObject.name}");
                //carrier.transform.parent = transform;
                //ObjectPool op = carrier.AddComponent<ObjectPool>();
                //op.poolData = data;

                var data = poolsData[i];
                var op = AddPoolAsChild(data, transform);
                op.FillPool();
                //FindObjectPool[data.PooledObject] = op;
                keys[i] = data.PooledObject.name;
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

        //public PoolableObject GetObject(PoolableObject prefab)
        //{
        //    if (FindObjectPool.TryGetValue(prefab, out ObjectPool pool))
        //    {
        //        if (pool)
        //        {
        //            return pool.ReleaseObject();
        //        }
        //    }
        //    Debug.Log($"Unable to find an object pool for prefab named {prefab.name}");
        //    return null;
        //}

        public PoolableObject ReleaseObject(string prefabName, Vector3? position = null)
        {
            if (FindObjectPoolByName.TryGetValue(prefabName, out ObjectPool pool))
            {
                if (pool)
                {
                    return pool.ReleaseObject(position);
                }
            }
            Debug.Log($"Unable to find an object pool by name {prefabName}");
            return null;
        }

        public PoolableObject ReleaseRandomObject(Vector3? position = null)
        {
            return ReleaseObject(keys[MiscTools.rng.Next(keys.Length)], position);
        }
    }
}