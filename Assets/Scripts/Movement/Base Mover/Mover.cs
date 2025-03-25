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
        //protected RaycastHit2D midGroundHit;
        protected RaycastHit2D leftGroundHit;

        public Transform Transform => transform;
        public Rigidbody2D Rigidbody => myRigidbody;
        public virtual float MaxSpeed { get; set; }
        public float Width => myWidth;
        public float Height => myHeight;
        public bool VerifyingFreefall => verifyingFreefall;
        public bool VerifyingJump => verifyingJump;
        public Vector3 ColliderCenterRight => myCollider.bounds.center + adjustedHalfWidth * transform.right;
        public Vector3 ColliderCenterLeft => myCollider.bounds.center - adjustedHalfWidth * transform.right;
        public Vector3 ColliderCenterFront => myCollider.bounds.center + adjustedHalfWidth
            * (int)CurrentOrientation * transform.right;
        public Vector3 ColliderCenterBack => myCollider.bounds.center - adjustedHalfWidth 
            * (int)CurrentOrientation * transform.right;
        public Vector3 ColliderCenterBottom => myCollider.bounds.center - 0.5f * myHeight * transform.up;
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
            rightGroundHit = Physics2D.Raycast(ColliderCenterRight, -transform.up, groundednessTolerance,
                groundLayer);
            //midGroundHit = Physics2D.Raycast(myCollider.bounds.center, -transform.up, groundednessTolerance);
            leftGroundHit = Physics2D.Raycast(ColliderCenterLeft, -transform.up, groundednessTolerance,
                groundLayer);

            //if (rightGroundHit && leftGroundHit)
            //{
            //    Debug.DrawLine(ColliderCenterRight, rightGroundHit.point, Color.red);
            //    Debug.DrawLine(ColliderCenterLeft, leftGroundHit.point, Color.red);
            //}
        }

        public virtual void UpdateState(bool jumping, bool freefalling)
        {
            if (rightGroundHit || leftGroundHit /*|| midGroundHit*/)
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
            //jumping = true;
            OnJump?.Invoke();
            Trigger(typeof(Jumping).Name);
            VerifyJump();
        }

        public virtual void Move(Vector2 direction, MovementOptions options)
        {
            Move(direction, MaxSpeed, options);
        }

        public virtual void MoveWithoutAcceleration(Vector2 direction, MovementOptions options)
        {
            MoveWithoutAcceleration(direction, MaxSpeed, options);
        }

        //direction assumed to be normalized
        public virtual void Move(Vector2 direction, float maxSpeed, MovementOptions options)
        {
            if (direction == Vector2.zero)
            {
                return;
            }

            if (options.RotateToDirection)
            {
                RotateTowardsMovementDirection(direction, options);
            }

            var velocity = options.ClampXVelocityOnly ? 
                new Vector2(myRigidbody.linearVelocity.x, 0) : myRigidbody.linearVelocity;
            var dot = Vector2.Dot(velocity, direction);
            if (dot <= 0 || velocity.sqrMagnitude < maxSpeed * maxSpeed)
            {
                myRigidbody.linearVelocity += options.Acceleration * Time.deltaTime * direction;
            }
        }

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

        public void RotateTransformUpTo(Vector2 transformUp)
        {
            if (transformUp == Vector2.zero) return;
            transform.rotation = Quaternion.LookRotation(transform.forward, transformUp);
        }

        //goal transformUp should be normalized
        public void TweenTransformUpTowards(Vector2 transformUp, float rotationalSpeed)
        {
            var tweened = MovementTools.CheapRotationalTween(transform.up, transformUp, 
                rotationalSpeed, Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(transform.forward, tweened);
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
            //return (int)CurrentOrientation * force.x * Vector2.right + force.y * Vector2.up;
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
            return (int)CurrentOrientation * transform.localScale.x < 0;
        }

        public virtual void UpdateDirectionFaced(bool flipWrtGlobalUp)
        {
            if (FacingWrongDirection())
            {
                transform.localScale = new Vector3(- transform.localScale.x, 
                    transform.localScale.y, transform.localScale.z);
                if (flipWrtGlobalUp)
                {
                    transform.up = MovementTools.ReflectAlongUnitVector(Vector3.right, transform.up);
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

            return (int)CurrentOrientation * transform.right;
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
            transform.position += myHeight / 8 * Vector3.up;
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