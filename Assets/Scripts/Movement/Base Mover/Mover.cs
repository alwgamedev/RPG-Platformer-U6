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

        protected readonly string jumpingName = typeof(Jumping).Name;
        protected readonly string freefallName = typeof(Freefall).Name;
        protected readonly string swimmingName = typeof(Swimming).Name;

        protected int groundLayer;
        //protected int waterLayer;
        protected Collider2D myCollider;
        protected Rigidbody2D myRigidbody;
        protected float myHeight;
        protected float myWidth;
        protected float adjustedHalfWidth;
        protected float defaultGravityScale;
        protected bool verifyingJump;
        protected bool verifyingFreefall;
        protected float groundednessTolerance;
        protected RaycastHit2D rightGroundHit;
        protected RaycastHit2D leftGroundHit;
        protected BuoyantObject swimmer;

        public bool InWater => swimmer && swimmer.InWater;
        public Rigidbody2D Rigidbody => myRigidbody;
        public virtual float MaxSpeed { get; set; }
        public float Width => myWidth;
        public float Height => myHeight;
        public bool VerifyingFreefall => verifyingFreefall;
        public bool VerifyingJump => verifyingJump;
        public Vector3 CenterPosition => myCollider.bounds.center;
        public Vector3 ColliderCenterRight => CenterPosition + adjustedHalfWidth * transform.right;
        public Vector3 ColliderCenterLeft => CenterPosition - adjustedHalfWidth * transform.right;
        public Vector3 ColliderCenterFront => CenterPosition + adjustedHalfWidth
            * (int)CurrentOrientation * transform.right;
        public Vector3 ColliderCenterBack => CenterPosition - adjustedHalfWidth
            * (int)CurrentOrientation * transform.right;
        public Vector3 ColliderCenterBottom => CenterPosition - 0.5f * myHeight * transform.up;
        public bool FacingRight => CurrentOrientation == HorizontalOrientation.right;
        public HorizontalOrientation CurrentOrientation { get; protected set; }

        public event Action<HorizontalOrientation> DirectionChanged;
        public event Action OnJump;
        public event Action OnFreefall;
        public event Action FreefallVerified;
        public event Action WaterExited;

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

            //waterLayer = LayerMask.GetMask("Water");

            groundednessTolerance = groundednessToleranceFactor * myHeight;

            defaultGravityScale = myRigidbody.gravityScale;

            swimmer = GetComponent<BuoyantObject>();
        }

        public virtual void UpdateGroundHits()
        {
            rightGroundHit = Physics2D.Raycast(ColliderCenterRight, - transform.up, groundednessTolerance,
                groundLayer);
            leftGroundHit = Physics2D.Raycast(ColliderCenterLeft, - transform.up, groundednessTolerance,
                groundLayer);
        }

        //called in movement controller's start
        public virtual void InitializeState()
        {
            if (swimmer)
            {
                swimmer.WaterEntered += EnterWater;
                swimmer.WaterExited += ExitWater;
            }

            UpdateGroundHits();

            if (rightGroundHit || leftGroundHit)
            {
                TriggerLanding();
            }
            else
            {
                TriggerFreefall();
            }
        }

        //returns whether a state change was triggered
        public virtual void UpdateState(string currentState/*bool jumping, bool freefalling*/)
        {
            bool jumping = currentState == jumpingName;
            bool freefalling = currentState == freefallName;
            bool swimming = currentState == swimmingName;

            if (rightGroundHit || leftGroundHit)
            {
                if (swimming || (jumping && !verifyingJump) || (freefalling && !verifyingFreefall))
                {
                    TriggerLanding();
                }
            }
            else if (InWater && !swimming && !verifyingJump && !verifyingFreefall)
            {
                Trigger(typeof(Swimming).Name);
            }
            else if (!InWater && !jumping && !freefalling)
            {
                TriggerFreefall();
            }
        }

        //protected virtual bool ShouldTriggerLandingIfGroundHit(string currentState/*bool jumping, bool freefalling*/)
        //{
        //    if (currentState == typeof(Jumping).Name && !verifyingJump)
        //    {
        //        return true;
        //    }
        //    if (currentState == )
        //    //return (jumping && !verifyingJump) || (freefalling && !verifyingFreefall);
        //}

        //protected virtual bool ShouldTriggerFreefallIfNoGroundHit(string currentState/*bool jumping, bool freefalling*/)
        //{
        //    return !jumping && !freefalling;
        //}

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

        protected virtual void TriggerSwimming()
        {
            Trigger(typeof(Swimming).Name);
        }

        public virtual void Accelerate(Vector2 a)
        {
            myRigidbody.AddForce(myRigidbody.mass * a);
        }

        public virtual void Move(bool backingUp, Vector2 relV, Vector2 direction, MovementOptions options)
        {
            Move(backingUp, relV, direction, MaxSpeed, options);
        }

        public virtual void MoveWithoutAcceleration(bool backingUp, Vector2 direction, MovementOptions options)
        {
            MoveWithoutAcceleration(backingUp, direction, MaxSpeed, options);
        }

        public virtual void Move(bool backingUp, Vector2 relV, Vector2 moveDirection,
            float maxSpeed, MovementOptions options)
        {
            myRigidbody.Move(FacingRight, backingUp, relV, moveDirection, maxSpeed, options);
        }

        public virtual void MoveWithoutAcceleration(bool backingUp, Vector2 direction, float maxSpeed, MovementOptions options)
        {
            myRigidbody.MoveWithoutAcceleration(FacingRight, backingUp, direction, maxSpeed, options);
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

        public Vector2 OrientForce(Vector2 force)
        {
            force.x *= (int)CurrentOrientation;
            return force;
        }


        //JUMPING

        public virtual void Jump(Vector2 force, bool triggerJumping = true)
        {
            myRigidbody.AddForce(force, ForceMode2D.Impulse);
            if (triggerJumping)
            {
                TriggerJumping();
            }
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


        //FREEFALL

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


        //WATER

        private void EnterWater(BuoyancySource b)
        {
            Trigger(typeof(Swimming).Name);
        }

        private void ExitWater()
        {
            WaterExited?.Invoke();

        }

        //private void OnTriggerEnter2D(Collider2D collider)
        //{
        //    if (1 << collider.gameObject.layer == waterLayer)
        //    {
        //        inWater = true;
        //        Trigger(typeof(Swimming).Name);
        //    }
        //}

        //protected virtual void OnTriggerExit2D(Collider2D collider)
        //{
        //    if (1 << collider.gameObject.layer == waterLayer)
        //    {
        //        inWater = false;
        //        WaterExited?.Invoke();
        //    }
        //}

        public virtual void HandleWaterExit(bool stillInSwimmingState)
        {
            if (stillInSwimmingState)
            {
                TriggerFreefall();
            }
        }


        //ORIENTATION

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
                var s = transform.localScale;
                s.x *= -1;
                transform.localScale = s;

                if (flipWrtGlobalUp)//yes this needs to be in addition to the above flip
                {
                    s = transform.up;
                    s.x *= -1;
                    transform.up = s;
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
        public virtual void PlayDeathEffect()
        {
            if (unfreezeRotationOnDeath)
            {
                myRigidbody.freezeRotation = false;
            }
            myRigidbody.AddForce(OrientForce(deathForce), ForceMode2D.Impulse);
            myRigidbody.AddTorque((int)CurrentOrientation * deathTorque, ForceMode2D.Impulse);
        }

        public virtual void OnRevival()
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
            WaterExited = null;
        }
    }
}