using UnityEngine;

namespace RPGPlatformer.Core
{
    public class SceneTools : MonoBehaviour
    {
        [SerializeField] ObjectPoolCollection effectPooler;
        [SerializeField] ObjectPoolCollection projectilePooler;

        public static SceneTools Instance;

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