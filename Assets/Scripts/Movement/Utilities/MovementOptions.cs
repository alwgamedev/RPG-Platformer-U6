using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    using static PhysicsTools;

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
        [SerializeField] MovementType movementType;
        [SerializeField] MoveDirection moveDirection;
        [SerializeField] float acceleration;
        [SerializeField] bool flipSprite;
        [SerializeField] bool changeDirectionWrtGlobalUp;
        [SerializeField] bool clampXVelocityOnly;
        [SerializeField] bool rotateToDirection;
        [SerializeField] float rotationSpeed;
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
        public bool ChangeDirectionWrtGlobalUp => changeDirectionWrtGlobalUp;
        public bool ClampXVelocityOnly => clampXVelocityOnly;
        public bool RotateToDirection => rotateToDirection;
        public float RotationSpeed => rotationSpeed;

        //given desired transform.right r, this returns the nearest possible transform.up
        //so that the transform.right is within the max/min rotation range
        //(feeding in transf r or transf l is convenient for movement, where r/l
        //will be the move direction we want to face)

        //r,l do NOT have to be unit vectors! (although they usually will be already if come from move directions)
        //return value will be unit vector if r is
        public Vector2 ClampedTrUpGivenGoalTrRight(Vector2 r)
        {
            if (r.y * maxRotationRight.x > r.x * maxRotationRight.y)
            {
                return maxRotationRight.CCWPerp();
            }
            else if (r.y * minRotationRight.x < r.x * minRotationRight.y)
            {
                return minRotationRight.CCWPerp();
            }

            return r.CCWPerp();
        }

        public Vector2 ClampedTrUpGivenGoalTrLeft(Vector2 l)
        {
            if (l.y * maxRotationLeft.x < l.x * maxRotationLeft.y)
            {
                return - maxRotationLeft.CCWPerp();
            }
            else if (l.y * minRotationLeft.x > l.x * minRotationLeft.y)
            {
                return - minRotationLeft.CCWPerp();
            }

            return - l.CCWPerp();
        }

        public Quaternion ClampedRotationGivenGoalTrRight(Vector2 r)
        {
            return Quaternion.LookRotation(Vector3.forward, ClampedTrUpGivenGoalTrRight(r));
        }

        public Quaternion ClampedRotationGivenGoalTrLeft(Vector2 l)
        {
            return Quaternion.LookRotation(Vector3.forward, ClampedTrUpGivenGoalTrLeft(l));
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