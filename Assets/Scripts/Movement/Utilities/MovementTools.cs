using UnityEngine;

namespace RPGPlatformer.Movement
{
    public static class MovementTools
    {
        public const float tan60 = 1.73f;
        public const float tan75 = 3.73f;
        public const float PI2 = Mathf.PI / 2;

        //this is the trajectory of the mover.ColliderCenterBottom point
        //assumes velocity will only be acted on by gravity (e.g. moveSpeed will not change during the trajectory)
        public static Trajectory ImpulseForceTrajectory(AdvancedMover mover, Vector2 impulseForce)
        {
            Vector2 g = Physics2D.gravity;
            Vector2 v0 = (impulseForce / mover.Rigidbody.mass)
                + mover.RunSpeed * (int)mover.CurrentOrientation * (Vector2)mover.transform.right;

            Vector2 Position(float t)
            {
                return 0.5f * t * t * g + t * v0 + (Vector2)mover.ColliderCenterBottom;
            }

            return new(mover.ColliderCenterBottom, -2 * v0.y / g.y, Position);
        }

        public static Vector2 CCWPerp(this Vector2 v)
        {
            return new Vector2(-v.y, v.x);
        }

        //approximately rotates from unit vector d1 towards d2 at given rotationalSpeed over time dt.
        //We do take a magnitude but it's better than inverse trig fcts.
        //d1 and d2 should be normalized, but return value will not be.
        //Rotational speed in radians per second.
        public static Vector2 CheapRotationalTween(Vector2 d1, Vector2 d2, float rotationalSpeed, float dt)
        {
            var d = d2 - d1;
            return d1 + (rotationalSpeed / d.magnitude ) * dt * d;
        }
    }
}