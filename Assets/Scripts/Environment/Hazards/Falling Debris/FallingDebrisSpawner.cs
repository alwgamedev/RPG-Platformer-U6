using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Environment
{

    public class FallingDebrisSpawner : MonoBehaviour
    {
        [SerializeField] ObjectPoolData[] poolData;
        [SerializeField] RandomizableVector2 spawnPosition;
        [SerializeField] RandomizableFloat timeBetweenDrops;
        [SerializeField] bool fixedDuration;
        [SerializeField] RandomizableFloat duration;
        [SerializeField] SortingOrderPerSpawn sortingOrderPerSpawn;

        public enum SortingOrderPerSpawn
        {
            none, increment, decrement
        }

        int groundLayer;
        int sortingOrderCounter;
        float dropTimer;
        float dropTime;
        float lifeTimer;
        float lifeTime;
        Collider2D ceiling;
        ObjectPoolCollection pools;

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
            ceiling = ((GameObject)poolData[0].ConfigurationParameters).GetComponent<Collider2D>();
            pools = gameObject.AddComponent<ObjectPoolCollection>();
            pools.CreatePools(poolData);

            if (fixedDuration)
            {
                lifeTime = duration.Value;
            }
        }

        private void OnEnable()
        {
            RestartDropTimer();
        }

        private void Update()
        {
            dropTimer += Time.deltaTime;
            if (dropTimer > dropTime)
            {
                Drop();
                RestartDropTimer();
            }

            if (fixedDuration)
            {
                lifeTimer += Time.deltaTime;
                if (lifeTimer > lifeTime)
                {
                    enabled = false;
                }
            }
        }

        public void Drop()
        {
            var p = spawnPosition.Value;
            var r = Physics2D.Raycast(p, Vector2.up, Mathf.Infinity, groundLayer);
            if (r)
            {
                p = r.point;
            }
            else
            {
                p = ceiling.ClosestPoint(p);
            }
            var o = pools.ReleaseRandomObject(p) as FallingDebris;
            if (o && sortingOrderPerSpawn != SortingOrderPerSpawn.none)
            {
                o.SpriteRenderer.sortingOrder = sortingOrderCounter;
                if (sortingOrderPerSpawn == SortingOrderPerSpawn.increment)
                {
                    sortingOrderCounter++;
                }
                else if (sortingOrderPerSpawn == SortingOrderPerSpawn.decrement)
                {
                    sortingOrderCounter--;
                }
            }
        }

        private void RestartDropTimer()
        {
            dropTimer = 0;
            dropTime = timeBetweenDrops.Value;
        }
    }
}