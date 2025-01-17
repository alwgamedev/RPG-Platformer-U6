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
        //protected float adjacentWallAngle;
        protected HorizontalOrientation? adjacentWallSide;

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
        //public float AdjacentWallAngle => adjacentWallAngle;
        public HorizontalOrientation? AdjacentWallSide => adjacentWallSide;

        public event Action AdjacentWallChanged;

        protected override void Awake()
        {
            base.Awake();

            maxSpeed = walkSpeed;
        }

        protected override void UpdateState()
        {
            CheckIfIsAgainstWall();

            if (rightGroundHit || leftGroundHit || adjacentWallSide.HasValue)
            {
                if ((jumping && !verifyingJump) || (airborne && !verifyingAirborne))
                {
                    TriggerLanding();
                }
            }
            else if (!jumping && !airborne && !adjacentWallSide.HasValue)
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
            return currentJumpNum < maxNumJumps || adjacentWallSide.HasValue;
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

        public void CheckIfIsAgainstWall()
        {
            if (!canWallCling) return;

            if(verifyingJump)
            {
                adjacentWallSide = null;
                return;
            }

            var oldVal = adjacentWallSide;
            UpdateAdjacentWall();

            if(oldVal != adjacentWallSide)
            {
                AdjacentWallChanged?.Invoke();
            }
        }

        public void UpdateAdjacentWall()
        {
            var rightHit = Physics2D.Raycast(ColliderCenterLeft, Vector3.right, 
                3 * myWidth, LayerMask.GetMask("Ground"));
            var leftHit = Physics2D.Raycast(ColliderCenterRight, - Vector3.right, 
                3 * myWidth,
                LayerMask.GetMask("Ground"));

            if (rightHit && (!leftHit || leftHit.distance > rightHit.distance))
            {
                var rightHit1 = Physics2D.Raycast(ColliderCenterLeft + 0.4f * myHeight * Vector3.up, Vector3.right,
                3 * myWidth, LayerMask.GetMask("Ground"));
                if(!rightHit1)//both rightHit & rightHit1 must hit in order to register a wall,
                    //otherwise you are considered to just be standing on a steep slope
                {
                    adjacentWallSide = null;
                    return;
                }

                adjacentWallSide = HorizontalOrientation.right;
                //UpdateWallAngle(rightHit);
                return;
            }
            if (leftHit && (!rightHit || rightHit.distance > leftHit.distance))
            {
                var leftHit1 = Physics2D.Raycast(ColliderCenterRight + 0.4f * myHeight * Vector3.up, - Vector3.right,
                3 * myWidth, LayerMask.GetMask("Ground"));
                if (!leftHit1)
                {
                    adjacentWallSide = null;
                    return;
                }
                adjacentWallSide = HorizontalOrientation.left;
                //UpdateWallAngle(leftHit);
                return;
            }
            //adjacentWallAngle = 0;
            adjacentWallSide = null;
        }

        //protected void UpdateWallAngle(RaycastHit2D wallHit)
        //{
        //    if (!adjacentWallSide.HasValue || !wallHit)
        //    {
        //        adjacentWallAngle = 0;
        //        return;
        //    }

        //    int sideMult = (int)adjacentWallSide.Value;
        //    Vector3 rayOrigin = sideMult > 0 ? ColliderCenterLeft : ColliderCenterRight;
        //    rayOrigin += 0.1f * myHeight * Vector3.up;

        //    var WallHit2 = Physics2D.Raycast(rayOrigin, sideMult * Vector3.right, LayerMask.GetMask("Ground"));
        //    if (!WallHit2)
        //    {
        //        adjacentWallAngle = -60;
        //        return;
        //    }

        //    var wallVector = wallHit.point - WallHit2.point;
        //    float rawWallAngle = Mathf.Rad2Deg * Mathf.Atan2(wallVector.y, wallVector.x);
        //    adjacentWallAngle = Mathf.Clamp(sideMult * (90 - rawWallAngle), -60, 60);
        //    Debug.Log("clamped wall angle " + adjacentWallAngle);
        //}

        protected override void OnDestroy()
        {
            base.OnDestroy();

            AdjacentWallChanged = null;
        }
    }
}