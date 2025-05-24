using RPGPlatformer.Core;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace RPGPlatformer.Environment
{
    public class FallingStalactiteGroup : MonoBehaviour
    {
        [SerializeField] ObjectPoolData poolData;
        [SerializeField] RandomizableVector2 spawnPosition;
        [SerializeField] RandomizableInt spawnWhenActiveBelow;
        [SerializeField] RandomizableInt goalActive;
        [SerializeField] RandomizableFloat timeBetweenSpawns;
        [SerializeField] RandomizableFloat timeBetweenDrops;

        bool playerInBounds;
        Collider2D ceiling;
        ObjectPool pool;
        Queue<FallingStalactite> active = new();

        Task dropCycle;

        event Action PlayerEnteredBounds;
        event Action PlayerExitedBounds;

        private void Awake()
        {
            ceiling = ((GameObject)poolData.ConfigurationParameters).GetComponent<Collider2D>();
            pool = ObjectPoolCollection.AddPoolAsChild(poolData, transform);
            pool.GeneratePool();
        }

        private void Start()
        {
            InitializeActive();
            PlayerEnteredBounds += OnPlayerEnteredBounds;
        }


        //SPAWNING

        private void InitializeActive()
        {
            for (int i = 0; i < goalActive.Value; i++)
            {
                SpawnStalactite();
            }
        }

        private async Task RefreshActive(CancellationToken token)
        {
            var g = goalActive.Value;
            g -= active.Count;
            for (int i = 0; i < g; i++)
            {
                await MiscTools.DelayGameTime(timeBetweenSpawns.Value, token);
                SpawnStalactite();
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


        //DROPPING

        private async void OnPlayerEnteredBounds()
        {
            if (dropCycle == null || dropCycle.IsCompleted)
            {
                using var cts = CancellationTokenSource
                    .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

                //dropCycle task will complete when playerInBounds = false,
                //but this allows us to end the cycle immediately rather than at beginning of
                //next iteration
                try
                {
                    PlayerExitedBounds += cts.Cancel;
                    dropCycle = DropCycle(cts.Token);
                    await dropCycle;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                finally
                {
                    PlayerExitedBounds -= cts.Cancel;
                }
            }
        }

        private async Task DropCycle(CancellationToken token)
        {
            while (playerInBounds)
            {
                DropStalactite(token);
                await MiscTools.DelayGameTime(timeBetweenDrops.Value, token);
            }
        }

        private void DropStalactite(CancellationToken token)
        {
            if (active.Count != 0)
            {
                active.Dequeue().Trigger();
            }

            if (active.Count < spawnWhenActiveBelow.Value)
            {
                //await RefreshActive(token);
                dropCycle = Task.WhenAll(dropCycle, RefreshActive(token));
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            if (!playerInBounds && collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                playerInBounds = true;
                PlayerEnteredBounds?.Invoke();
            }
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            if (!playerInBounds && collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                playerInBounds = true;
                PlayerEnteredBounds?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            if (playerInBounds && collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                playerInBounds = false;
                PlayerExitedBounds?.Invoke();
            }
        }

        private void OnDestroy()
        {
            PlayerEnteredBounds = null;
            PlayerExitedBounds = null;
        }
    }
}