using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ObjectPool : MonoBehaviour, IObjectPool
    {
        public PoolableObject pooledObject;
        public int poolSize;
        public bool generateOnAwake;
        public Object configurationParameters;
        //^use e.g. if the object depends on something in the scene that can't be added to prefab
        //(like patroller needing patrol bounds)

        Queue<PoolableObject> pool = new();

        public int Available => pool.Count;

        private void Awake()
        {
            if (generateOnAwake)
            {
                GeneratePool();
            }
        }

        public PoolableObject GetObject()
        {
            //don't think lock is necessary for us; we don't have any multithreading
            //but if we do in the future, then we don't have to worry about forgetting to add this
            lock (pool)
            {
                if (pool.Count == 0)
                {
                    return InstantiatePooledObject(configurationParameters);
                }

                PoolableObject item = pool.Dequeue();
                item.BeforeSetActive();
                item.gameObject.SetActive(true);
                return item;
            }
        }

        public void ReturnObject(PoolableObject item)
        {
            lock (pool)
            {
                AddToQueue(item);
            }
        }
        PoolableObject InstantiatePooledObject(object configurationParameters)
        {
            var o = Instantiate(pooledObject);
            o.Configure(configurationParameters);
            return o;
        }

        void AddToQueue(PoolableObject item)
        {
            if (!item) return;
            item.transform.SetParent(transform);
            //item.source = this;
            item.OnEnqueued(this);
            pool.Enqueue(item);
            item.gameObject.SetActive(false);
        }

        public void GeneratePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                AddToQueue(InstantiatePooledObject(configurationParameters));
            }
        }
    }
}