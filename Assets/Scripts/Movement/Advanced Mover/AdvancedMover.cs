using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AdvancedMover : Mover
    {
        [SerializeField] float acceleration = 30;
        [SerializeField] float freefallMovementAccelerationFactor = 0.8f;
        [SerializeField] float runSpeed = 3;
        [SerializeField] float walkSpeed = 0.8f;
        [SerializeField] int maxNumJumps = 2;
        [SerializeField] float jumpForce = 400;
        [SerializeField] bool detectWalls;

        protected int currentJumpNum = 0;
        protected float maxSpeed;
        protected bool running;
        protected Quaternion adjacentWallAngle;
        protected bool facingWall;

        public bool DetectWalls => detectWalls;
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
        public Quaternion AdjacentWallAngle => adjacentWallAngle;
        public bool FacingWall => facingWall;
        //public virtual float MaxPermissibleDropoffHeight => 2 * myHeight;

        protected override void Awake()
        {
            base.Awake();

            maxSpeed = walkSpeed;
        }

        protected override void Update()
        {
            UpdateAdjacentWall();

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

        public void UpdateAdjacentWall()
        {
            //CHECK BOTH SIDES rather than just checking current orientation side, because current orientation
            //can be inaccurate when approach the wall from freefall state

            if (!detectWalls) return;

            if (verifyingJump)
            {
                NoAdjacentWall();
                return;
            }

            var lowerHit = Physics2D.Raycast(ColliderCenterBack, (int)CurrentOrientation * Vector3.right,
                3 * myWidth, LayerMask.GetMask("Ground"));

            //Debug.DrawLine(ColliderCenterBack, ColliderCenterBack
            //    + 3 * myWidth * (int)CurrentOrientation * Vector3.right, Color.blue);

            if (!lowerHit)
            {
                NoAdjacentWall();
                return;
            }

            var upperHit = Physics2D.Raycast(ColliderCenterBack + 0.3f * myHeight * Vector3.up,
                     (int)CurrentOrientation * Vector3.right, 3 * myWidth, LayerMask.GetMask("Ground"));

            //Debug.DrawLine(ColliderCenterBack + 0.3f * myHeight * Vector3.up, ColliderCenterBack
            //    + 0.3f * myHeight * Vector3.up + 3 * myWidth * (int)CurrentOrientation * Vector3.right, Color.blue);

            if (!upperHit)
            {
                NoAdjacentWall();
                return;
            }

            adjacentWallAngle = Quaternion.Euler(0, 0, WallAngle(lowerHit.point, upperHit.point) - 90);
            facingWall = true;
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

        //detects both drop offs and step-ups
        public bool DropOffInFront(float maxHeight)
        {
            float spacing = 0.1f;
            maxHeight += 0.5f * myHeight;//shifting up higher to help detect step-ups
            Vector2 origin = ColliderCenterFront + 0.5f * myHeight * Vector3.up;
            Vector2 directlyBelowCharacter = ColliderCenterFront - groundednessTolerance * Vector3.up;

            List<Vector2> hits = new() { directlyBelowCharacter };

            //Debug.DrawLine(ColliderCenterFront, directlyBelowCharacter, Color.green);

            for (int i = 1; i <= 10; i++)
            {
                var rc = Physics2D.Raycast(origin + ((int)CurrentOrientation) * i * spacing * Vector2.right, 
                    -Vector2.up, maxHeight, LayerMask.GetMask("Ground"));

                //Debug.DrawLine(origin + ((int)CurrentOrientation) * i * spacing * Vector2.right,
                //    origin + ((int)CurrentOrientation) * i * spacing * Vector2.right - maxHeight * Vector2.up,
                //    Color.green);

                if (!rc)
                {
                    return true;
                }

                if (rc.point.y - hits[i - 1].y > spacing * (2 + MovementTools.sqrt3))
                //more than 60deg slope detected
                {
                    return true;
                }

                hits.Add(rc.point);
            }
            return false;
        }
    }
}