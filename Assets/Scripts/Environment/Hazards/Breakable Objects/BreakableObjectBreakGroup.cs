using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace RPGPlatformer.Environment
{
    public class BreakableObjectBreakGroup : MonoBehaviour
    {
        [SerializeField] BreakableObjectPiece[] pieces;
        [SerializeField] float breakDelay;

        bool hasBroken;

        public float BreakDelay => breakDelay;

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

        public void SetVelocity(Vector2 velocity)
        {
            foreach (var p in pieces)
            {
                p.Rigidbody.linearVelocity = velocity;
            }
        }

        public async Task Break(float timeToDestroy, BreakData data, CancellationToken token)
        {
            if (hasBroken) return;

            hasBroken = true; 
            transform.SetParent(null);
            await MiscTools.DelayGameTime(breakDelay, token);
            foreach (var p in pieces)
            {
                p.OnBreak(data);
            }
            Destroy(gameObject, timeToDestroy);
        }
    }
}