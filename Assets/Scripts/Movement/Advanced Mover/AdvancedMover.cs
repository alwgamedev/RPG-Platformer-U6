using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AdvancedMover : Mover
    {
        [SerializeField] protected float acceleration = 30;
        [SerializeField] protected float freefallMovementAccelerationFactor = 0.8f;
        [SerializeField] protected float runSpeed = 3;
        [SerializeField] protected float walkSpeed = 0.8f;
        [SerializeField] protected int maxNumJumps = 2;
        [SerializeField] protected float jumpForce = 400;
        [SerializeField] protected float doubleJumpForceMultiplier = 1.18f;

        protected int currentJumpNum = 0;
        protected float doubleJumpForce;
        protected float maxSpeed;
        protected bool running;
        protected Vector2 adjacentWallDirection = Vector2.up;
        protected bool facingWall;

        public float MaxSpeed => maxSpeed;
        public float RunSpeed => runSpeed;
        public float WalkSpeed => walkSpeed;
        public virtual bool Running
        {
            get => running;
            set
            {
                running = value;
                maxSpeed = running ? runSpeed : walkSpeed;
            }
        }
        public bool FacingWall => facingWall;

        public event Action AwkwardWallMoment;

        protected override void Awake()
        {
            base.Awake();

            maxSpeed = walkSpeed;
            doubleJumpForce = doubleJumpForceMultiplier * jumpForce;
        }

        public void MoveGroundedWithoutAcceleration(bool matchRotationToGround)
        {
            MoveWithoutAcceleration(maxSpeed, GroundDirectionVector(), matchRotationToGround);
        }

        public void MoveGrounded(bool matchRotationToGround = false)
        {
            Move(acceleration, maxSpeed, GroundDirectionVector(), matchRotationToGround, false);
        }

        public void MoveFreefall(HorizontalOrientation orientation)
        {
            Move(acceleration * freefallMovementAccelerationFactor, maxSpeed, Vector2.right, false, true);
        }

        public float SpeedFraction()
        {
            return myRigidbody.linearVelocity.magnitude / runSpeed;
        }

        public void Jump()
        {
            if (CanJump())
            {
                Jump(JumpForce());
                currentJumpNum++;
            }
        }

        public override void Jump(Vector2 force)
        {
            base.Jump(force);

            NoAdjacentWall();
        }

        public bool CanJump()
        {
            return currentJumpNum < maxNumJumps;
        }

        public Vector2 JumpForce()
        {
            return currentJumpNum > 0 ? doubleJumpForce * Vector2.up : jumpForce * Vector2.up;
        }

        public void ResetJumpNum()
        {
            currentJumpNum = 0;
        }

        public void ToggleRun()
        {
            Running = !Running;
        }

        public void BeginWallCling(bool airborne)
        {
            if (airborne)
            {
                TriggerLanding();
            }
            transform.rotation = Quaternion.LookRotation(Vector3.forward, adjacentWallDirection);
        }

        public void MaintainWallCling()
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, adjacentWallDirection);
        }

        public void EndWallCling()
        {
            transform.rotation = Quaternion.identity;
        }

        public void UpdateAdjacentWall(bool airborne)
        {
            if (verifyingJump)
            {
                NoAdjacentWall();
                return;
            }

            //TO-DO: add more hits

            var upperHit = Physics2D.Raycast(ColliderCenterBack + 0.4f * myHeight * Vector3.up,
                     (int)CurrentOrientation * Vector3.right, 1.75f * myWidth, LayerMask.GetMask("Ground"));
            var midHit = Physics2D.Raycast(ColliderCenterBack + 0.1f * myHeight * Vector3.up, 
                (int)CurrentOrientation * Vector3.right, 1.75f * myWidth, LayerMask.GetMask("Ground")); 
            var lowerHit = Physics2D.Raycast(ColliderCenterBack - 0.2f * myHeight * Vector3.up,
                (int)CurrentOrientation * Vector3.right, 1.75f * myWidth, LayerMask.GetMask("Ground"));

            //Debug.DrawLine(ColliderCenterBack + 0.4f * myHeight * Vector3.up,
            //    ColliderCenterBack + 0.4f * myHeight * Vector3.up + 1.75f * myWidth
            //    * (int)CurrentOrientation * Vector3.right, Color.blue);
            //Debug.DrawLine(ColliderCenterBack + 0.1f * myHeight * Vector3.up, ColliderCenterBack
            //    + 0.1f * myHeight * Vector3.up + 1.75f * myWidth * (int)CurrentOrientation * Vector3.right, Color.blue);
            //Debug.DrawLine(ColliderCenterBack - 0.2f * myHeight * Vector3.up,
            //    ColliderCenterBack - 0.2f * myHeight * Vector3.up + 1.75f * myWidth
            //    * (int)CurrentOrientation * Vector3.right, Color.blue);

            if (midHit && lowerHit)
            {
                facingWall = true;
                adjacentWallDirection = midHit.point - lowerHit.point;
                return;
            }
            else if (midHit && upperHit)
            {
                facingWall = true;
                adjacentWallDirection = upperHit.point - midHit.point;
                return;
            }
            else if (!midHit && upperHit)
            {
                facingWall = true;
                adjacentWallDirection = Vector3.up;
                return;
            }
            else if (midHit || lowerHit)
            {
                if (airborne)
                {
                    TriggerLanding();
                    AwkwardWallMoment?.Invoke();
                }
            }

            NoAdjacentWall();
        }

        protected virtual void NoAdjacentWall()
        {
            adjacentWallDirection = Vector2.up;
            facingWall = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AwkwardWallMoment = null;
        }
    }
}