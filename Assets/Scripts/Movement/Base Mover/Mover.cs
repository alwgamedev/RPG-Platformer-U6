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
        [SerializeField] float groundednessToleranceFactor = 0.7f;
        [SerializeField] bool unfreezeRotationOnDeath;
        [SerializeField] Vector2 deathForce = 120 * Vector2.right + 120 * Vector2.up;
        [SerializeField] float deathTorque = 10;

        protected Collider2D myCollider;
        protected Rigidbody2D myRigidbody;
        protected float myHeight;
        protected float myWidth;
        //protected bool jumping;
        //protected bool freefalling;
        protected bool verifyingJump;
        protected bool verifyingFreefall;
        protected float groundednessTolerance;
        protected RaycastHit2D rightGroundHit;
        protected RaycastHit2D leftGroundHit;

        public Transform Transform => transform;
        public Rigidbody2D Rigidbody => myRigidbody;
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
        //public event Action OnDestroyed;

        protected virtual void Awake()
        {
            myCollider = GetComponent<Collider2D>();
            myRigidbody = GetComponent<Rigidbody2D>();

            CurrentOrientation = HorizontalOrientation.right;

            myHeight = myCollider.bounds.max.y - myCollider.bounds.min.y;
            myWidth = myCollider.bounds.max.x - myCollider.bounds.min.x;

            groundednessTolerance = groundednessToleranceFactor * myHeight;//a little extra than 0.5f * height, because sometimes the
            //ground collider is a bit below the surface (and we don't want to be randomly losing groundedness
            //as we walk over uneven terrain. Also the back hit needs to go quite far on steep terrain)

            //may make this higher for some AI (because they check for dropoffs only when moving grounded,
            //and when they don't check for dropoffs they can walk off cliffs)
        }

        //protected virtual void Update()
        //{
        //    UpdateGroundHits();
        //    UpdateState();
        //}

        public virtual void UpdateGroundHits()
        {
            rightGroundHit = Physics2D.Raycast(ColliderCenterRight, -transform.up, groundednessTolerance,
                LayerMask.GetMask("Ground"));
            leftGroundHit = Physics2D.Raycast(ColliderCenterLeft, -transform.up, groundednessTolerance,
                LayerMask.GetMask("Ground"));
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
            //jumping = false;
            //freefalling = false;
            Trigger(typeof(Grounded).Name);
        }

        protected virtual void TriggerFreefall()
        {
            //freefalling = true;
            OnFreefall?.Invoke();//cancels any ongoing verification
            VerifyFreefall();
            Trigger(typeof(Freefall).Name);
        }

        protected virtual void TriggerJumping()
        {
            //jumping = true;
            OnJump?.Invoke();
            Trigger(typeof(Jumping).Name);
            VerifyJump();
        }

        //Direction is assumed to be "right pointing" (as transform.right always points right in new system)
        //(it will be multiplied by current orientation)
        public void Move(float acceleration, float maxSpeed, Vector2 direction, bool rotateToDirection = true, 
            bool clampXOnly = false)
        {
            if (direction == Vector2.zero)
            {
                return;
            }

            if (rotateToDirection)
            {
                RotateToDirection(direction);
            }

            direction *= (int)CurrentOrientation;
            var velocity = clampXOnly ? new Vector2(myRigidbody.linearVelocity.x, 0) : myRigidbody.linearVelocity;
            var dot = Vector2.Dot(velocity, direction);
            if (dot <= 0 || velocity.magnitude < maxSpeed)
            {
                myRigidbody.linearVelocity += acceleration * Time.deltaTime * direction;
            }
        }

        public void MoveWithoutAcceleration(float maxSpeed, Vector2 direction, bool rotateToDirection = false)
        {
            if (direction == Vector2.zero) return;

            if (rotateToDirection)
            {
                RotateToDirection(direction);
            }

            direction *= (int)CurrentOrientation;
            myRigidbody.linearVelocity = maxSpeed * direction;
        }

        public void RotateToDirection(Vector2 direction)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction.CCWPerp());
            //CCWPerp always works because our transform.forward never changes
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

        public virtual void Jump(Vector2 force)
        {
            myRigidbody.AddForce(OrientForce(force), ForceMode2D.Impulse);
            TriggerJumping();
        }

        public Vector2 OrientForce(Vector2 force)
        {
            return (int)CurrentOrientation * force.x * Vector2.right + force.y * Vector2.up;
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
                verifyingJump = false;
                return;
            }
            finally
            {
                OnJump -= cts.Cancel;
            }
        }

        protected async void VerifyFreefall()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);
            try
            {
                OnFreefall += cts.Cancel;
                OnJump += cts.Cancel;
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
                verifyingFreefall = false;
                return;
            }
            finally
            {
                OnFreefall -= cts.Cancel;
                OnJump -= cts.Cancel;
                //OnDestroyed -= cts.Cancel;
            }
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

        //ALWAYS RIGHT POINTING
        public Vector2 GroundDirectionVector()
        {
            if (rightGroundHit && leftGroundHit)
            {
                return (rightGroundHit.point - leftGroundHit.point).normalized;
            }
            return transform.right;
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
            //OnDestroyed?.Invoke();

            base.OnDestroy();

            DirectionChanged = null;
            //OnDestroyed = null;
            OnJump = null;
            OnFreefall = null;
            FreefallVerified = null;
        }
    }
}