using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ObjectPool : MonoBehaviour, IObjectPool
    {
        public PoolableObject pooledObject;
        public int poolSize;
        public bool generateOnAwake;

        Queue<PoolableObject> pool = new();

        private void Awake()
        {
            if (generateOnAwake)
            {
                GeneratePool();
            }
        }

        public PoolableObject GetObject()
        {
            if (pool.Count == 0)
            {
                return InstantiatePooledObject();
            }

            PoolableObject item = pool.Dequeue();
            item.gameObject.SetActive(true);
            return item;
            //lock (pool)
            //{
            //    if (pool.Count == 0)
            //    {
            //        return InstantiatePooledObject();
            //    }

            //    PoolableObject item = pool.Dequeue();
            //    item.gameObject.SetActive(true);
            //    return item;
            //}
        }

        public void ReturnObject(PoolableObject item)
        {
            lock (pool)
            {
                AddToQueue(item);
            }
        }
        PoolableObject InstantiatePooledObject()
        {
            return Instantiate(pooledObject);
        }

        void AddToQueue(PoolableObject item)
        {
            if (!item) return;
            item.transform.SetParent(transform);
            item.source = this;
            pool.Enqueue(item);
            item.gameObject.SetActive(false);
        }

        public void GeneratePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var o = InstantiatePooledObject();
                o.Configure(this);
                AddToQueue(o);
            }
        }
    }
}