using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(BreakableObject))]
    public class CollisionBasedBreaker : MonoBehaviour
    {
        [SerializeField] float collisionBreakForce;

        BreakableObject breakableObject;

        private void Awake()
        {
            breakableObject = GetComponent<BreakableObject>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (breakableObject.HasBroken) return;

            var contactForce = collision.GetContact(0).normalImpulse;
            if (contactForce > collisionBreakForce)
            {
                breakableObject.Break();
            }
        }
    }

}