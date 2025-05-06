using UnityEngine;

namespace RPGPlatformer.Effects
{
    public class SpiderEggFragment : MonoBehaviour
    {
        //[SerializeField] Vector2 fractureAnchor;
        [SerializeField] Vector2 breakAccel;

        public Rigidbody2D Rb { get; private set; }
        public FixedJoint2D Joint { get; private set; }

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Joint = GetComponent<FixedJoint2D>();
        }

        //public void Fracture()
        //{
        //    Joint.autoConfigureConnectedAnchor = false;
        //    Joint.anchor = fractureAnchor;
        //}

        public void Break()
        {
            Joint.enabled = false;
            Rb.freezeRotation = false;
            Rb.AddForce(Rb.mass * breakAccel, ForceMode2D.Impulse);
        }
    }
}