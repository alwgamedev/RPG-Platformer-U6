using System;
using UnityEngine;
using RPGPlatformer.Core;
using System.Threading.Tasks;
using System.Threading;

namespace RPGPlatformer.Movement
{
    public enum HorizontalOrientation
    {
        right = 1, left = -1
    }

    public class Mover : StateDriver, IMover
    {
        [SerializeField] protected float groundednessToleranceFactor = 0.7f;
        [SerializeField] protected bool unfreezeRotationOnDeath;
        [SerializeField] protected Vector2 deathForce = 120 * Vector2.right + 120 * Vector2.up;
        [SerializeField] protected float deathTorque = 10;
        [SerializeField] protected bool treatContactCharacterAsGround;
        [SerializeField] protected float adjustedHalfWidthFactor = 0.45f;

        protected int groundLayer;
        protected Collider2D myCollider;
        protected Rigidbody2D myRigidbody;
        protected float myHeight;
        protected float myWidth;
        protected float adjustedHalfWidth;
        protected float defaultGravityScale;
        protected bool verifyingJump;
        protected bool verifyingFreefall;//TO-DO: could we just have one variable "verifyingAirborne"?
        protected float groundednessTolerance;
        protected RaycastHit2D rightGroundHit;
        protected RaycastHit2D leftGroundHit;

        //public Transform transform => base.transform;
        public Rigidbody2D Rigidbody => myRigidbody;
        public virtual float MaxSpeed { get; set; }
        public float Width => myWidth;
        public float Height => myHeight;
        public bool VerifyingFreefall => verifyingFreefall;
        public bool VerifyingJump => verifyingJump;
        public Vector3 ColliderCenterRight => myCollider.bounds.center + adjustedHalfWidth * base.transform.right;
        public Vector3 ColliderCenterLeft => myCollider.bounds.center - adjustedHalfWidth * base.transform.right;
        public Vector3 ColliderCenterFront => myCollider.bounds.center + adjustedHalfWidth
            * (int)CurrentOrientation * base.transform.right;
        public Vector3 ColliderCenterBack => myCollider.bounds.center - adjustedHalfWidth
            * (int)CurrentOrientation * base.transform.right;
        public Vector3 ColliderCenterBottom => myCollider.bounds.center - 0.5f * myHeight * base.transform.up;
        public bool FacingRight => CurrentOrientation == HorizontalOrientation.right;
        public HorizontalOrientation CurrentOrientation { get; protected set; }

        public event Action<HorizontalOrientation> DirectionChanged;
        public event Action OnJump;
        public event Action OnFreefall;
        public event Action FreefallVerified;

        protected virtual void Awake()
        {
            myCollider = GetComponent<Collider2D>();
            myRigidbody = GetComponent<Rigidbody2D>();

            CurrentOrientation = HorizontalOrientation.right;

            myHeight = myCollider.bounds.max.y - myCollider.bounds.min.y;
            myWidth = myCollider.bounds.max.x - myCollider.bounds.min.x;
            adjustedHalfWidth = adjustedHalfWidthFactor * myWidth;

            groundLayer = LayerMask.GetMask("Ground");
            if (treatContactCharacterAsGround)
            {
                groundLayer = groundLayer | LayerMask.GetMask("Contact Character");
            }

            groundednessTolerance = groundednessToleranceFactor * myHeight;

            defaultGravityScale = myRigidbody.gravityScale;
        }

        public virtual void UpdateGroundHits()
        {
            rightGroundHit = Physics2D.Raycast(ColliderCenterRight, -base.transform.up, groundednessTolerance,
                groundLayer);
            leftGroundHit = Physics2D.Raycast(ColliderCenterLeft, -base.transform.up, groundednessTolerance,
                groundLayer);

            //if (rightGroundHit && leftGroundHit)
            //{
            //    Debug.DrawLine(ColliderCenterRight, rightGroundHit.point, Color.red);
            //    Debug.DrawLine(ColliderCenterLeft, leftGroundHit.point, Color.red);
            //}
        }

