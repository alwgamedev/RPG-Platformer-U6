using UnityEngine;

namespace RPGPlatformer.Core
{
    public class GlobalObjectPooler : MonoBehaviour
    {
        [SerializeField] ObjectPoolCollection effectPooler;
        [SerializeField] ObjectPoolCollection projectilePooler;

        public static GlobalObjectPooler Instance;

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