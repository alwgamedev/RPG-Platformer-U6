using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class BuoyantObject : MonoBehaviour
    {
        Rigidbody2D rb;
        Collider2D coll;
        BuoyancySource buoyancySource;

        public float Width { get; private set; }
        public float Height { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();

            Width = coll.bounds.max.x - coll.bounds.min.x;
            Height = coll.bounds.max.y - coll.bounds.min.y;
        }

        private void FixedUpdate()
        {
            if (buoyancySource /*|| !buoyancySource.gameObject.activeInHierarchy*/)
            {
                rb.AddForce(buoyancySource.BuoyancyForce(
                    DisplacedArea(buoyancySource.FluidHeight(coll.bounds.center.x))));

                var s = rb.linearVelocity.magnitude;
                if (s > 1E-05f)//unity uses this in their normalize function
                    //(not using normalize bc I need the speed, and don't want to compute magnitude twice)
                {
                    var u = rb.linearVelocity / s;
                    var a = CrossSectionWidth(u);

                    rb.AddForce(buoyancySource.DragForce(s, a, u));
                }
            }
        }

        //assume the buoyant object is rectangular and upright (never rotates)
        public float DisplacedArea(float fluidHeight)
        {
            return Mathf.Max((fluidHeight - coll.bounds.min.y) * Width, 0);
        }

        /// <summary>
        /// Cross section width in direction of current velocity (you can pass in speed if it has alread been calculated).
        public float CrossSectionWidth(Vector2 velocityDirection)
        {
            if (velocityDirection.y < 1E-05f)
            {
                return Height;
            }

            return Mathf.Min(Height, Width / velocityDirection.y);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!collider.gameObject.activeInHierarchy)
                return;

            if (collider.gameObject.TryGetComponent(out BuoyancySource b))
            {
                buoyancySource = b;
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (buoyancySource && collider.transform == buoyancySource.transform)
            {
                buoyancySource = null;
            }
        }
    }
}