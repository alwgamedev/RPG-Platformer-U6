using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AdvancedMover : Mover
    {
        [SerializeField] float acceleration = 30;
        [SerializeField] float airborneAccelerationFactor = 0.8f;
        [SerializeField] float runSpeed = 3;
        [SerializeField] float walkSpeed = 0.8f;
        [SerializeField] int maxNumJumps = 2;
        [SerializeField] float jumpForce = 400;
        [SerializeField] bool canWallCling;

        protected int currentJumpNum = 0;
        protected float maxSpeed;
        protected bool running;
        protected HorizontalOrientation? adjacentWall;
        protected bool isAgainstWall;

        public bool CanWallCling => canWallCling;
        public float MaxSpeed => maxSpeed;
        public float RunSpeed => runSpeed;
        public float WalkSpeed => walkSpeed;
        public bool Running
        {
            get => running;
            set
            {
                running = value;
                maxSpeed = running ? runSpeed : walkSpeed;
            }
        }

        public HorizontalOrientation? AdjacentWall => adjacentWall;

        public event Action AdjacentWallChanged;

        protected override void Awake()
        {
            base.Awake();

            maxSpeed = walkSpeed;
        }

        protected override void UpdateState()
        {
            if (canWallCling)
            {
                CheckIfIsAgainstWall();
            }

            if (rightHit || leftHit || adjacentWall.HasValue)
            {
                if ((jumping && !verifyingJump) || (airborne && !verifyingAirborne))
                {
                    TriggerLanding();
                }
            }
            else if (!jumping && !airborne && !adjacentWall.HasValue)
            {
                TriggerAirborne();
            }
        }

        public void MoveGrounded()
        {
            Move(acceleration, maxSpeed, GroundDirectionVector(), false);
        }

        public void MoveAirborne(HorizontalOrientation orientation)
        {
            Move(acceleration * airborneAccelerationFactor, maxSpeed, Vector2.right * (int)orientation, true);
        }

        public float SpeedFraction()
        {
            float speed = Mathf.Abs(myRigidbody.linearVelocity.x);
            return Mathf.Clamp(speed / runSpeed, 0, 1);
        }

        public void Jump()
        {
            if (CanJump())
            {
                Jump(JumpForce());
                currentJumpNum++;
            }
        }

        private bool CanJump()
        {
            return currentJumpNum < maxNumJumps || adjacentWall.HasValue;
        }

        private Vector2 JumpForce()
        {
            if(currentJumpNum == 0)
            {
                return jumpForce * Vector2.up;
            }
            return (0.27f * SpeedFraction() * jumpForce) * Vector2.right 
                + (1.25f - Mathf.Clamp((0.6f * myRigidbody.linearVelocityY) - 1, -0.15f, 0.4f))
                * jumpForce * Vector2.up;
        }

        public void ResetJumpNum()
        {
            currentJumpNum = 0;
        }

        public void ToggleRun()
        {
            Running = !Running;
        }

        private void CheckIfIsAgainstWall()
        {
            if(verifyingJump)
            {
                adjacentWall = null;
                return;
            }

            var oldVal = adjacentWall;
            adjacentWall = GetAdjacentWall();
            isAgainstWall = adjacentWall.HasValue;

            if(oldVal != adjacentWall)
            {
                AdjacentWallChanged?.Invoke();
            }
        }

        public HorizontalOrientation? GetAdjacentWall()
        {
            var rightHit = Physics2D.Raycast(transform.position, Vector3.right, 
                5 * myWidth, LayerMask.GetMask("Ground"));
            var leftHit = Physics2D.Raycast(transform.position, - Vector3.right, 
                5 * myWidth,
                LayerMask.GetMask("Ground"));

            if (rightHit && (!leftHit || leftHit.distance > rightHit.distance))
            {
                return HorizontalOrientation.right;
            }
            if(leftHit && (!rightHit || rightHit.distance > leftHit.distance))
            {
                return HorizontalOrientation.left;
            }
            return null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            AdjacentWallChanged = null;
        }
    }
}