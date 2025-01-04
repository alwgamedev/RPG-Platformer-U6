using System;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    [RequireComponent(typeof(AnimationControl))]
    [RequireComponent(typeof(AdvancedMover))]
    public class AdvancedMovementController : MonoBehaviour, IMovementController
    {
        protected AdvancedMover mover;
        protected AdvancedMovementStateManager movementManager;

        public float moveInput = 0;

        protected Action<float> CurrentMoveAction;
        protected Action OnUpdate;

        public bool SystemActive { get; set; }
        public Rigidbody2D Rigidbody => mover.MyRigidbody;
        public HorizontalOrientation CurrentOrientation => mover.CurrentOrientation;
        public IMover Mover => mover;
        public bool Running => mover.Running;

        protected virtual void Awake()
        {
            mover = GetComponent<AdvancedMover>();
            movementManager = new(mover, GetComponent<AnimationControl>());
            movementManager.Configure();
        }

        protected virtual void OnEnable()
        {
            movementManager.StateMachine.stateGraph.grounded.OnEntry += OnGroundedEntry;
            movementManager.StateMachine.stateGraph.jumping.OnEntry += OnJumpingEntry;
            movementManager.StateMachine.stateGraph.airborne.OnEntry += OnAirborneEntry;

            OnUpdate = HandleMoveInput;
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void HandleMoveInput()
        {
            if (moveInput != 0)
            {
                CurrentMoveAction?.Invoke(moveInput);
            }
            movementManager.AnimateMovement(mover.SpeedFraction());
        }

        public void SetRunning(bool val)
        {
            mover.Running = val;
        }

        public virtual void OnGroundedEntry()
        {
            CurrentMoveAction = (input) =>
            {
                SetOrientation(input);
                mover.MoveGrounded();
            };
        }

        public virtual void OnJumpingEntry()
        {
            CurrentMoveAction = (input) =>
            {
                SetOrientation(input);
                mover.MoveAirborne(mover.CurrentOrientation);
            };
        }

        public virtual void OnAirborneEntry()
        {
            CurrentMoveAction = (input) =>
            {
                mover.MoveAirborne((HorizontalOrientation)input);
            };
        }

        public virtual void SetOrientation(float input)
        {
            input = Mathf.Sign(input);
            mover.SetOrientation((HorizontalOrientation)input);
        }

        public void FaceTarget(Transform target)
        {
            if (target)
            {
                FaceTarget(target.position);
            }
        }

        public void FaceTarget(Vector2 target)
        {
            SetOrientation(Mathf.Sign(target.x - transform.position.x));
        }

        public virtual void OnDeath()
        {
            OnUpdate = null;
            moveInput = 0;
            mover.OnDeath();
        }

        public virtual void OnRevival()
        {
            mover.OnRevival();
            OnUpdate = () =>
            {
                HandleMoveInput();
                movementManager.AnimateMovement(mover.SpeedFraction());
            };
        }
    }
}