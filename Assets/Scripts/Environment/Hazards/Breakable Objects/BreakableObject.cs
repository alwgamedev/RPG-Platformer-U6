using UnityEngine;
using RPGPlatformer.Core;
using System.Threading.Tasks;

namespace RPGPlatformer.Environment
{
    public class BreakableObject : MonoBehaviour
    {
        [SerializeField] BreakableObjectBreakGroup[] breakGroups;
        [SerializeField] float timeToDestroyGroupAfterBreak;
        [SerializeField] Transform parentToDestroy;
        [SerializeField] BreakOptions options;

        Rigidbody2D container;
        Collider2D containerCollider;
        bool hasBroken;

        public bool HasBroken => hasBroken;

        private void Awake()
        {
            container = GetComponent<Rigidbody2D>();
            containerCollider = container.GetComponent<Collider2D>();
        }

        public async void Break()
        {
            await Break(DefaultBreakData());
        }

        public async void Break(Collision2D collision)
        {
            await Break(CollisionBreakData(collision));
        }

        public async Task Break(BreakData data)
        {
            if (hasBroken) return;

            hasBroken = true;
            container.bodyType = RigidbodyType2D.Kinematic;
            containerCollider.enabled = false;

            bool setStatic = breakGroups.Length > 1 || breakGroups[0].BreakDelay > 0;
            foreach (var group in breakGroups)
            {
                if (setStatic)
                { 
                    group.SetRigidbodyType(RigidbodyType2D.Static);
                }//ensures you still have a stable platform to jump off while earlier groups crumble
                //(mainly for crumbling platform)
                group.EnableColliders(true);
            }

            foreach (var group in breakGroups)
            {
                await group.Break(timeToDestroyGroupAfterBreak, data, GlobalGameTools.Instance.TokenSource.Token);
            }

            Destroy(parentToDestroy.gameObject);
        }

        private BreakData DefaultBreakData()
        {
            return new BreakData(options, container.linearVelocity, default, default);
        }

        private BreakData CollisionBreakData(Collision2D collision)
        {
            return new BreakData(options, container.linearVelocity, 
                collision.GetContact(0).normalImpulse * collision.GetContact(0).normal,
                collision.GetContact(0).point);
        }
    }
}