using System;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    [RequireComponent(typeof(AnimationControl))]
    [RequireComponent(typeof(AdvancedMover))]
    public class AdvancedMovementController : MonoBehaviour, IMovementController
    {
        [SerializeField] protected bool detectWalls;

        //protected bool wallClinging;
        protected AdvancedMover mover;
        protected AdvancedMovementStateManager movementManager;
        protected Action<float> CurrentMoveAction;
        protected Action OnFixedUpdate;
        protected Action OnUpdate;
        //protected Action OnMoveInputChanged;

        protected float moveInput = 0;

        public Rigidbody2D Rigidbody => mover.Rigidbody;
        public HorizontalOrientation CurrentOrientation => mover.CurrentOrientation;
        public IMover Mover => mover;
        public virtual float MoveInput
        {
            get => moveInput;
            set
            {
                moveInput = value;
                //OnMoveInputChanged?.Invoke();
            }
        }

        public bool Running => mover.Running;

        protected virtual void Awake()
        {
            InitializeMover();
            //mover = GetComponent<AdvancedMover>();
            //InitializeMovementManager();

            OnFixedUpdate += HandleMoveInput;
        }

        protected virtual void Start()
        {
            InitializeMovementManager();

            OnUpdate += AnimateMovement;

            mover.FreefallVerified += OnFreefallVerified;

            if (detectWalls)
            {
                OnUpdate += mover.UpdateAdjacentWall;
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

        //protected virtual void LateUpdate()
        //{
        //    AnimateMovement();
        //}

        protected virtual void InitializeMovementManager()
        {
            movementManager = new(mover, GetComponent<AnimationControl>());
            movementManager.Configure();

            movementManager.StateGraph.grounded.OnEntry += OnGroundedEntry;
            movementManager.StateGraph.jumping.OnEntry += OnJumpingEntry;
            movementManager.StateGraph.freefall.OnEntry += OnFreefallEntry;
            movementManager.StateGraph.freefall.OnExit += OnFreefallExit;
        }

        protected virtual void InitializeMover()
        {
            mover = GetComponent<AdvancedMover>();
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
            if (moveInput != 0 && mover.FacingWall && !movementManager.StateMachine.HasState(typeof(Grounded)))
            {
                movementManager.AnimateWallScramble(false);
                if (/*!wallClinging ||*/ !movementManager.IsWallClinging())
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

            if (/*wallClinging ||*/ movementManager.IsWallClinging())
            {
                EndWallCling();
                return;
            }

            if (!movementManager.StateMachine.HasState(typeof(Jumping))
                && mover.FacingWall && Mathf.Abs(mover.AdjacentWallAngle.eulerAngles.z) < 20)
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
            //wallClinging = true;
            movementManager.AnimateWallCling(true);
            mover.BeginWallCling();
        }

        protected virtual void EndWallCling()
        {
            movementManager.AnimateWallCling(false);
            mover.EndWallCling();
            //wallClinging = false;
        }

        protected virtual void OnGroundedEntry()
        {
            CurrentMoveAction = GroundedMoveAction;
        }

        protected virtual void OnJumpingEntry()
        {
            CurrentMoveAction = JumpingMoveAction;
        }

        protected virtual void OnFreefallEntry() { }

        protected virtual void OnFreefallVerified()
        {
            if (movementManager.StateMachine.HasState(typeof(Freefall)))
            {
                CurrentMoveAction = FreefallMoveAction;
                movementManager.AnimateFreefall();
            }
        }

        protected void OnFreefallExit()
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
            mover.MoveFreefall(mover.CurrentOrientation);
        }

        protected virtual void FreefallMoveAction(float input)
        {
            SetOrientation(input, false);
            mover.MoveFreefall((HorizontalOrientation)input);
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
            //OnMoveInputChanged = null;
        }
    }
}