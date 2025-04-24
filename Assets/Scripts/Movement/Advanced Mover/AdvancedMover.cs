using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AdvancedMover : Mover, IClimber
    {
        [SerializeField] protected float runSpeed = 3;
        [SerializeField] protected float walkSpeed = 0.8f;
        [SerializeField] protected int maxNumJumps = 2;
        [SerializeField] protected Vector2 jumpForce = 375 * Vector2.up;
        [SerializeField] protected float doubleJumpForceMultiplier = 1.18f;

        protected int currentJumpNum = 0;
        protected Vector2 doubleJumpForce;
        protected Vector2 adjacentWallDirection = Vector2.up;
        protected bool facingWall;

        public float RunSpeed => runSpeed;
        public float WalkSpeed => walkSpeed;
        public virtual bool Running { get; set; }
        public bool FacingWall => facingWall;
        public ClimberData ClimberData { get; }

        public event Action AwkwardWallMoment;

        protected override void Awake()
        {
            base.Awake();

            doubleJumpForce = doubleJumpForceMultiplier * jumpForce;
        }

        public void Jump()
        {
            if (CanJump())
            {
                Jump(JumpForce());
                currentJumpNum++;
            }
        }

        public override void Jump(Vector2 force, bool triggerJumping = true)
        {
            base.Jump(force, triggerJumping);

            NoAdjacentWall();
        }

        public bool CanJump()
        {
            return currentJumpNum < maxNumJumps;
        }

        public Vector2 JumpForce()
        {
            return OrientForce(currentJumpNum > 0 ? doubleJumpForce : jumpForce);
        }

        public void ResetJumpNum()
        {
            currentJumpNum = 0;
        }


        //WALL CLINGING

        public void BeginWallCling(/*bool airborne,*/ float rotationSpeed)
        {
            //if (airborne)
            //{
            //    TriggerLanding();
            //}

            //ResetJumpNum();
            TriggerLanding();

            RotateTowardsAdjacentWall(rotationSpeed);
        }

        public void MaintainWallCling(float rotationSpeed)
        {
            RotateTowardsAdjacentWall(rotationSpeed);
        }

        public void RotateTowardsAdjacentWall(float rotationSpeed)
        {
            transform.TweenTransformUpTowards(adjacentWallDirection.normalized, rotationSpeed);
        }

        public void EndWallCling()
        {
            transform.rotation = Quaternion.identity;
        }

        public void UpdateAdjacentWall(bool grounded, int n, float d)
        {

            var origin = ColliderCenterBack + 0.5f * myHeight * transform.up;
            var m = 3 * n;
            var spacing = myHeight / m;
            if (grounded)
            {
                spacing *= 0.95f;//to avoid feet casts hitting against steep slopes while walking
            }
            var delta = - spacing * transform.up;
            var length = d * myWidth;
            var lengthN = length / n;
            var direction = (int)CurrentOrientation * transform.right;

            var firstHit = Vector2.zero;
            var firstHitIndex = -1;
            int m2 = m / 2;

            //break height into thirds, each third having n pieces (total of 3n + 1 hits to check)
            for(int i = 0; i < m + 1; i++)
            {
                if (i > 2 * n && grounded)
                {
                    length -= lengthN;//to avoid feet casts hitting against steep slopes while walking
                }

                var hit = Physics2D.Raycast(origin + i * delta, direction, length, groundLayer);
                //Debug.DrawLine(origin + i * delta, origin + i * delta + direction * length, Color.red);

                if (!hit)
                {
                    continue;
                }
                else if (firstHitIndex == -1)
                {
                    firstHit = hit.point;
                    firstHitIndex = i;
                    if (i > m2)
                    {
                        break;
                    }
                }
                else if (firstHitIndex <= m2 && i - firstHitIndex >= n)
                {
                    facingWall = true;
                    adjacentWallDirection = firstHit - hit.point;
                    return;
                }
            }

            NoAdjacentWall();

            //in this case there may have been a second hit, but not in a way that counts as wall clinging
            if (firstHitIndex != -1 && (!grounded || firstHitIndex < 0.75f * m))
            {
                AwkwardWallMoment?.Invoke();
            }
        }

        protected virtual void NoAdjacentWall()
        {
            adjacentWallDirection = Vector2.up;
            facingWall = false;
        }


        //CLIMBING

        //and somewhere we need to trigger climbing state (and in entry, 
        //movement controller will configure its update functions etc.)

        public void TryGrabOntoClimbableObject()
        {
            //do an overlap circle from say 2/3 up body and latch onto nearest climbNode
        }

        public void OnBeginClimb()
        {
            //set rb to kinematic
            //turn off collider? but then you can't get hit by projectiles
            //just trying not to have collision with rope
            //you could also deactive rope colliders, since only player interacts with them
        }

        //to be called in fixedupdate by movement controller
        //(call even if move input zero, because we still need to update position and rotation)
        //ClimbingMovementOptions = climbSpeed & rotationSpeed
        public void UpdateClimb(float moveInput /*ClimbingMovementOptions*/)
        {
            //increment ClimbData position
            //if currentNode == null EndClimb()
            //set transform.position (+ offset)
            //  (later maybe tween position, which would allow potential to be thrown off rope at certain distance
            //  i.e. if rope is moving so rapidly that tween rate is too low to maintain position)
            //match rotation
        }

        public void EndClimb()
        {
            //set current node to null
            //re-enable normal physics
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AwkwardWallMoment = null;
        }
    }
}