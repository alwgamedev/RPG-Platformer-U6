using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class FallingDebrisSpawner : MonoBehaviour
    {
        [SerializeField] ObjectPoolData poolData;
        [SerializeField] RandomizableVector2 spawnPosition;
        [SerializeField] RandomizableFloat timeBetweenDrops;

        int groundLayer;
        float dropTimer;
        float dropTime;
        Collider2D ceiling;
        ObjectPool pool;

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
            ceiling = ((GameObject)poolData.ConfigurationParameters).GetComponent<Collider2D>();
            pool = ObjectPoolCollection.AddPoolAsChild(poolData, transform);
            pool.FillPool();
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
            pool.ReleaseObject(p);
        }

        private void RestartDropTimer()
        {
            dropTimer = 0;
            dropTime = timeBetweenDrops.Value;
        }
    }
}