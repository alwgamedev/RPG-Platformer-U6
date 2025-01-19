using System;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    [RequireComponent(typeof(AnimationControl))]
    [RequireComponent(typeof(AdvancedMover))]
    public class AdvancedMovementController : MonoBehaviour, IMovementController
    {
        protected bool wallClinging;
        protected AdvancedMover mover;
        protected AdvancedMovementStateManager movementManager;
        protected Action<float> CurrentMoveAction;
        protected Action OnFixedUpdate;
        protected Action OnUpdate;
        protected Action OnMoveInputChanged;

        protected float moveInput = 0;

        public Rigidbody2D Rigidbody => mover.Rigidbody;
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
            movementManager.StateMachine.stateGraph.freefall.OnExit += OnAirborneExit;

            mover.FreefallVerified += OnAirborneVerified;

            OnUpdate += AnimateMovement;

            if (mover.DetectWalls)
            {
                OnUpdate += SetDownSpeed;
                OnUpdate += HandleAdjacentWallInteraction;
            }
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }


        //BASIC FUNCTIONS

        public void SetRunning(bool val)
        {
            mover.Running = val;
        }

        public virtual void MoveTowards(Vector2 point)
        {
            MoveInput = point.x - transform.position.x;
        }

        public virtual void MoveAwayFrom(Vector2 point)
        {
            if (transform.position.x == point.x)
            {
                MoveInput = 1;
                return;
            }
            MoveInput = transform.position.x - point.x;
        }

        public void FaceTowards(Transform target)
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

        public virtual void SetOrientation(float input, bool updateXScale = true)
        {
            input = Mathf.Sign(input);
            mover.SetOrientation((HorizontalOrientation)input, updateXScale);
        }

        
        //STATE CHANGE HANDLERS

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

        protected virtual void SetDownSpeed()
        {
            movementManager.SetDownSpeed(mover.Rigidbody.linearVelocityY);
        }

        protected virtual void HandleAdjacentWallInteraction()
        {
            if (moveInput != 0 && mover.FacingWall)
            {
                movementManager.AnimateWallScramble(false);
                if (!wallClinging || !movementManager.IsWallClinging())
                //^just being safe by checking that wall cling animation is also playing
                {
                    BeginWallCling();
                }
                else
                {
                    mover.MaintainWallCling();
                }
                return;
            }

            if (wallClinging || movementManager.IsWallClinging())
            {
                EndWallCling();
                return;
            }

            if (!movementManager.StateMachine.HasState(typeof(Jumping))
                && mover.FacingWall)
            {
                movementManager.AnimateWallScramble(true);
            }
            else
            {
                movementManager.AnimateWallScramble(false);
            }
        }

        protected virtual void BeginWallCling()
        {
            wallClinging = true;
            movementManager.AnimateWallCling(true);
            mover.BeginWallCling();
        }

        protected virtual void EndWallCling()
        {
            movementManager.AnimateWallCling(false);
            mover.EndWallCling();
            wallClinging = false;
        }

        protected virtual void OnGroundedEntry()
        {
            CurrentMoveAction = GroundedMoveAction;
        }

        protected virtual void OnJumpingEntry()
        {
            CurrentMoveAction = JumpingMoveAction;
        }

        protected virtual void OnAirborneVerified()
        {
            if (movementManager.StateMachine.HasState(typeof(Freefall)))
            {
                CurrentMoveAction = AirborneMoveAction;
                movementManager.AnimateFreefall();
            }
        }

        protected void OnAirborneExit()
        {
            mover.UpdateXScale();
        }

        protected virtual void GroundedMoveAction(float input)
        {
            SetOrientation(input);
            mover.MoveGrounded();
        }

        protected virtual void JumpingMoveAction(float input)
        {
            SetOrientation(input);
            mover.MoveAirborne(mover.CurrentOrientation);
        }

        protected virtual void AirborneMoveAction(float input)
        {
            SetOrientation(input, false);
            mover.MoveAirborne((HorizontalOrientation)input);
        }


        //DEATH AND DESTROY HANDLERS

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
            CurrentMoveAction = null;
            OnUpdate = null;
            OnFixedUpdate = null;
            OnMoveInputChanged = null;
        }
    }
}