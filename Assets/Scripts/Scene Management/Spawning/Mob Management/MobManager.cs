using UnityEngine;
using RPGPlatformer.Saving;
using System.Text.Json.Nodes;
using RPGPlatformer.Core;
using System.Text.Json;
using System;

namespace RPGPlatformer.SceneManagement
{
    public class MobManager : MonoBehaviour, ISavable
    {
        [SerializeField] MobData mobData;

        ObjectPoolCollection pools;
        bool mobDefeated;
        int currentSpawnRate;
        int ticksSinceLastSpawn;

        private void Awake()
        {
            //if you are worried about the performance hit on scene transitions (i.e. generating big pool on scene load), 
            //you can set poolsize = 0 and have it instantiate the poolable object as needed?
            SavingSystem.SceneLoadComplete += OnSceneLoadComplete;
        }

        protected virtual void OnSceneLoadComplete()
        {
            if (mobDefeated)
            {
                gameObject.SetActive(false);
            }
            else
            {
                pools = gameObject.AddComponent<ObjectPoolCollection>();
                pools.CreatePools(mobData.PoolData);
                GlobalGameTools.Instance.TickTimer.NewTick += OnNewTick;
            }

            SavingSystem.SceneLoadComplete -= OnSceneLoadComplete;
        }

        protected virtual void OnNewTick()
        {
            if (!gameObject.activeInHierarchy || !CanSpawn())
            {
                return;
            }

            if (MobDefeated())
            {
                mobDefeated = true;
                GlobalGameTools.Instance.TickTimer.NewTick -= OnNewTick;
                return;
            }

            ticksSinceLastSpawn++;
            if (ticksSinceLastSpawn >= currentSpawnRate)
            {
                Spawn();
                ResetSpawnTimer();
            }
        }

        protected virtual bool MobDefeated()
        {
            if (mobDefeated || !mobData.MobSize.HasMax)
            {
                return mobDefeated;
            }

            var count = mobData.MobSize.CountingOption == MobSize.MobSizeCountingOption.onRelease ?
                pools.TotalReleased : pools.TotalReturned;
            return count > mobData.MobSize.MaxToSpawn;
        }

        protected virtual bool CanSpawn()
        {
            return pools.Active < mobData.SpawnRate.MaxActivePopulation.Value;
        }

        protected virtual void Spawn()
        {
            var toSpawn = Math.Min(mobData.SpawnRate.MaxActivePopulation.Value - pools.Active,
                mobData.SpawnRate.QuantityPerSpawn.Value);

            for (int i = 0; i < toSpawn; i++)
            {
                pools.ReleaseRandomObject(mobData.SpawnPosition.Value);
            }
        }

        protected virtual void ResetSpawnTimer()
        {
            ticksSinceLastSpawn = 0;
            currentSpawnRate = mobData.SpawnRate.TicksBetweenSpawns.Value;
        }

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(mobDefeated);
        }

        public void RestoreState(JsonNode jNode)
        {
            mobDefeated = JsonSerializer.Deserialize<bool>(jNode);
        }

        private void OnDestroy()
        {
            SavingSystem.SceneLoadComplete -= OnSceneLoadComplete;
        }
    }
}