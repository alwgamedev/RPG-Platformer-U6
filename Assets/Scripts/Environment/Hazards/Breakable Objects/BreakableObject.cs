using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Environment
{
    public class BreakableObject : MonoBehaviour
    {
        [SerializeField] BreakableObjectBreakGroup[] breakGroups;
        [SerializeField] float timeToDestroyGroupAfterBreak;
        [SerializeField] Transform parentToDestroy;
        //[SerializeField] float collisionBreakForce;

        Rigidbody2D container;
        Collider2D containerCollider;
        bool hasBroken;

        public bool HasBroken => hasBroken;

        private void Awake()
        {
            container = GetComponent<Rigidbody2D>();
            containerCollider = container.GetComponent<Collider2D>();
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.M))
        //    {
        //        Break();
        //    }
        //}

        //private void OnCollisionEnter2D(Collision2D collision)
        //{
        //    if (hasBroken) return;

        //    var contactForce = collision.GetContact(0).normalImpulse;
        //    if (contactForce > collisionBreakForce)
        //    {
        //        Break();
        //    }
        //}

        public async void Break()
        {
            if (hasBroken) return;

            hasBroken = true;
            container.bodyType = RigidbodyType2D.Kinematic;
            containerCollider.enabled = false;

            foreach (var group in breakGroups)
            {
                group.SetRigidbodyType(RigidbodyType2D.Static);//ensures you still have a stable platform to jump off
                group.EnableColliders(true);
            }

            foreach (var group in breakGroups)
            {
                await group.Break(timeToDestroyGroupAfterBreak, GlobalGameTools.Instance.TokenSource.Token);
            }

            Destroy(parentToDestroy.gameObject);
        }
    }
}