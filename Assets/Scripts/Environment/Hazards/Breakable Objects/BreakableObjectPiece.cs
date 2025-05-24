using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class BreakableObjectPiece : MonoBehaviour
    {
        //[SerializeField] RandomizableVector2 breakForce;

        public Rigidbody2D Rigidbody { get; private set; }
        public Collider2D Collider { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Collider = GetComponent<Collider2D>();
        }

        //public void ApplyBreakForce()
        //{
        //    Rigidbody.AddForce(breakForce.Value, ForceMode2D.Impulse);
        //}

        public void OnBreak(BreakData data)
        {
            Rigidbody.bodyType = RigidbodyType2D.Dynamic;
            if (data.options.InheritVelocity)
            {
                Rigidbody.linearVelocity = data.initialVelocity;
            }
            if (data.options.ApplyBreakForce)
            {
                var v = ((Vector2)transform.position - data.breakPoint).normalized;
                v = Vector2.Dot(data.breakForce, v) * v;
                Rigidbody.AddForce(v, ForceMode2D.Impulse);
            }
        }
    }
}