        public virtual void UpdateState(bool jumping, bool freefalling)
        {
            if (rightGroundHit || leftGroundHit)
            {
                if ((jumping && !verifyingJump) || (freefalling && !verifyingFreefall))
                {
                    TriggerLanding();
                }
            }
            else if (!jumping && !freefalling)
            {
                TriggerFreefall();
            }
        }

        public void SetGravityScale(float f)
        {
            myRigidbody.gravityScale = f;
        }

        public void ReturnGravityScaleToDefault()
        {
            SetGravityScale(defaultGravityScale);
        }

        protected virtual void TriggerLanding()
        {
            Trigger(typeof(Grounded).Name);
        }

        protected virtual void TriggerFreefall()
        {
            OnFreefall?.Invoke();//cancels any ongoing verification
            Trigger(typeof(Freefall).Name);
            VerifyFreefall();
        }

        protected virtual void TriggerJumping()
        {
            OnJump?.Invoke();
            Trigger(typeof(Jumping).Name);
            VerifyJump();
        }

        public virtual void Move(Vector2 relV, Vector2 direction, MovementOptions options)
        {
            Move(relV, direction, MaxSpeed, options);
        }

        public virtual void MoveWithoutAcceleration(Vector2 direction, MovementOptions options)
        {
            MoveWithoutAcceleration(direction, MaxSpeed, options);
        }

        //direction assumed to be normalized
        public virtual void Move(Vector2 relV, Vector2 direction, float maxSpeed, MovementOptions options)
        {
            if (direction == Vector2.zero)
            {
                return;
            }

            if (options.RotateToDirection)
            {
                RotateTowardsMovementDirection(direction, options);
            }

            var v = options.ClampXVelocityOnly ?
                new Vector2(relV.x, 0) : relV;
            var dot = Vector2.Dot(v, direction);

            if (dot <= 0 || v.sqrMagnitude < maxSpeed * maxSpeed)//then we'll assume the angle between v and direction to be small (in fact, 0)
            {
                myRigidbody.linearVelocity += options.Acceleration * Time.deltaTime * direction;
            }
        }

        //to-do: clamping acceleration
        //if dot <= 0, no clamp (although still maybe clamp because sometimes our guy goes shooting off weirdly)
        //otherwise, assume angle between velocity and direction is zero, because we expect small changes in angle
        //then just need |dv| <= M - |v| (where |dv| = accel * dt, accel to be the clamped acceleration)
        //assuming |v| close to M, use a taylor polynomial centered at M to quickly approximate |v|

        public virtual void MoveWithoutAcceleration(Vector2 direction, float maxSpeed, MovementOptions options)
        {
            if (direction == Vector2.zero) return;

            if (options.RotateToDirection)
            {
                RotateTowardsMovementDirection(direction, options);
            }

            myRigidbody.linearVelocity = maxSpeed * direction;
        }

        public void RotateTowardsMovementDirection(Vector2 moveDirection, MovementOptions options)
        {
            var tUp = FacingRight ? options.ClampedTrUpGivenGoalTrRight(moveDirection)
                : options.ClampedTrUpGivenGoalTrLeft(moveDirection);
            TweenTransformUpTowards(tUp.normalized, options.RotationSpeed);
        }

        //goal transformUp should be normalized
        public void TweenTransformUpTowards(Vector2 transformUp, float rotationalSpeed)
        {
            var tweened = MovementTools.CheapRotationalTween(base.transform.up, transformUp, 
                rotationalSpeed, Time.deltaTime);
            base.transform.rotation = Quaternion.LookRotation(base.transform.forward, tweened);
        }

        public virtual void Stop(bool maintainVerticalVelocity = true)
        {
            if (maintainVerticalVelocity)
            {
                myRigidbody.linearVelocity = new Vector2(0, myRigidbody.linearVelocityY);
            }
            else
            {
                myRigidbody.linearVelocity = Vector2.zero;
            }
        }

