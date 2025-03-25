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
        [SerializeField] Vector2 maxRotationRight;
        [SerializeField] Vector2 minRotationRight;
        [SerializeField] Vector2 maxRotationLeft;
        [SerializeField] Vector2 minRotationLeft;

        public MovementType MovementType => movementType;
        public MoveDirection MoveDirection => moveDirection;
        public float Acceleration => acceleration;
        public bool FlipSprite => flipSprite;
        public bool ClampXVelocityOnly => clampXVelocityOnly;
        public bool RotateToDirection => rotateToDirection;
        
        //returns rotation q such that if transformation.rotation = q,
        //then transform.right points in direction r

        public Quaternion RotateTransformLeftTo(Vector2 l)
        {
            if (l.y > maxRotationLeft.y)
            {
                return Quaternion.LookRotation(Vector3.forward, - maxRotationLeft.CCWPerp());
            }
            else if (l.y < minRotationLeft.y)
            {
                return Quaternion.LookRotation(Vector3.forward, - minRotationLeft.CCWPerp());
            }

            return Quaternion.LookRotation(Vector3.forward, - l.CCWPerp());
        }

        public Quaternion RotateTransformRightTo(Vector2 r)
        {
            if (r.y > maxRotationRight.y)
            {
                return Quaternion.LookRotation(Vector3.forward, maxRotationRight.CCWPerp());
            }
            else if (r.y < minRotationRight.y)
            {
                return Quaternion.LookRotation(Vector3.forward, minRotationRight.CCWPerp());
            }

            return Quaternion.LookRotation(Vector3.forward, r.CCWPerp());
        }

        public void OnBeforeSerialize()
        {
            maxRotationRight = new(Mathf.Cos(maxRotationAngle), Mathf.Sin(maxRotationAngle));
            minRotationRight = new(Mathf.Cos(minRotationAngle), Mathf.Sin(minRotationAngle));
            maxRotationLeft = new(- maxRotationRight.x, maxRotationRight.y);
            minRotationLeft = new(- minRotationRight.x, minRotationRight.y);
        }

        public void OnAfterDeserialize() { }
        
    }
}