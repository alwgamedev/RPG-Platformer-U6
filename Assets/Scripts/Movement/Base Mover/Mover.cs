using System;
using UnityEngine;
using RPGPlatformer.Core;
using System.Threading.Tasks;

namespace RPGPlatformer.Movement
{
    public enum HorizontalOrientation : int
    {
        right = 1, left = -1
    }

    public class Mover : StateDriver, IMover
    {
        [SerializeField] Vector2 deathForce = 120 * Vector2.right + 120 * Vector2.up;//forces applied to rb upon character death
        [SerializeField] float deathTorque = 10;

        protected Collider2D myCollider;
        protected Rigidbody2D myRigidbody;
        protected float myHeight;
        protected float myWidth;
        protected Vector3 localColliderRightCenter;//offset between transform.position and front center of collider
        protected Vector3 localColliderLeftCenter;
        protected bool jumping;
        protected bool airborne;
        protected bool verifyingJump;
        protected bool verifyingAirborne;
        protected float groundednessTolerance;
        protected RaycastHit2D rightHit;
        protected RaycastHit2D leftHit;

        public Rigidbody2D MyRigidbody => myRigidbody;
        public Vector3 ColliderFront => transform.position + localColliderRightCenter;
        public Vector3 ColliderBack => transform.position + localColliderLeftCenter;


        public HorizontalOrientation CurrentOrientation { get; protected set; }

        public event Action<HorizontalOrientation> UpdatedXScale;

        protected virtual void Awake()
        {
            myCollider = GetComponent<Collider2D>();
            myRigidbody = GetComponent<Rigidbody2D>();

            CurrentOrientation = HorizontalOrientation.right;

            myHeight = myCollider.bounds.max.y - myCollider.bounds.min.y;
            myWidth = myCollider.bounds.max.x - myCollider.bounds.min.x;

            groundednessTolerance = 0.7f * myHeight;

            localColliderRightCenter = myCollider.bounds.center + (myWidth / 4) * Vector3.right - transform.position;
            localColliderLeftCenter = myCollider.bounds.center - (myWidth / 4) * Vector3.right - transform.position;
        }

        private void Update()
        {
            rightHit = Physics2D.Raycast(ColliderFront, -transform.up, groundednessTolerance, 
                LayerMask.GetMask("Ground"));
            leftHit = Physics2D.Raycast(ColliderBack, -transform.up, groundednessTolerance, 
                LayerMask.GetMask("Ground"));
            if(rightHit || leftHit)
            {
                if((jumping && !verifyingJump) || (airborne && !verifyingAirborne))
                {
                    TriggerLanding();
                }
            }
            else if(!jumping && !airborne)
            {
                TriggerAirborne();
            }
        }

        protected virtual void TriggerLanding()
        {
            jumping = false;
            airborne = false;
            Trigger(typeof(Grounded).Name);
        }

        protected virtual void TriggerAirborne()
        {
            airborne = true;
            VerifyAirborne();
            Trigger(typeof(Airborne).Name);
        }

        protected virtual void TriggerJumping()
        {
            jumping = true;
            VerifyJump();
            Trigger(typeof(Jumping).Name);
        }

        public void Move(float acceleration, float maxSpeed, Vector2 direction, bool clampXOnly)
        {
            if (direction == Vector2.zero)
            {
                return;
            }
            float delta = 0;
            Vector2 velocity = myRigidbody.linearVelocity;
            if (clampXOnly)
            {
                delta = ClampedDeltaV(acceleration * Time.deltaTime, maxSpeed, new Vector2(velocity.x, 0), new Vector2(Mathf.Sign(direction.x), 0));
                //option to only clamp horizontal velocity (e.g. when airborne)
            }
            else
            {
                delta = ClampedDeltaV(acceleration * Time.deltaTime, maxSpeed, velocity, direction);
            }
            myRigidbody.linearVelocity += delta * direction;
        }

        private float ClampedDeltaV(float defaultDeltaV, float maxSpeed, Vector2 velocity, Vector2 direction)
        {
            float dot = Vector2.Dot(velocity, direction);
            float speed = velocity.magnitude;
            if (dot <= 0 || speed <= maxSpeed)
            {
                return defaultDeltaV;
            }
            else
            {
                return 0;
            }
        }

        public virtual void Jump(Vector2 force)
        {
            myRigidbody.AddForce(OrientForce(force), ForceMode2D.Impulse);
            TriggerJumping();
        }

        protected async void VerifyJump()
        {
            verifyingJump = true;
            await Task.Delay(100, GlobalGameTools.Instance.TokenSource.Token);
            verifyingJump = false;
        }

        protected async void VerifyAirborne()
        {
            verifyingAirborne = true;
            await Task.Delay(100, GlobalGameTools.Instance.TokenSource.Token);
            verifyingAirborne = false;
        }

        protected Vector2 OrientForce(Vector2 force)
        {
            return (int)CurrentOrientation * force.x * Vector2.right + force.y * Vector2.up;
        }

        public virtual void SetOrientation(HorizontalOrientation orientation)
        {
            if (orientation != CurrentOrientation)
            {
                CurrentOrientation = orientation;
                UpdateXScale();
            }
        }

        public virtual void UpdateXScale()
        {
            transform.localScale = new Vector3((int)CurrentOrientation * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            UpdatedXScale.Invoke(CurrentOrientation);
        }

        public Vector2 GroundDirectionVector()
        {
            if (rightHit && leftHit)
            {
                return (int)CurrentOrientation * (rightHit.point - leftHit.point).normalized;
            }
            return (Vector2)transform.right * (int)CurrentOrientation;
        }

        public void OnDeath()
        {
            myRigidbody.freezeRotation = false;
            myRigidbody.AddForce(-(int)CurrentOrientation * deathForce.x * Vector2.right 
                + deathForce.y * Vector2.up, ForceMode2D.Impulse);
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
            UpdatedXScale = null;
        }
    }
}