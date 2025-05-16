using UnityEngine;
using RPGPlatformer.Saving;
using System.Text.Json.Nodes;
using RPGPlatformer.Core;
using System.Text.Json;

namespace RPGPlatformer.SceneManagement
{
    [RequireComponent(typeof(ObjectPool))]
    public class MobManager : MonoBehaviour, ISavable
    {
        [SerializeField] MobData mobData;

        ObjectPool pool;
        bool mobDefeated;

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
            pool = GetComponent<ObjectPool>();
        }

        //private void Awake()
        //{
        //    CreatePools();
        //}

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(mobDefeated);
        }

        public void RestoreState(JsonNode jNode)
        {
            mobDefeated = JsonSerializer.Deserialize<bool>(jNode);
        }
    }
}