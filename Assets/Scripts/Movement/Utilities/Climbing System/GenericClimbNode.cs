using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class GenericClimbNode<T> : MonoBehaviour, IClimbNode where T : GenericClimbNode<T>
    {
        public T Higher { get; protected set; }
        public T Lower { get; protected set; }
        public float MaxPosition { get; protected set; }
        public float MinPosition { get; protected set; }
        public Vector2 Velocity => Rigidbody.linearVelocity;
        public Rigidbody2D Rigidbody { get; protected set; }

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        protected virtual void FixedUpdate()
        {
            UpdateMaxAndMinPositions();
        }

        public virtual void SetAdjacentNodes(T higher, T lower)
        {
            Higher = higher;
            Lower = lower;
        }

        public void ApplyAcceleration(Vector2 acceleration)
        {
            Rigidbody.AddForce(Rigidbody.mass * acceleration);
        }

        public Vector2 VelocityAtPosition(float localPosition)
        {
            if (localPosition >= 0)
            {
                if (Higher)
                {
                    return Vector2.Lerp(Velocity, Higher.Velocity, localPosition / MaxPosition);
                }

                return Velocity;
            }

            if (localPosition < 0 && Lower)
            {
                return Vector2.Lerp(Velocity, Lower.Velocity, localPosition / MinPosition);
            }

            return Velocity;
        }

        public ClimberData GetClimberData(float localPosition)
        {
            if (localPosition > MaxPosition && Higher)
            {
                return Higher.GetClimberData(localPosition - MaxPosition);
            }

            if (localPosition < MinPosition && Lower)
            {
                return Lower.GetClimberData(localPosition - MinPosition);
            }

            return new(this, Mathf.Clamp(localPosition, MinPosition, MaxPosition));
        }

        //"local position" measured on a bent number line, where positives go in HigherDirection()
        //and negatives in LowerDirection()
        public Vector3 LocalToWorldPosition(float localPosition)
        {
            return transform.position
                + (localPosition > 0 ? localPosition * HigherDirection() : -localPosition * LowerDirection());
        }

        public float WorldToLocalPosition(Vector3 worldPosition)
        {
            return Vector3.Dot(worldPosition - transform.position, HigherDirection());
        }

        public Vector3 HigherDirection()
        {
            if (!Higher || MaxPosition < 1E-05f)
            {
                return Vector3.up;
            }

            return HigherRay() / MaxPosition;
        }

        public Vector3 HigherRay()
        {
            if (!Higher)
            {
                return Vector3.zero;
            }

            return Higher.transform.position - transform.position;
        }

        public Vector3 LowerDirection()
        {
            if (!Lower || -MinPosition < 1E-05f)
            {
                return -Vector3.up;
            }

            return LowerRay() / -MinPosition;
        }

        public Vector3 LowerRay()
        {
            if (!Lower)
            {
                return Vector3.zero;
            }

            return Lower.transform.position - transform.position;
        }

        protected void UpdateMaxAndMinPositions()
        {
            MaxPosition = HigherRay().magnitude;
            MinPosition = -LowerRay().magnitude;
        }
    }
}