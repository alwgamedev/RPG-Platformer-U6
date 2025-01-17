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
        protected Action<float> CurrentMoveAction;
        protected Action OnFixedUpdate;
        protected Action OnUpdate;
        protected Action OnMoveInputChanged;

        private float moveInput = 0;//even child classes have to set it through the property!

        public Rigidbody2D Rigidbody => mover.MyRigidbody;
        public HorizontalOrientation CurrentOrientation => mover.CurrentOrientation;
        public IMover Mover => mover;
        public float MoveInput
        {
            get => moveInput;
            set
            {
                moveInput = value;
                OnMoveInputChanged?.Invoke();
            }
        }

        public bool Running => mover.Running;

        protected virtual void Awake()
        {
            mover = GetComponent<AdvancedMover>();
            movementManager = new(mover, GetComponent<AnimationControl>());
            movementManager.Configure();

            OnFixedUpdate += HandleMoveInput;
        }

        protected virtual void OnEnable()
        {
            movementManager.StateMachine.stateGraph.grounded.OnEntry += OnGroundedEntry;
            movementManager.StateMachine.stateGraph.jumping.OnEntry += OnJumpingEntry;
            movementManager.StateMachine.stateGraph.airborne.OnEntry += OnAirborneEntry;

            OnUpdate += AnimateMovement;

            if (mover.CanWallCling)
            {
                OnMoveInputChanged += HandleWallCling;
                mover.AdjacentWallChanged += HandleWallCling;
                //OnUpdate += UpdateWallAngle;
            }
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
            //movementManager.AnimateMovement(mover.SpeedFraction());
        }

        protected virtual void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        protected virtual void HandleMoveInput()
        {
            if (moveInput != 0)
            {
                CurrentMoveAction?.Invoke(moveInput);
            }
        }

        protected virtual void AnimateMovement()
        {
            movementManager.AnimateMovement(mover.SpeedFraction());
        }

        protected virtual void HandleWallCling()
        {
            if (moveInput != 0 && mover.AdjacentWallSide.HasValue
                && Mathf.Sign(moveInput) == (int)mover.AdjacentWallSide.Value)
            {
                movementManager.AnimateWallCling(true);
                return;
            }
            movementManager.AnimateWallCling(false);
        }

        //protected virtual void UpdateWallAngle()
        //{
        //    movementManager.SetWallAngle(mover.AdjacentWallAngle);
        //}

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
                movementManager.AnimateWallCling(false);
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

        //public void CheckIfClingingToWall()
        //{
        //    if (mover.BeginWallCling(moveInput))
        //    {
        //        movementManager.AnimateWallCling(true);
        //    }
        //    else if (mover.EndWallCling(moveInput))
        //    {
        //        movementManager.AnimateWallCling(false);
        //    }
        //}

        public virtual void OnDeath()
        {
            OnFixedUpdate = null;
            moveInput = 0;
            mover.OnDeath();
        }

        public virtual void OnRevival()
        {
            mover.OnRevival();
            OnFixedUpdate = () =>
            {
                HandleMoveInput();
                movementManager.AnimateMovement(mover.SpeedFraction());
            };
        }

        protected virtual void OnDestroy()
        {
            OnUpdate = null;
            OnFixedUpdate = null;
            OnMoveInputChanged = null;
        }
    }
}