using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AIMover : AdvancedMover
    {
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
                var rc = Physics2D.Raycast(rcOrigin, -transform.up, maxHeight, groundLayer);

                //Debug.DrawLine(origin + ((int)CurrentOrientation) * i * spacing * Vector2.right,
                //    origin + ((int)CurrentOrientation) * i * spacing * Vector2.right - maxHeight * Vector2.up,
                //    Color.green);

                if (!rc)
                {
                    distanceInFront = i * spacing;
                    return true;
                }

                if (Mathf.Abs(rc.point.y - hits[i - 1].y) > spacing * PhysicsTools.tan75)
                //more than 75deg slope detected
                {
                    distanceInFront = i * spacing;
                    return true;
                }

                hits[i] = rc.point;
            }
            return false;
        }

        //either move to AIMover or Physics tools
        public virtual bool CanJumpGap(out Vector2 landingPoint)
        {
            landingPoint = transform.position;

            if (!CanJump())
            {
                return false;
            }

            Trajectory jumpTrajectory =
                PhysicsTools.ImpulseForceTrajectory(this, JumpForce());

            float dt = jumpTrajectory.timeToReturnToLevel / 20;

            for (int i = 10; i <= 30; i++)
            {
                var hitOrigin = jumpTrajectory.position(i * dt);
                var hit = Physics2D.Raycast(hitOrigin, -Vector2.up, 0.5f * myHeight, groundLayer);

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

                    return ground.y < Mathf.Abs(ground.x) * PhysicsTools.tan60;
                }
            }
            return false;
        }
    }
}