        public virtual void Jump(Vector2 force, bool triggerJumping = true)
        {
            myRigidbody.AddForce(force, ForceMode2D.Impulse);
            if (triggerJumping)
            {
                TriggerJumping();
            }
        }

        public Vector2 OrientForce(Vector2 force)
        {
            force.x *= (int)CurrentOrientation;
            return force;
        }

        protected async void VerifyJump()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);
            try
            {
                OnJump += cts.Cancel;
                verifyingJump = true;
                await Task.Delay(200, cts.Token);
                verifyingJump = false;
            }
            catch (TaskCanceledException)
            {
                JumpVerificationCatch();
            }
            finally
            {
                JumpVerificationFinally(cts);
            }
        }

        protected virtual void PrepareJumpVerification(CancellationTokenSource cts)
        {
            OnJump += cts.Cancel;
        }

        protected virtual void JumpVerificationCatch()
        {
            verifyingJump = false;
            return;
        }

        protected virtual void JumpVerificationFinally(CancellationTokenSource cts)
        {
            OnJump -= cts.Cancel;
        }

        protected async void VerifyFreefall()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);
            try
            {
                PrepareFreefallVerification(cts);
                verifyingFreefall = true;
                await Task.Delay(100, cts.Token);
                verifyingFreefall = false;
                if (!leftGroundHit && !rightGroundHit)
                {
                    FreefallVerified?.Invoke();
                }
            }
            catch (TaskCanceledException)
            {
                FreefallVerificationCatch();
            }
            finally
            {
                FreefallVerificationFinally(cts);
            }
        }

        protected virtual void PrepareFreefallVerification(CancellationTokenSource cts)
        {
            OnFreefall += cts.Cancel;
            OnJump += cts.Cancel;
        }

        protected virtual void FreefallVerificationCatch()
        {
            verifyingFreefall = false;
            return;
        }

        protected virtual void FreefallVerificationFinally(CancellationTokenSource cts)
        {
            OnFreefall -= cts.Cancel;
            OnJump -= cts.Cancel;
        }

        public virtual void SetOrientation(HorizontalOrientation orientation, bool updateDirectionFaced, 
            bool flipWrtGlobalUp)
        {
            if (orientation != CurrentOrientation)
            {
                CurrentOrientation = orientation;

                if (updateDirectionFaced)
                {
                    UpdateDirectionFaced(flipWrtGlobalUp);
                }
            }
        }

        public virtual bool FacingWrongDirection()
        {
            return (int)CurrentOrientation * base.transform.localScale.x < 0;
        }

        public virtual void UpdateDirectionFaced(bool flipWrtGlobalUp)
        {
            if (FacingWrongDirection())
            {
                base.transform.localScale = new Vector3(- base.transform.localScale.x, 
                    base.transform.localScale.y, base.transform.localScale.z);
                if (flipWrtGlobalUp)
                {
                    base.transform.up = MovementTools.ReflectAlongUnitVector(Vector3.right, base.transform.up);
                }
                DirectionChanged.Invoke(CurrentOrientation);
            }
        }

        public Vector2 GroundDirectionVector()
        {

            if (rightGroundHit && leftGroundHit)
            {
                return (int)CurrentOrientation * (rightGroundHit.point - leftGroundHit.point).normalized;
            }

            return (int)CurrentOrientation * base.transform.right;
        }

        //could hook this up to an animation event
        public void OnDeath()
        {
            myRigidbody.freezeRotation = !unfreezeRotationOnDeath;
            myRigidbody.AddForce(OrientForce(deathForce), ForceMode2D.Impulse);
            myRigidbody.AddTorque((int)CurrentOrientation * deathTorque, ForceMode2D.Impulse);
        }

        public void OnRevival()
        {
            myRigidbody.freezeRotation = true;
            myRigidbody.rotation = 0;
            base.transform.position += myHeight / 8 * Vector3.up;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DirectionChanged = null;
            OnJump = null;
            OnFreefall = null;
            FreefallVerified = null;
        }
    }
}