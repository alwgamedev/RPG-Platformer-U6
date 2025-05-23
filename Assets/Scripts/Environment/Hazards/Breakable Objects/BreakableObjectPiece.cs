using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Environment
{
    public class BreakableObjectPiece : MonoBehaviour
    {
        [SerializeField] RandomizableVector2 breakForce;

        public Rigidbody2D Rigidbody { get; private set; }
        public Collider2D Collider { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Collider = GetComponent<Collider2D>();
        }

        public void ApplyBreakForce()
        {
            Rigidbody.AddForce(breakForce.Value, ForceMode2D.Impulse);
        }
    }
}