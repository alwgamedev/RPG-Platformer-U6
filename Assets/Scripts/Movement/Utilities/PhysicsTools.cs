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
            return d1 + rotationalSpeed  * dt * ((d2 - d1).normalized);
        }

        /// <summary>
        /// Approximately rotates unit vector d1 towards unit vector d2 at given rotational speed (rad/sec)
        /// for time ellapsed dt. (Uses straight line approximation of arc length instead of
        /// calculating an exact angle, basically).
        /// </summary>
        public static Vector2 CheapRotationalTweenClamped(Vector2 d1, Vector2 d2, float rotationalSpeed, float dt)
        {
            var d = d2 - d1;
            return d1 + Mathf.Clamp(rotationalSpeed * dt / d.magnitude, 0, 1) * d;
        }

        /// <summary>
        /// Reflect v across hyperplane perpendicular to unit vector u.
        /// </summary>
        public static Vector3 ReflectAcrossPerpendicularHyperplane(Vector3 u, Vector3 v)
        {
            return v - 2 * Vector3.Dot(u, v) * u;
        }

        /// <summary>
        /// Rotates w around the axis perpendicular to plane spanned by a & b, by the angle that takes a to b.
        /// If a, b, or a x b is zero, returns w.
        /// </summary>
        public static Vector3 FromToRotation(Vector3 a, Vector3 b, Vector3 w, bool alreadyNormalized = false)
        {
            if (!alreadyNormalized)
            {
                a = a.normalized;
                b = b.normalized;
            }

            if (a == Vector3.zero || b == Vector3.zero)
            {
                return w;
            }

            var c = Vector3.Cross(a, b);
            var u = c.normalized;

            if (u == Vector3.zero)
            {
                return w;
            }

            return Vector3.Dot(a, b) * w + Vector3.Cross(c, w) + Vector3.Dot(u, w) * u;
        }

        /// <summary>
        /// Sets total force and velocity to zero, so you don't have any lingering velocity
        /// that screws things up. Should use this pretty much everywhere you plan to toggle
        /// an rb btwn kinematic and dynamic (e.g. climbing, evil root grab).
        /// </summary>
        public static void SetKinematic(this Rigidbody2D rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.totalForce = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }

        /// <summary>
        /// moveDirection assumed normalized.
        /// relV is current "relative" velocity (which is usually just rb.linearVelocity, unless
        /// you want velocity to be taken relative to a moving platform, e.g. when player has mounted shuttlehawk)
        /// </summary>
        public static void Move(this Rigidbody2D rb, bool facingRight, bool backingUp,
            Vector2 relV, Vector2 moveDirection, float maxSpeed, MovementOptions options)
        {
            if (moveDirection == Vector2.zero)
            {
                return;
            }

            if (options.RotateToDirection)
            {
                rb.transform.RotateTowardsMovementDirection(facingRight, 
                    backingUp ? - moveDirection : moveDirection, options);
            }

            var v = options.ClampXVelocityOnly ?
                new Vector2(relV.x, 0) : relV;
            var dot = Vector2.Dot(v, moveDirection);

            if (dot <= 0 || v.sqrMagnitude < maxSpeed * maxSpeed)
            {
                rb.AddForce(rb.mass * options.Acceleration * moveDirection);
            }
        }

        /// <summary>
        /// moveDirection assumed normalized
        /// </summary>
        public static void MoveWithoutAcceleration(this Rigidbody2D rb, bool facingRight, bool backingUp,
            Vector2 moveDirection, float maxSpeed, MovementOptions options)
        {
            if (moveDirection == Vector2.zero) return;

            if (options.RotateToDirection)
            {
                rb.transform.RotateTowardsMovementDirection(facingRight,
                    backingUp ? - moveDirection : moveDirection, options);
            }

            rb.linearVelocity = maxSpeed * moveDirection;
        }

        /// <summary>
        /// move direction is assumed normalized
        /// </summary>
        public static void RotateTowardsMovementDirection(this Transform t, bool moveDirectionIsGoalTrRight, 
            Vector2 moveDirection, MovementOptions options)
        {
            var tUp = moveDirectionIsGoalTrRight ? options.ClampedTrUpGivenGoalTrRight(moveDirection)
                : options.ClampedTrUpGivenGoalTrLeft(moveDirection);
            t.TweenTransformUpTowards(tUp, options.RotationSpeed);
        }

        /// <summary>
        /// goalTransformup assumed to be normalized
        /// </summary>
        public static void TweenTransformUpTowards(this Transform t, Vector2 goalTransformUp, float rotationalSpeed)
        {
            var tweened = CheapRotationalTweenClamped(t.up, goalTransformUp,
                rotationalSpeed, Time.deltaTime);
            t.rotation = Quaternion.LookRotation(t.forward, tweened);
        }
    }
}