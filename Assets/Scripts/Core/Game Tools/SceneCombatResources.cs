using UnityEngine;

namespace RPGPlatformer.Core
{
    //has static instance to make it easy accessible, but will be non-persistent.
    //this is for scene-specific creatures that need special combat resources
    //that aren't worth adding to global pooler (e.g. the spit projectile for mother spider)
    public class SceneCombatResources : MonoBehaviour
    {
        [SerializeField] ObjectPoolCollection effectPooler;
        [SerializeField] ObjectPoolCollection projectilePooler;

        public static SceneCombatResources Instance;

        public ObjectPoolCollection EffectPooler => effectPooler;
        public ObjectPoolCollection ProjectilePooler => projectilePooler;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}