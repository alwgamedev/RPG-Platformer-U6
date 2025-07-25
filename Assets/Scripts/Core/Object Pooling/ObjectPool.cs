﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
                FillPool();
            }
        }

        public PoolableObject ReleaseObject(Vector3? position = null)
        {
            lock (pool)
            {
                PoolableObject item = pool.Count != 0 ?
                    pool.Dequeue() : InstantiatePooledObject(poolData.ConfigurationParameters);
                if (position.HasValue)
                {
                    item.SetPosition(position.Value);
                }
                item.BeforeSetActive();
                item.gameObject.SetActive(true);
                item.AfterSetActive();
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
            item.OnEnqueued(this);
            pool.Enqueue(item);
            item.gameObject.SetActive(false);
        }

        public void AddToPool(int q)
        {
            for (int i = 0; i < q; i++)
            {
                AddToQueue(InstantiatePooledObject(poolData.ConfigurationParameters));
            }
        }

        public void FillPool()
        {
            AddToPool(poolData.PoolSize - Available);
        }

        public async Task FillPoolAsync(int quantityPerFrame, float dt, CancellationToken token)
        {
            while (Available < poolData.PoolSize)
            {
                AddToPool(Math.Min(poolData.PoolSize - Available, quantityPerFrame));
                await MiscTools.DelayGameTime(dt, token);
            }
        }
    }
}