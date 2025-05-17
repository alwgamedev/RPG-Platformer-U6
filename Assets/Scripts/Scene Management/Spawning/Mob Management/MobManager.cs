using UnityEngine;
using RPGPlatformer.Saving;
using System.Text.Json.Nodes;
using RPGPlatformer.Core;
using System.Text.Json;
using System;

namespace RPGPlatformer.SceneManagement
{
    //[RequireComponent(typeof(ObjectPool))]
    public class MobManager : MonoBehaviour, ISavable
    {
        [SerializeField] MobData mobData;

        ObjectPool pool;
        bool mobDefeated;
        int currentSpawnRate;
        int ticksSinceLastSpawn;

        //when go to spawn, will use mob size to re-evaluate whether is defeated
        //if defeated, save state and destroy game object (or set inactive maybe safer)
        //mobSize.CountingOption will determine whether we use pool.Released or pool.Returned
        //to determine whether mob has been defeated

        //so we need to make sure we don't spawn anything until SavingSystem has completed load
        //at beginning of scene (this can be after start in some cases; it's async and we don't know...)

        //note, if choose to count on return, you need to make sure "defeated" members are returned 
        //on death rather than destroyed

        //we will have a PoolableAICombatant which in its Configure will
        //a) if TryGetComponent(AIPatroller), set patrol bounds (those will be the configuration parameters)
        //b) set combatant.destroyOnFinalizeDeath = false and subscribe ReturnToSource to comb.OnDeathFinalized
        //we can make more PoolableObject derived classes for specific cases as needed
        //that require different configure functions and/or configure parameters

        private void Awake()
        {
            //if you are worried about the performance hit on scene transitions, 
            //you can set poolsize = 0 and have it instantiate the poolable object
            //as needed, based on the SpawnRate data
            pool = ObjectPoolCollection.AddPoolAsChild(mobData.PoolData, transform);
            pool.GeneratePool();
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
                //begin spawn behavior
                //i.e. configure update function
                ResetSpawnTimer();
                GlobalGameTools.Instance.TickTimer.NewTick += OnNewTick;
            }

            SavingSystem.SceneLoadComplete -= OnSceneLoadComplete;
        }

        protected virtual void OnNewTick()
        {
            if (!CanSpawn())
                return;

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
                pool.TotalReleased : pool.TotalReturned;
            return count > mobData.MobSize.MaxToSpawn;
        }

        protected virtual bool CanSpawn()
        {
            return pool.Active < mobData.SpawnRate.MaxActivePopulation.Value;
        }

        protected virtual void Spawn()
        {
            var toSpawn = Math.Min(mobData.SpawnRate.MaxActivePopulation.Value - pool.Active,
                mobData.SpawnRate.QuantityPerSpawn.Value);

            for (int i = 0; i < toSpawn; i++)
            {
                var o = pool.ReleaseObject(mobData.SpawnPosition.Value);
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