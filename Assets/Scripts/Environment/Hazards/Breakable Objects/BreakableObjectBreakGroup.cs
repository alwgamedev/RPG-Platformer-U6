using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace RPGPlatformer.Environment
{
    public class BreakableObjectBreakGroup : MonoBehaviour
    {
        //[SerializeField] Rigidbody2D[] rbs;
        [SerializeField] BreakableObjectPiece[] pieces;
        [SerializeField] float breakDelay;
        //[SerializeField] ParticleSystem breakParticles;

        //Collider2D[] colliders;
        bool hasBroken;

        public float BreakDelay => breakDelay;

        //private void Start()
        //{
        //    //colliders = new Collider2D[pieces.Length];

        //    //for (int i = 0; i < pieces.Length; i++)
        //    //{
        //    //    if (pieces[i])
        //    //    {
        //    //        colliders[i] = pieces[i].Collider;
        //    //    }
        //    //}

        //    //if (breakParticles)
        //    //{
        //    //    breakParticles.GetComponent<ParticleSystemRenderer>().velocityScale = 0;
        //    //}
        //}

        public void EnableColliders(bool val)
        {
            foreach (var p in pieces)
            {
                p.Collider.enabled = val;
            }
        }

        public void SetRigidbodyType(RigidbodyType2D bodyType)
        {
            foreach (var p in pieces)
            {
                p.Rigidbody.bodyType = bodyType;
            }
        }

        private void ApplyBreakForce()
        {
            foreach (var p in pieces)
            {
                p.ApplyBreakForce();
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
            ApplyBreakForce();
            

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