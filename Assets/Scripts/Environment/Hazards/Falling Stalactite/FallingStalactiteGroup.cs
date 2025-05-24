using RPGPlatformer.Core;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPGPlatformer.Environment
{
    public class FallingStalactiteGroup : MonoBehaviour
    {
        [SerializeField] ObjectPoolData poolData;
        [SerializeField] RandomizableVector2 spawnPosition;
        [SerializeField] RandomizableInt spawnWhenActiveBelow;
        [SerializeField] RandomizableInt goalActive;
        [SerializeField] RandomizableFloat timeBetweenSpawns;

        Collider2D ceiling;
        ObjectPool pool;
        Queue<FallingStalactite> active;

        private void Awake()
        {
            ceiling = ((GameObject)poolData.ConfigurationParameters).GetComponent<Collider2D>();
            pool = ObjectPoolCollection.AddPoolAsChild(poolData, transform);
            pool.GeneratePool();
        }

        private async Task RefreshActive()
        {
            var g = goalActive.Value;
            var toSpawn = g - active.Count;

            if (toSpawn > 0)
            {
                for (int i = 0; i < toSpawn; i++)
                {
                    await MiscTools.DelayGameTime(timeBetweenSpawns.Value, 
                        GlobalGameTools.Instance.TokenSource.Token);
                    SpawnStalactite();
                }
            }
        }

        private void SpawnStalactite()
        {
            if (pool.Available == 0)
            {
                pool.GeneratePool();
            }

            var p = ceiling.ClosestPoint(spawnPosition.Value);
            active.Enqueue((FallingStalactite)pool.ReleaseObject(p));
        }

        private async void DropStalactite()
        {
            if (active.Count != 0)
            {
                active.Dequeue().Trigger();
            }
            if (active.Count < goalActive.Value)
            {
                await RefreshActive();
            }
        }
    }
}