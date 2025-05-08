using System.Runtime.CompilerServices;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    [ExecuteAlways]
    public class BreakableObjectConfigurer : MonoBehaviour
    {
        [SerializeField] PhysicsMaterial2D physicsMat;
        [SerializeField] float uniformMass;
        [SerializeField] float uniformLinearDamping;
        [SerializeField] LayerMask excludeLayers;
        [SerializeField] RigidbodyType2D bodyType;
        [SerializeField] bool freezeRotation;

        private void OnValidate()
        {
            UpdatePieces();
        }

        private void UpdatePieces()
        {
            var childRbs = GetComponentsInChildren<Rigidbody2D>();

            foreach (var rb in childRbs)
            {
                if (rb.transform == transform)
                    continue;

                rb.mass = uniformMass;
                rb.linearDamping = uniformLinearDamping;
                rb.excludeLayers = excludeLayers;
                rb.bodyType = bodyType;
                rb.freezeRotation = freezeRotation;
                
                if (rb.TryGetComponent(out Collider2D c))
                {
                    c.sharedMaterial = physicsMat;
                    c.enabled = rb.bodyType == RigidbodyType2D.Dynamic;
                }
            }
        }

        public void SetBodyType(RigidbodyType2D bodyType)
        {
            this.bodyType = bodyType;
            UpdatePieces();
        }
    }
}