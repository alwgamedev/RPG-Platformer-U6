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
        protected Quaternion adjacentWallAngle;
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
        public Quaternion AdjacentWallAngle => adjacentWallAngle;
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
            //CHECK BOTH SIDES rather than just checking current orientation side, because current orientation
            //can be inaccurate when approach the wall from freefall state

            var rightHit = Physics2D.Raycast(ColliderCenterLeft, Vector3.right, 
                3.5f * myWidth, LayerMask.GetMask("Ground"));

            //Debug.DrawLine(ColliderCenterLeft, ColliderCenterLeft + 3.5f * myWidth * Vector3.right, Color.red);

            var leftHit = Physics2D.Raycast(ColliderCenterRight, - Vector3.right, 
                3.5f * myWidth,
                LayerMask.GetMask("Ground"));

            //Debug.DrawLine(ColliderCenterRight, ColliderCenterLeft - 3.5f * myWidth * Vector3.right, Color.blue);

            if (rightHit && (!leftHit || leftHit.distance > rightHit.distance))
            {
                //Debug.DrawLine(rightHit.point + 0.005f * Vector2.right, rightHit.point - 0.005f * Vector2.right,
                //    Color.yellow);

                var rightHit1 = Physics2D.Raycast(ColliderCenterLeft + 0.3f * myHeight * Vector3.up, Vector3.right,
                3 * myWidth, LayerMask.GetMask("Ground"));

                //Debug.DrawLine(ColliderCenterLeft + 0.25f * myHeight * Vector3.up,
                //    ColliderCenterLeft + 0.25f * myHeight * Vector3.up + 3 * myWidth * Vector3.right, Color.red);

                if (!rightHit1)//both rightHit & rightHit1 must hit in order to register a wall,
                    //otherwise you are considered to just be standing on a steep slope
                {
                    adjacentWallAngle = Quaternion.identity;
                    adjacentWallSide = null;
                    return;
                }

                //Debug.DrawLine(rightHit1.point + 0.005f * Vector2.right, rightHit1.point - 0.005f * Vector2.right,
                //

                var wall = rightHit1.point - rightHit.point;
                float wallAngle = Mathf.Rad2Deg * Mathf.Atan2(wall.y, wall.x);
                adjacentWallAngle = Quaternion.Euler(0, 0, wallAngle - 90);
                adjacentWallSide = HorizontalOrientation.right;
                return;
            }
            if (leftHit && (!rightHit || rightHit.distance > leftHit.distance))
            {
                //Debug.DrawLine(leftHit.point + 0.005f * Vector2.right, leftHit.point - 0.005f * Vector2.right,
                //    Color.yellow);

                var leftHit1 = Physics2D.Raycast(ColliderCenterRight + 0.3f * myHeight * Vector3.up, - Vector3.right,
                3 * myWidth, LayerMask.GetMask("Ground")); 
                
                //Debug.DrawLine(ColliderCenterRight + 0.25f * myHeight * Vector3.up,
                //    ColliderCenterLeft + 0.25f * myHeight * Vector3.up - 3 * myWidth * Vector3.right, Color.blue);


                if (!leftHit1)
                {
                    transform.rotation = Quaternion.identity;
                    adjacentWallSide = null;
                    return;
                }

                //Debug.DrawLine(leftHit1.point + 0.005f * Vector2.right, leftHit1.point - 0.005f * Vector2.right,
                //    Color.yellow);

                var wall = leftHit1.point - leftHit.point;
                float wallAngle = Mathf.Rad2Deg * Mathf.Atan2(wall.y, wall.x);
                adjacentWallAngle = Quaternion.Euler(0, 0, wallAngle - 90);
                adjacentWallSide = HorizontalOrientation.left;
                return;
            }
            adjacentWallAngle = Quaternion.identity;
            adjacentWallSide = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            AdjacentWallChanged = null;
        }
    }
}