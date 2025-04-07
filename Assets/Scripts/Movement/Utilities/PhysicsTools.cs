using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public static class PhysicsTools
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

        /// <summary>
        /// Approximately rotates unit vector d1 towards unit vector d2 at given rotational speed (rad/sec)
        /// for time ellapsed dt. (Uses straight line approximation of arc length instead of
        /// calculating an exact angle, basically).
        /// </summary>
        public static Vector2 CheapRotationalTween(Vector2 d1, Vector2 d2, float rotationalSpeed, float dt)
        {
            var d = d2 - d1;
            return d1 + (rotationalSpeed / d.magnitude ) * dt * d;
        }

        /// <summary>
        /// Reflect v along axis u (u assumed to be unit vector). 
        /// (I.e. reflect across hyperplane spanned by v and u x v).
        /// </summary>
        public static Vector3 ReflectAlongUnitVector(Vector3 u, Vector3 v)
        {
            return v - 2 * Vector3.Dot(u, v) * u;
        }

        /// <summary>
        /// Rotates w around the axis perpendicular to plane spanned by a & b, by the angle that takes a to b.
        /// If a, b, or a x b is zero, returns w.
        /// </summary>
        public static Vector3 FromToRotation(Vector3 a, Vector3 b, Vector3 w, bool alreadyNormalized = false)
        {
            if (a == Vector3.zero || b == Vector3.zero)
            {
                return w;
            }

            if (!alreadyNormalized)
            {
                var A = a.magnitude;
                var B = b.magnitude; 
                a = a / A;
                b = b / B;
            }

            var c = Vector3.Cross(a, b);

            if (c == Vector3.zero)
            {
                return w;
            }

            var C = c.magnitude;
            var u = c / C;

            return Vector3.Dot(a, b) * w + Vector3.Cross(c, w) + Vector3.Dot(u, w) * u;
        }
    }
}