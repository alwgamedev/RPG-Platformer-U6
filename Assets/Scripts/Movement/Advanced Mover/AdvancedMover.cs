using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AdvancedMover : Mover
    {
        [SerializeField] protected float runSpeed = 3;
        [SerializeField] protected float walkSpeed = 0.8f;
        [SerializeField] protected float swimSpeed = .25f;
        [SerializeField] protected int maxNumJumps = 2;
        [SerializeField] protected Vector2 jumpForce = 375 * Vector2.up;
        [SerializeField] protected float doubleJumpForceMultiplier = 1.18f;

        protected int currentJumpNum = 0;
        protected Vector2 doubleJumpForce;
        protected Vector2 adjacentWallDirection = Vector2.up;
        protected bool facingWall;

        public float RunSpeed => runSpeed;
        public float WalkSpeed => walkSpeed;
        public float SwimSpeed => swimSpeed;
        public virtual bool Running { get; set; }
        public bool FacingWall => facingWall;

        public event Action AwkwardWallMoment;

        protected override void Awake()
        {
            base.Awake();

            doubleJumpForce = doubleJumpForceMultiplier * jumpForce;
        }


        //JUMPING

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

        public void BeginWallCling(float rotationSpeed)
        {
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AwkwardWallMoment = null;
        }
    }
}