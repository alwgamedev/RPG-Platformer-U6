using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AdvancedMover : Mover
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

        //public void UpdateAdjacentWall(bool airborne)
        //{
        //    if (verifyingJump)
        //    {
        //        NoAdjacentWall();
        //        return;
        //    }

        //    //TO-DO: add more hits

        //    var upperHit = Physics2D.Raycast(ColliderCenterBack + 0.4f * myHeight * Vector3.up,
        //        (int)CurrentOrientation * Vector3.right, 1.75f * myWidth, LayerMask.GetMask("Ground"));
        //    var midHit = Physics2D.Raycast(ColliderCenterBack + 0.1f * myHeight * Vector3.up, 
        //        (int)CurrentOrientation * Vector3.right, 1.75f * myWidth, LayerMask.GetMask("Ground")); 
        //    var lowerHit = Physics2D.Raycast(ColliderCenterBack - 0.2f * myHeight * Vector3.up,
        //        (int)CurrentOrientation * Vector3.right, 1.75f * myWidth, LayerMask.GetMask("Ground"));

        //    //Debug.DrawLine(ColliderCenterBack + 0.4f * myHeight * Vector3.up,
        //    //    ColliderCenterBack + 0.4f * myHeight * Vector3.up + 1.75f * myWidth
        //    //    * (int)CurrentOrientation * Vector3.right, Color.blue);
        //    //Debug.DrawLine(ColliderCenterBack + 0.1f * myHeight * Vector3.up, ColliderCenterBack
        //    //    + 0.1f * myHeight * Vector3.up + 1.75f * myWidth * (int)CurrentOrientation * Vector3.right, Color.blue);
        //    //Debug.DrawLine(ColliderCenterBack - 0.2f * myHeight * Vector3.up,
        //    //    ColliderCenterBack - 0.2f * myHeight * Vector3.up + 1.75f * myWidth
        //    //    * (int)CurrentOrientation * Vector3.right, Color.blue);

        //    if (midHit && lowerHit)
        //    {
        //        facingWall = true;
        //        adjacentWallDirection = midHit.point - lowerHit.point;
        //        return;
        //    }
        //    else if (midHit && upperHit)
        //    {
        //        facingWall = true;
        //        adjacentWallDirection = upperHit.point - midHit.point;
        //        return;
        //    }
        //    else if (!midHit && upperHit)
        //    {
        //        facingWall = true;
        //        adjacentWallDirection = Vector3.up;
        //        return;
        //    }
        //    else if (midHit || lowerHit)
        //    {
        //        if (airborne)
        //        {
        //            TriggerLanding();
        //            AwkwardWallMoment?.Invoke();
        //        }
        //    }

        //    NoAdjacentWall();
        //}

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


        //DROP OFF DETECTION (mainly for AI)

        public bool DropOffAhead(float maxHeight, HorizontalOrientation direction, out float distanceInFront)
        {
            distanceInFront = Mathf.Infinity;
            float spacing = 0.08f;
            maxHeight += 0.5f * myHeight;
            //shifting up higher to help detect step-ups
            //TO-DO: get ride of this and just change the serialized field in prefabs (just increase by 0.5 in all)
            Vector2 origin = (direction == HorizontalOrientation.right ? ColliderCenterRight : ColliderCenterLeft)
                + 0.5f * myHeight * transform.up;

            Vector2[] hits = new Vector2[16];
            hits[0] = ColliderCenterFront - groundednessTolerance * transform.up;

            //Debug.DrawLine(ColliderCenterFront, directlyBelowCharacter, Color.green);

            for (int i = 1; i <= 15; i++)
            {
                var rcOrigin = origin + ((int)direction) * i * spacing * (Vector2)transform.up;
                var rc = Physics2D.Raycast(rcOrigin, - transform.up, maxHeight, groundLayer);

                //Debug.DrawLine(origin + ((int)CurrentOrientation) * i * spacing * Vector2.right,
                //    origin + ((int)CurrentOrientation) * i * spacing * Vector2.right - maxHeight * Vector2.up,
                //    Color.green);

                if (!rc)
                {
                    distanceInFront = i * spacing;
                    return true;
                }

                if (Mathf.Abs(rc.point.y - hits[i - 1].y) > spacing * MovementTools.tan75)
                //more than 75deg slope detected
                {
                    distanceInFront = i * spacing;
                    return true;
                }

                hits[i] = rc.point;
            }
            return false;
        }

        public virtual bool CanJumpGap(out Vector2 landingPoint)
        {
            landingPoint = Transform.position;

            if (!CanJump())
            {
                return false;
            }

            Trajectory jumpTrajectory =
                MovementTools.ImpulseForceTrajectory(this, JumpForce());

            float dt = jumpTrajectory.timeToReturnToLevel / 20;

            for (int i = 10; i <= 30; i++)
            {
                var hitOrigin = jumpTrajectory.position(i * dt);
                var hit = Physics2D.Raycast(hitOrigin, - Vector2.up, 0.5f * myHeight, groundLayer);

                //Debug.DrawLine(hitOrigin, hitOrigin - 0.5f * myHeight * Vector2.up, Color.blue, 3);

                if (hit && hit.distance > 0)
                {
                    landingPoint = hit.point;

                    //check if landing area is level ground
                    //YES this is important because the hit could be the side of a shallow cliff
                    var hit1Origin = hitOrigin + ((int)CurrentOrientation) * myWidth * Vector2.right;
                    var hit1 = Physics2D.Raycast(hit1Origin, -Vector2.up, 0.5f * myHeight, groundLayer);

                    //Debug.DrawLine(hit1Origin, hit1Origin - 0.5f * myHeight * Vector2.up, Color.red, 3);

                    if (!hit1 || hit1.distance == 0)
                    {
                        continue;
                    }

                    var ground = hit1.point - hit.point;

                    return ground.y < Mathf.Abs(ground.x) * MovementTools.tan60;
                }
            }
            return false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AwkwardWallMoment = null;
        }
    }
}