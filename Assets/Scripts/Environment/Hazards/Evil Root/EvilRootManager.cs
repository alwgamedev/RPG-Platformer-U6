using UnityEngine;
using RPGPlatformer.Core;
//using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.Environment
{
    public class EvilRootManager : MonoBehaviour, IEvilRootManager
    {
        [SerializeField] Collider2D platform;
        [SerializeField] RandomizableVector3 spawnPos;
        [SerializeField] RandomizableVector3 emergePos;
        [SerializeField] RandomizableFloat spawnTime;

        ObjectPool pool;
        bool playerInBounds;
        float nextSpawnTime;
        float spawnTimer;
        //System.Random rng = new();

        //int DeployedRoots => pool.poolSize - pool.Available;

        public Collider2D Platform => platform;

        private void Awake()
        {
            pool = GetComponent<ObjectPool>();
        }

        private void Update()
        {
            if (playerInBounds && spawnTimer < nextSpawnTime)
            {
                spawnTimer += Time.deltaTime;
            }
            else if (playerInBounds && pool.Available != 0)
            {
                nextSpawnTime = spawnTime.Value;//MiscTools.RandomFloat(spawnTimeMin, spawnTimeMax);
                    //(spawnTimeMax - spawnTimeMin) * (float)MiscTools.rng.NextDouble() + spawnTimeMin;
                spawnTimer = 0;
                DeployRoot();
            }
        }

        private void DeployRoot()
        {
            var r = (EvilRoot)pool.GetObject();
            r.transform.position = GetRandomSpawnPosition();
            r.SetEmergePosition(GetRandomEmergePosition(r.transform.position.x));
            r.OnDeploy(r.transform.position.x > transform.position.x, GlobalGameTools.Instance.TokenSource.Token);
        }

        private Vector2 GetRandomSpawnPosition()
        {
            return spawnPos.Value;
            //return MiscTools.RandomPointInBox(spawnMin.position, spawnMax.position);
            //var x = (spawnMax.position.x - spawnMin.position.x) * (float)rng.NextDouble() + spawnMin.position.x;
            //var y = (spawnMax.position.y - spawnMin.position.y) * (float)rng.NextDouble() + spawnMin.position.y;
            //return new Vector2(x, y);
        }

        private Vector2 GetRandomEmergePosition(float x)
        {
            var w2 = (emergePos.Max.x - emergePos.Min.x) / 2;
            x = MiscTools.RandomFloat(x - w2, x + w2);
            var y = MiscTools.RandomFloat(emergePos.Min.y, emergePos.Max.y);
            return new Vector2(x, y);
            //var w2 = (emergeMax.position.x - emergeMin.position.x) / 4;
            //x = x - w2 + 2 * w2 * (float)MiscTools.rng.NextDouble();
            //var y = (emergeMax.position.y - emergeMin.position.y) * (float)MiscTools.rng.NextDouble() 
            //    + emergeMin.position.y;
            //return new Vector2(x, y);
        }

        private void OnPlayerEnter()
        {
            playerInBounds = true;
        }

        private void OnPlayerExit()
        {
            playerInBounds = false;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                OnPlayerEnter();
            }
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy || playerInBounds)
                return;

            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                OnPlayerEnter();
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                OnPlayerExit();
            }
        }

        //private void DeployRoot(EvilRoot root)
        //{
        //    if (!root || root.gameObject.activeSelf)
        //        return;

        //    root.CycleComplete += CompletionHandler;
        //    root.BeforeSetActive();
        //    root.gameObject.SetActive(true);
        //    root.OnDeploy(root.transform.position.x > transform.position.x, GlobalGameTools.Instance.TokenSource.Token);

        //    void CompletionHandler()
        //    {
        //        root.gameObject.SetActive(false);
        //        dormant.Enqueue(root);
        //        root.CycleComplete -= CompletionHandler;
        //    }
        //}
    }
}