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

        protected int groundLayer;
        protected Collider2D myCollider;
        protected Rigidbody2D myRigidbody;
        protected float myHeight;
        protected float myWidth;
        protected bool verifyingJump;
        protected bool verifyingFreefall;//TO-DO: could we just have one variable "verifyingAirborne"?
        protected float groundednessTolerance;
        protected RaycastHit2D rightGroundHit;
        protected RaycastHit2D leftGroundHit;

        public Transform Transform => transform;
        public Rigidbody2D Rigidbody => myRigidbody;
        public int GroundLayer => groundLayer;
        public virtual float MaxSpeed { get; set; }
        public float Width => myWidth;
        public float Height => myHeight;
        public Vector3 ColliderCenterRight => myCollider.bounds.center + 0.45f * myWidth * transform.right;
        public Vector3 ColliderCenterLeft => myCollider.bounds.center - 0.45f * myWidth * transform.right;
        public Vector3 ColliderCenterFront => myCollider.bounds.center + 0.45f * myWidth 
            * (int)CurrentOrientation * transform.right;
        public Vector3 ColliderCenterBack => myCollider.bounds.center - 0.45f * myWidth 
            * (int)CurrentOrientation * transform.right;
        public Vector3 ColliderCenterBottom => myCollider.bounds.center - 0.5f * myHeight * transform.up;
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

            groundLayer = LayerMask.GetMask("Ground");
            if (treatContactCharacterAsGround)
            {
                groundLayer = groundLayer | LayerMask.GetMask("Contact Character");
            }

            groundednessTolerance = groundednessToleranceFactor * myHeight;
        }

        public virtual void UpdateGroundHits()
        {
            rightGroundHit = Physics2D.Raycast(ColliderCenterRight, -transform.up, groundednessTolerance,
                groundLayer);
            leftGroundHit = Physics2D.Raycast(ColliderCenterLeft, -transform.up, groundednessTolerance,
                groundLayer);

            //Debug.DrawLine(ColliderCenterRight, ColliderCenterRight - groundednessTolerance * transform.up);
            //Debug.DrawLine(ColliderCenterLeft, ColliderCenterLeft - groundednessTolerance * transform.up);
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
                transform.rotation = options.RotateTransformRightTo((int)CurrentOrientation * direction);
            }

            var velocity = options.ClampXVelocityOnly ? 
                new Vector2(myRigidbody.linearVelocity.x, 0) : myRigidbody.linearVelocity;
            var dot = Vector2.Dot(velocity, direction);
            if (dot <= 0 || velocity.magnitude < maxSpeed)
            {
                myRigidbody.linearVelocity += options.Acceleration * Time.deltaTime * direction;
            }
        }

        public virtual void MoveWithoutAcceleration(Vector2 direction, float maxSpeed, MovementOptions options)
        {
            if (direction == Vector2.zero) return;

            if (options.RotateToDirection)
            {
                transform.rotation = options.RotateTransformRightTo((int)CurrentOrientation * direction);
            }

            myRigidbody.linearVelocity = maxSpeed * direction;
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
                await Task.Delay(200, cts.Token);
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

        public virtual void SetOrientation(HorizontalOrientation orientation, bool updateDirectionFaced)
        {
            if (orientation != CurrentOrientation)
            {
                CurrentOrientation = orientation;

                if (updateDirectionFaced)
                {
                    UpdateDirectionFaced();
                }
            }
        }

        public virtual bool FacingWrongDirection()
        {
            return (int)CurrentOrientation * transform.localScale.x < 0;
        }

        public virtual void UpdateDirectionFaced()
        {
            if (FacingWrongDirection())
            {
                transform.localScale = new Vector3(- transform.localScale.x, transform.localScale.y, transform.localScale.z);
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