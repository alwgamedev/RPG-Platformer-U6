using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(BuoyantObject))]
    public class WaterAgitator : MonoBehaviour
    {
        float halfWidth;
        Rigidbody2D rb;
        Collider2D coll;
        WaterMeshGenerator waterMesh;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            halfWidth = 0.5f * (coll.bounds.max.x - coll.bounds.min.x);
        }

        private void Start()
        {
            var b = GetComponent<BuoyantObject>();
            b.WaterEntered += OnWaterEntered;
            b.WaterExited += OnWaterExited;
        }

        private void FixedUpdate()
        {
            AgitateWater();
        }

        private void OnWaterEntered(BuoyancySource b)
        {
            waterMesh = b.WaterMesh;
        }

        private void OnWaterExited()
        {
            waterMesh = null;
        }

        private void AgitateWater()
        {
            if (waterMesh)
            {
                waterMesh.AgitateWater(transform.position.x, transform.position.y, halfWidth, rb.linearVelocityY);
            }
        }
    }
}