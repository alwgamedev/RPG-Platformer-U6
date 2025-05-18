using RPGPlatformer.Combat;
using RPGPlatformer.Saving;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    //for objects that get defeated and disappear from the game forever
    //(e.g. a wall you break down in one level that should be gone next time you come back to level)

    //**Make sure those objects don't destroy themselves when defeated!
    //They need to stay inactive until the end of the scene so that their defeated state gets saved.

    //if you don't want to save any other components on the object,
    //you could attach this to a parent or child object and give that a savable monobehaviour component
    public class SingleSpawnDefeatableEntity : MonoBehaviour, ISavable
    {
        [SerializeField] protected GameObject defeatableEntity;

        protected bool defeated;

        public bool Defeated => defeated;

        public event Action InitializationComplete;
        public event Action OnDefeated;

        protected virtual void Awake()
        {
            if (!defeatableEntity)
            {
                defeatableEntity = gameObject;
            }

            SavingSystem.SceneLoadComplete += OnSceneLoadComplete;
        }

        protected void OnSceneLoadComplete()
        {
            if (defeated)
            {
                Deactivate();
            }
            else
            {
                Configure();
            }

            InitializationComplete?.Invoke();
            SavingSystem.SceneLoadComplete -= OnSceneLoadComplete;
        }

        protected virtual void Configure()
        {
            if (defeatableEntity.TryGetComponent(out ICombatant c))
            {
                c.DestroyOnFinalizeDeath = false;
                c.DeathFinalized += Defeat;
            }
            else if (defeatableEntity.TryGetComponent(out IHealth h))
            {
                h.OnDeath += HealthDefeat;
            }
        }

        protected void Defeat()
        {
            if (!defeated)
            {
                defeated = true;
                OnDefeated?.Invoke();
                Deactivate();
            }
            //^don't destroy, we need to keep the game object around so it gets included in
            //the end of scene save (saving system does a save before next scene is loaded)
        }

        protected void Deactivate()
        {
            if (defeatableEntity)
            {
                defeatableEntity.SetActive(false);
            }
        }

        protected void HealthDefeat(IDamageDealer d)
        {
            Defeat();
        }

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(defeated);
        }

        public void RestoreState(JsonNode jNode)
        {
            defeated = jNode.Deserialize<bool>();
        }

        protected virtual void OnDestroy()
        {
            SavingSystem.SceneLoadComplete -= OnSceneLoadComplete;
            InitializationComplete = null;
            OnDefeated = null;
        }
    }
}