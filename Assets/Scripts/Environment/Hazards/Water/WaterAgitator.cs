using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class WaterAgitator : MonoBehaviour
    {
        float halfWidth = .1f;
        Rigidbody2D rb;
        Collider2D coll;
        WaterMeshGenerator waterMesh;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            halfWidth = 0.5f * (coll.bounds.max.x - coll.bounds.min.x);
        }

        private void FixedUpdate()
        {
            AgitateWater();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!collider.gameObject.activeInHierarchy)
                return;

            if (collider.gameObject.TryGetComponent(out WaterMeshGenerator w))
            {
                waterMesh = w;
                AgitateWater();
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (waterMesh && collider.transform == waterMesh.transform)
            {
                waterMesh = null;
            }
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