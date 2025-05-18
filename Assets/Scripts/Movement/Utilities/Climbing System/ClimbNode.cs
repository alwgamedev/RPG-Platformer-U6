using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class ClimbNode : MonoBehaviour
    {

        public ClimbNode Higher { get; private set; }
        public ClimbNode Lower {  get; private set; }
        public float MaxPosition { get; private set; }
        public float MinPosition { get; private set; }
        public Rigidbody2D Rigidbody { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            UpdateMaxAndMinPositions();
        }

        public void SetAdjacentNodes(ClimbNode higher, ClimbNode lower)
        {
            Higher = higher;
            Lower = lower;
        }

        public void ApplyAcceleration(Vector2 acceleration)
        {
            Rigidbody.AddForce(Rigidbody.mass * acceleration);
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
                + (localPosition > 0 ? localPosition * HigherDirection() : - localPosition * LowerDirection());
        }

        public float WorldToLocalPosition(Vector3 worldPosition)
        {
            return Vector3.Dot(worldPosition - transform.position, HigherDirection());
        }

        public Vector3 HigherDirection()
        {
            if (!Higher)
            {
                return Vector3.up;
            }

            return (Higher.transform.position - transform.position).normalized;
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
            if (!Lower)
            {
                return - Vector3.up;
            }

            return (Lower.transform.position - transform.position).normalized;
        }

        public Vector3 LowerRay()
        {
            if (!Lower)
            {
                return Vector3.zero;
            }

            return Lower.transform.position - transform.position;
        }

        private void UpdateMaxAndMinPositions()
        {
            MaxPosition = HigherRay().magnitude;
            MinPosition = - LowerRay().magnitude;
        }
    }
}