using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public enum MovementType
    {
        Grounded, Freefall, Jumping, Flying
    }

    public enum MoveDirection
    {
        Ground, Input, Horizontal, Vertical
    }

    [Serializable]
    public struct MovementOptions : ISerializationCallbackReceiver
    {
        const float PI2 = Mathf.PI / 2;

        [SerializeField] MovementType movementType;
        [SerializeField] MoveDirection moveDirection;
        [SerializeField] float acceleration;
        [SerializeField] bool flipSprite;
        [SerializeField] bool clampXVelocityOnly;
        [SerializeField] bool rotateToDirection;
        [Range(-PI2, PI2)][SerializeField] float maxRotationAngle;
        [Range(-PI2, PI2)][SerializeField] float minRotationAngle;
        [SerializeField] Vector2 maxRotation;
        [SerializeField] Vector2 minRotation;

        public MovementType MovementType => movementType;
        public MoveDirection MoveDirection => moveDirection;
        public float Acceleration => acceleration;
        public bool FlipSprite => flipSprite;
        public bool ClampXVelocityOnly => clampXVelocityOnly;
        public bool RotateToDirection => rotateToDirection;
        
        //r is desired direction for transform.right
        //r should be a unit vector
        public Quaternion RotateTransformRightTo(Vector2 r)
        {
            if (r.y > maxRotation.y)
            {
                return Quaternion.LookRotation(Vector3.forward, maxRotation.CCWPerp());
            }
            else if (r.y < minRotation.y)
            {
                return Quaternion.LookRotation(Vector3.forward, minRotation.CCWPerp());
            }

            return Quaternion.LookRotation(Vector3.forward, r.CCWPerp());
        }

        public void OnBeforeSerialize()
        {
            maxRotation = new(Mathf.Cos(maxRotationAngle), Mathf.Sin(maxRotationAngle));
            minRotation = new(Mathf.Cos(minRotationAngle), Mathf.Sin(minRotationAngle));
        }

        public void OnAfterDeserialize() { }
        
    }
}