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
        [SerializeField] RandomizableInt goalActive;
        [SerializeField] RandomizableFloat timeBetweenSpawns;
        [SerializeField] RandomizableFloat timeBetweenDrops;

        int groundLayer;
        bool playerInBounds;
        Collider2D ceiling;
        ObjectPool pool;
        Queue<FallingStalactite> active = new();

        Task dropCycle;

        event Action PlayerEnteredBounds;

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("ground");
            ceiling = ((GameObject)poolData.ConfigurationParameters).GetComponent<Collider2D>();
            pool = ObjectPoolCollection.AddPoolAsChild(poolData, transform);
            pool.FillPool();
        }

        private void Start()
        {
            InitializeActive();
            PlayerEnteredBounds += OnPlayerEnteredBounds;
            GlobalGameTools.Instance.Player.OnDeath += OnPlayerDeath;
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
            if (pool.Available == 0)
            {
                dropCycle = Task.WhenAll(dropCycle, pool.FillPoolAsync(1, .1f, token));
                await Task.Yield();
            }

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
            var p = spawnPosition.Value;
            var r = Physics2D.Raycast(p, Vector2.up, Mathf.Infinity, groundLayer);
            if (r)
            {
                p = r.point;
            }
            else
            {
                p = ceiling.ClosestPoint(p);
            }
            active.Enqueue((FallingStalactite)pool.ReleaseObject(p));
        }


        //DROPPING

        private async void OnPlayerEnteredBounds()
        {
            if (dropCycle == null || dropCycle.IsCompleted)
            {
                using var cts = CancellationTokenSource
                    .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

                dropCycle = DropCycle(cts.Token);
                await dropCycle;
            }
        }

        private async Task DropCycle(CancellationToken token)
        {
            while (playerInBounds)
            {
                await DropStalactite(token);
                await MiscTools.DelayGameTime(timeBetweenDrops.Value, token);
            }
        }

        private async Task DropStalactite(CancellationToken token)
        {
            if (active.Count != 0)
            {
                active.Dequeue().Trigger();
            }

            if (active.Count == 0)
            {
                await RefreshActive(token);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            if (!playerInBounds && collider.transform == GlobalGameTools.Instance.PlayerTransform
                && !GlobalGameTools.Instance.PlayerIsDead)
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
                //PlayerExitedBounds?.Invoke();
            }
        }

        private void OnPlayerDeath()
        {
            if (playerInBounds)
            {
                playerInBounds = false;
            }
        }

        private void OnDestroy()
        {
            PlayerEnteredBounds = null;
        }
    }
}