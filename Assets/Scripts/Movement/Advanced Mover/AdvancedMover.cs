using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AdvancedMover : Mover
    {
        [SerializeField] protected float runSpeed = 3;
        [SerializeField] protected float walkSpeed = 0.8f;
        [SerializeField] protected float swimmingRunSpeed = 2;
        [SerializeField] protected float swimmingWalkSpeed = 1;
        [SerializeField] protected int maxNumJumps = 2;
        [SerializeField] protected Vector2 jumpForce = 375 * Vector2.up;
        [SerializeField] protected float doubleJumpForceMultiplier = 1.18f;

        protected int currentJumpNum = 0;
        protected Vector2 doubleJumpForce;
        protected Vector2 adjacentWallDirection = Vector2.up;
        protected bool facingWall;

        public float RunSpeed => runSpeed;
        public float WalkSpeed => walkSpeed;
        public float SwimmingWalkSpeed => swimmingWalkSpeed;
        public float SwimmingRunSpeed => swimmingRunSpeed;
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
            var length = d * myWidth;//raycast length
            var lengthN = length / n;
            var direction = (int)CurrentOrientation * transform.right;

            RaycastHit2D hit;
            var firstHit = Vector2.zero;
            var firstHitIndex = -1;
            float m2 = 0.5f * m;
            float m4 = 0.5f * m2;

            //break height into thirds, each third having n pieces (total of 3n + 1 hits to check)

            Vector2 v;
            int i = 0;
            while (i <= m)
            {
                if (i > 2 * n && grounded)
                {
                    length -= lengthN;//to avoid feet casts hitting against steep slopes while walking
                }

                hit = Physics2D.Raycast(origin + i * delta, direction, length, groundLayer);
                //Debug.DrawLine(origin + i * delta, origin + i * delta + direction * length, Color.red);

                if (!hit)
                {
                    i++;
                    continue;
                }
                else if (firstHitIndex < m4)
                {
                    firstHitIndex = i;
                    if (i > m2)
                    {
                        break;
                    }
                    else if (i >= m4)
                    {
                        firstHit = hit.point;
                        i = n + firstHitIndex;
                        //^we require firstHitIndex - secondHitIndex >= n (i.e. separated by 1/3 of height)
                        continue;
                    }

                    i++;
                    continue;
                    //continue until find a hit with i >= m4
                    //(if only hit is < m4 then we're just hitting our head and it should count as an
                    //awkward wall moment)
                }
                else
                {
                    v = firstHit - hit.point;
                    if (Mathf.Abs(v.y) < Mathf.Abs(v.x))//smaller than 45 deg angle
                    {
                        i++;
                        continue;
                    }
                    else
                    {
                        facingWall = true;
                        adjacentWallDirection = v;
                        return;
                    }
                }
            }

            NoAdjacentWall();

            //in this case there may have been a second hit, but not in a way that counts as wall clinging
            if (firstHitIndex != -1 && (!grounded || firstHitIndex < 3 * m4))
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