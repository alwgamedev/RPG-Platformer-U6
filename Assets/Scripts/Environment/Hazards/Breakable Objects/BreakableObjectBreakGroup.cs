using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace RPGPlatformer.Environment
{
    public class BreakableObjectBreakGroup : MonoBehaviour
    {
        [SerializeField] Rigidbody2D[] rbs;
        [SerializeField] float breakDelay;
        //[SerializeField] ParticleSystem breakParticles;

        Collider2D[] colliders;
        bool hasBroken;

        public float BreakDelay => breakDelay;

        private void Awake()
        {
            colliders = new Collider2D[rbs.Length];

            for (int i = 0; i < rbs.Length; i++)
            {
                if (rbs[i])
                {
                    colliders[i] = rbs[i].GetComponent<Collider2D>();
                }
            }

            //if (breakParticles)
            //{
            //    breakParticles.GetComponent<ParticleSystemRenderer>().velocityScale = 0;
            //}
        }

        public void EnableColliders(bool val)
        {
            foreach (var c in colliders)
            {
                if (c)
                {
                    c.enabled = val;
                }
            }
        }

        public void SetRigidbodyType(RigidbodyType2D bodyType)
        {
            foreach (var rb in rbs)
            {
                rb.bodyType = bodyType;
            }
        }

        public async Task Break(float timeToDestroy, CancellationToken token)
        {
            if (hasBroken) return;

            hasBroken = true;
            transform.SetParent(null);

            //if (breakParticles)
            //{
            //    //breakParticles.transform.position = transform.position + particleSystemOffset;
            //    breakParticles.Play();
            //}

            await MiscTools.DelayGameTime(breakDelay, token);

            SetRigidbodyType(RigidbodyType2D.Dynamic);

            Destroy(gameObject, timeToDestroy);
        }

        //public void DetachParticles()
        //{
        //    if (breakParticles)
        //    {
        //        breakParticles.transform.SetParent(null);
        //    }
        //    //otherwise I can't get them to stop inheriting velocity and flying off
        //}
    }
}