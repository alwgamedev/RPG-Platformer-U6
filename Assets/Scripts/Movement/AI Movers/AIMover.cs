using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AIMover : AdvancedMover
    {

        public void SetDetectWalls(bool val)
        {
            detectWalls = val;
        }

        public virtual Vector2 EmergencyJumpForce()
        {
            return 0.5f * jumpForce * Vector2.right + 1.35f * jumpForce * Vector2.up;
        }

        //detects both drop offs and step-ups
        public bool DropOffInFront(float maxHeight, out float distanceInFront)
        {
            distanceInFront = Mathf.Infinity;
            float spacing = 0.1f;
            maxHeight += 0.5f * myHeight;//shifting up higher to help detect step-ups
            Vector2 origin = ColliderCenterFront + 0.5f * myHeight * Vector3.up;
            Vector2 directlyBelowCharacter = ColliderCenterFront - groundednessTolerance * Vector3.up;

            List<Vector2> hits = new() { directlyBelowCharacter };

            //Debug.DrawLine(ColliderCenterFront, directlyBelowCharacter, Color.green);

            for (int i = 1; i <= 10; i++)
            {
                var rcOrigin = origin + ((int)CurrentOrientation) * i * spacing * Vector2.right;
                var rc = Physics2D.Raycast(rcOrigin, -Vector2.up, maxHeight, LayerMask.GetMask("Ground"));

                //Debug.DrawLine(origin + ((int)CurrentOrientation) * i * spacing * Vector2.right,
                //    origin + ((int)CurrentOrientation) * i * spacing * Vector2.right - maxHeight * Vector2.up,
                //    Color.green);

                if (!rc)
                {
                    distanceInFront = i * spacing;
                    return true;
                }

                if (Mathf.Abs(rc.point.y - hits[i - 1].y) > spacing * (2 + MovementTools.sqrt3))
                //more than 75deg slope detected
                {
                    distanceInFront = i * spacing;
                    return true;
                }

                hits.Add(rc.point);
            }
            return false;
        }

        public virtual bool CanJumpGap(out Vector2 landingPoint)
        {
            landingPoint = default;

            if (!CanJump())
            {
                return false;
            }

            Trajectory jumpTrajectory =
                MovementTools.ImpulseForceTrajectory(this, OrientForce(JumpForce()));

            float dt = jumpTrajectory.timeToReturnToLevel / 20;

            for (int i = 10; i <= 30; i++)
            {
                Vector2 hitOrigin = jumpTrajectory.position(i * dt);
                var hit = Physics2D.Raycast(hitOrigin, -Vector2.up, 0.5f * myHeight,
                        LayerMask.GetMask("Ground"));

                //Debug.DrawLine(hitOrigin, hitOrigin - 0.5f * mover.Height * Vector2.up, Color.blue, 3);

                if (hit && hit.distance > 0)
                {
                    landingPoint = hit.point;

                    //check if landing area is level ground
                    var hit1Origin = hitOrigin + ((int)CurrentOrientation) * myWidth * Vector2.right;
                    var hit1 = Physics2D.Raycast(hit1Origin, -Vector2.up, 0.5f * myHeight,
                        LayerMask.GetMask("Ground"));

                    //Debug.DrawLine(hit1Origin, hit1Origin - 0.5f * mover.Height * Vector2.up, Color.red, 3);

                    if (!hit1 || hit1.distance == 0)
                    {
                        continue;
                    }

                    Vector2 ground = hit1.point - hit.point;
                    //Debug.DrawLine(hit.point, hit1.point, Color.yellow, 3);

                    float groundAngle = Mathf.Rad2Deg * Mathf.Atan2(ground.y, ground.x);
                    groundAngle = Mathf.Abs(groundAngle);
                    if (CurrentOrientation == HorizontalOrientation.left)
                    {
                        groundAngle = 180 - groundAngle;
                    }

                    if (groundAngle > 60)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}