using UnityEngine;
using RPGPlatformer.Core;
//using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.Environment
{
    public class EvilRootManager : MonoBehaviour, IEvilRootManager
    {
        [SerializeField] Transform rootSortingLayerDataSource;
        [SerializeField] Collider2D platform;
        [SerializeField] RandomizableVector3 spawnPos;
        [SerializeField] RandomizableVector3 emergePos;
        [SerializeField] RandomizableFloat spawnTime;

        protected ObjectPool pool;
        bool playerInBounds;
        float nextSpawnTime;
        float spawnTimer;

        public Transform RootSortingLayerDataSource => rootSortingLayerDataSource;
        public Collider2D Platform => platform;

        private void Awake()
        {
            pool = GetComponent<ObjectPool>();
        }

        protected virtual void Update()
        {
            if (!playerInBounds)
                return;

            if (spawnTimer < nextSpawnTime)
            {
                spawnTimer += Time.deltaTime;
            }
            else if (pool.Available != 0)
            {
                nextSpawnTime = spawnTime.Value;
                spawnTimer = 0;
                DeployRoot();
            }
        }

        protected void DeployRoot()
        {
            var r = (EvilRoot)pool.ReleaseObject(spawnPos.Value);
            //r.transform.position = spawnPos.Value;
            r.SetEmergePosition(GetRandomEmergePosition(r.transform.position.x));
            //r.SetColliderAvoidanceSide(r.transform.position.x < transform.position.x ?
            //    CurveBounds.AvoidanceSide.left : CurveBounds.AvoidanceSide.right);
            //bool throwRight = MiscTools.rng.Next(0, 2) > 0;
            r.OnDeploy(GlobalGameTools.Instance.TokenSource.Token);
        }

        private Vector2 GetRandomEmergePosition(float x)
        {
            var w2 = (emergePos.Max.x - emergePos.Min.x) / 2;
            x = MiscTools.RandomFloat(x - w2, x + w2);
            var y = MiscTools.RandomFloat(emergePos.Min.y, emergePos.Max.y);
            return new Vector2(x, y);
        }

        protected virtual void OnPlayerEnter()
        {
            playerInBounds = true;
        }

        protected virtual void OnPlayerExit()
        {
            playerInBounds = false;
            nextSpawnTime = 0;//so that we spawn immediately when player enters bounds again
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
    }
}