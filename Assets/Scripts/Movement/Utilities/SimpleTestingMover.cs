using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class SimpleTestingMover : MonoBehaviorInputConfigurer, IEntityOrienter
    {
        [SerializeField] float maxSpeed;
        [SerializeField] MovementOptions movementOptions;

        int groundLayer;
        float moveInput;
        Rigidbody2D rb;
        Collider2D coll;

        Vector2 colliderCenterRight => coll.bounds.center
            + coll.bounds.extents.x * transform.right;
        Vector2 colliderCenterLeft => coll.bounds.center
            - coll.bounds.extents.x * transform.right;

        public HorizontalOrientation CurrentOrientation => (HorizontalOrientation)Math.Sign(transform.localScale.x);

        public event Action<HorizontalOrientation> DirectionChanged;

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
        }

        private void Update()
        {
            ComputeMoveInput();
        }

        private void FixedUpdate()
        {
            HandleMoveInput();
        }

        private void ComputeMoveInput()
        {
            moveInput = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0)
                + (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0);

            Orient();
        }

        private void Orient()
        {
            if (moveInput == 0) return;
            else if (moveInput * transform.localScale.x < 0)
            {
                var s = transform.localScale;
                s.x *= -1;
                transform.localScale = s;
                DirectionChanged?.Invoke(CurrentOrientation);
            }
        }

        private void HandleMoveInput()
        {
            rb.Move(transform.localScale.x > 0, false, rb.linearVelocity,
                moveInput * GroundDirectionVector(),
                maxSpeed, movementOptions);
        }

        private Vector2 GroundDirectionVector()
        {
            var rightHit = Physics2D.Raycast(colliderCenterRight, -transform.up, Mathf.Infinity, groundLayer);
            var leftHit = Physics2D.Raycast(colliderCenterLeft, -transform.up, Mathf.Infinity, groundLayer);

            if (rightHit && leftHit)
            {
                return (rightHit.point - leftHit.point).normalized;
            }

            return Mathf.Sign(transform.localScale.x) * transform.right;
        }

        private void OnDestroy()
        {
            DirectionChanged = null;
        }
    }
}