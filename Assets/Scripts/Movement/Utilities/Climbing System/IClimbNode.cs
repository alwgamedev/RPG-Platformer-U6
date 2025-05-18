using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IClimbNode
    {
        public float Speed { get; }

        public void ApplyAcceleration(Vector2 acceleration);

        public ClimberData GetClimberData(float localPosition);

        public Vector3 LocalToWorldPosition(float localPosition);
        public float WorldToLocalPosition(Vector3 worldPosition);

        public Vector3 HigherDirection();

        public Vector3 HigherRay();

        public Vector3 LowerDirection();

        public Vector3 LowerRay();
    }
}