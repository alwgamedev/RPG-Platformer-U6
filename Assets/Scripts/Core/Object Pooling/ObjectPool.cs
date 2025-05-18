using System;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ObjectPool : MonoBehaviour, IObjectPool
    {
        public ObjectPoolData poolData;
        public bool generateOnAwake;

        Queue<PoolableObject> pool = new();

        public int Available => pool.Count;
        public int TotalReleased { get; private set; }
        public int TotalReturned { get; private set; }
        public int Active => TotalReleased - TotalReturned;
        //^note active can be inaccurate if
        //a) an item is returned to the pool that wasn't originally released from this pool
        //b) a released item is destroyed and never returned
        //ATM neither of these cases occurs, but keep it in mind

        private void Awake()
        {
            if (generateOnAwake)
            {
                GeneratePool();
            }
        }

        public PoolableObject ReleaseObject()
        {
            //don't think lock is necessary for us; we don't have any multithreading
            //but if we do in the future, then we don't have to worry about forgetting to add this
            lock (pool)
            {
                PoolableObject item = pool.Count != 0 ?
                    pool.Dequeue() : InstantiatePooledObject(poolData.ConfigurationParameters);
                item.BeforeSetActive();
                item.gameObject.SetActive(true);
                TotalReleased++;
                return item;
            }
        }

        public PoolableObject ReleaseObject(Vector3 position)
        {
            lock (pool)
            {
                PoolableObject item = pool.Count != 0 ?
                    pool.Dequeue() : InstantiatePooledObject(poolData.ConfigurationParameters);
                item.transform.position = position;
                item.BeforeSetActive();
                item.gameObject.SetActive(true);
                TotalReleased++;
                return item;
            }
        }

        public void ReturnObject(PoolableObject item)
        {
            lock (pool)
            {
                AddToQueue(item);
                TotalReturned++;
            }
        }
        PoolableObject InstantiatePooledObject(object configurationParameters)
        {
            var o = Instantiate(poolData.PooledObject);
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
            for (int i = 0; i < poolData.PoolSize; i++)
            {
                AddToQueue(InstantiatePooledObject(poolData.ConfigurationParameters));
            }
        }
    }
}