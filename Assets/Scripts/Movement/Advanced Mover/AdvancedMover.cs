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
        //[SerializeField] protected bool detectWalls;

        protected int currentJumpNum = 0;
        protected float maxSpeed;
        protected bool running;
        protected Quaternion adjacentWallAngle;
        protected bool facingWall;

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
        public float AdjacentWallAngle => adjacentWallAngle.eulerAngles.z;//note this will be in [0, 360]
        public bool FacingWall => facingWall /*&& IsExceptableWallAngle(AdjacentWallAngle)*/;
        //public virtual float MaxPermissibleDropoffHeight => 2 * myHeight;

        protected override void Awake()
        {
            base.Awake();

            maxSpeed = walkSpeed;

            //if (detectWalls)
            //{
            //    OnUpdate += UpdateAdjacentWall;
            //}
        }

        protected override void Update()
        {
            //UpdateAdjacentWall();
            //OnUpdate?.Invoke();

            base.Update();
        }

        public void MoveGrounded()
        {
            Move(acceleration, maxSpeed, GroundDirectionVector(), false);
        }

        public void MoveFreefall(HorizontalOrientation orientation)
        {
            Move(acceleration * freefallMovementAccelerationFactor, maxSpeed, Vector2.right * (int)orientation, true);
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
            return currentJumpNum < maxNumJumps; //|| adjacentWallSide.HasValue;
        }

        public Vector2 JumpForce()
        {
            float mult = currentJumpNum > 0 ? 1.18f : 1f;
            return mult * jumpForce * Vector2.up;

            //if(currentJumpNum == 0)
            //{
            //    return jumpForce * Vector2.up;
            //}
            //return (0.2f * SpeedFraction() * jumpForce) * Vector2.right 
            //    + (1.18f - Mathf.Clamp((0.6f * myRigidbody.linearVelocityY) - 1, -0.15f, 0.25f))
            //    * jumpForce * Vector2.up;
        }

        public void ResetJumpNum()
        {
            currentJumpNum = 0;
        }

        public void ToggleRun()
        {
            Running = !Running;
        }

        public void BeginWallCling()
        {
            //if (!adjacentWallSide.HasValue) return;
            if (jumping || freefalling)
            {
                TriggerLanding();
            }
            transform.rotation = adjacentWallAngle;
        }

        public void MaintainWallCling()
        {
            transform.rotation = adjacentWallAngle;
        }

        public void EndWallCling()
        {
            transform.rotation = Quaternion.identity;
        }

        //public bool IsExceptableWallAngle(float angle)
        //{
        //    return angle < 30 || angle > 330;
        //}

        public void UpdateAdjacentWall()
        {
            //if (!detectWalls) return;

            if (verifyingJump)
            {
                NoAdjacentWall();
                return;
            }


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
                adjacentWallAngle = Quaternion.Euler(0, 0, WallAngle(lowerHit.point, midHit.point) - 90);
                return;
            }
            else if (midHit && upperHit)
            {
                facingWall = true;
                adjacentWallAngle = Quaternion.Euler(0, 0, WallAngle(midHit.point, upperHit.point) - 90);
                return;
            }
            else if (!midHit && upperHit)
            {
                facingWall = true;
                adjacentWallAngle = Quaternion.identity;
                return;
            }
            else if (!midHit & lowerHit)
            {
                if (jumping || freefalling)
                {
                    TriggerLanding();
                }
            }

            NoAdjacentWall();
        }

        protected float WallAngle(Vector3 lowerHit, Vector3 upperHit)
        {
            var wall = upperHit - lowerHit;
            return Mathf.Rad2Deg * Mathf.Atan2(wall.y, wall.x);
        }

        protected virtual void NoAdjacentWall()
        {
            adjacentWallAngle = Quaternion.identity;
            facingWall = false;
        }
    }
}