using UnityEngine;

namespace RPGPlatformer.Core
{
    public class PersistentObjectsSpawner : MonoBehaviorInputConfigurer
    {
        [SerializeField] GameObject[] prefabsToSpawn;

        bool thisHoldsPersistentObjects;

        static bool persistentObjectsHaveSpawned;

        private void Awake()
        {
            if (!persistentObjectsHaveSpawned)
            {
                SpawnPersistentObjects();
            }
            else if (!thisHoldsPersistentObjects)
            {
                Destroy(gameObject);
            }
        }

        private void SpawnPersistentObjects()
        {
            foreach (var p in prefabsToSpawn)
            {
                if (p != null)
                {
                    Instantiate(p, transform);
                }
            }

            DontDestroyOnLoad(gameObject);

            persistentObjectsHaveSpawned = true;
            thisHoldsPersistentObjects = true;
        }
    }
}