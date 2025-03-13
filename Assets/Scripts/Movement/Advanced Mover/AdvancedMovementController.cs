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
        [SerializeField] protected bool matchRotationToGround;

        protected AdvancedMover mover;
        protected AdvancedMovementStateManager movementManager;

        //protected bool disabled;
        protected Action<float> CurrentMoveAction;
        protected Action OnFixedUpdate;
        protected Action TempFixedUpdate;
        protected Action OnUpdate;
        protected Action TempUpdate;

        protected float moveInput = 0;

        public bool Moving => moveInput != 0;
        public Rigidbody2D Rigidbody => mover.Rigidbody;
        public HorizontalOrientation CurrentOrientation => mover.CurrentOrientation;
        public IMover Mover => mover;
        public virtual float MoveInput 
            //looks very silly, but AIMovementControllers will override this,
            //so do need this here
        {
            get => moveInput;
            set
            {
                moveInput = value;
            }
        }

        public bool Running => mover.Running;

        protected virtual void Awake()
        {
            InitializeMover();
            //InitializeMoveActions();

            OnFixedUpdate += HandleMoveInput;
        }

        protected virtual void Start()
        {
            InitializeMovementManager();

            OnUpdate += AnimateMovement;

            mover.FreefallVerified += OnFreefallVerified;

            if (detectWalls)
            {
                ConfigureWallDetection();
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

        protected virtual void InitializeMovementManager()
        {
            movementManager = new(mover, GetComponent<AnimationControl>());
            movementManager.Configure();

            movementManager.StateGraph.grounded.OnEntry += OnGroundedEntry;
            movementManager.StateGraph.jumping.OnEntry += OnJumpingEntry;
            movementManager.StateGraph.freefall.OnEntry += OnFreefallEntry;
            movementManager.StateGraph.freefall.OnExit += OnFreefallExit;
        }

        //protected virtual void InitializeMoveActions()
        //{
        //    GroundedMoveAction = matchRotationToGround ? RotateToGroundMoveAction : StdGroundedMoveAction;
        //}

        protected virtual void InitializeMover()
        {
            mover = GetComponent<AdvancedMover>();
        }

        protected virtual void ConfigureWallDetection()
        {
            OnUpdate += mover.UpdateAdjacentWall;
            OnUpdate += SetDownSpeed;
            OnUpdate += HandleAdjacentWallInteraction;
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

        public virtual void SetOrientation(float input, bool updateDirectionFaced = true)
        {
            mover.SetOrientation((HorizontalOrientation)Mathf.Sign(input), updateDirectionFaced);
        }

        public virtual void HardStop()
        {
            mover.Stop();
            MoveInput = 0;
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
                if (!movementManager.IsWallClinging())
                {
                    BeginWallCling();
                }
                else
                {
                    mover.MaintainWallCling();
                }
                return;
            }

            if (movementManager.IsWallClinging())
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
            mover.UpdateDirectionFaced();
        }

        protected virtual void GroundedMoveAction(float input)
        {
            SetOrientation(input);
            mover.MoveGrounded(matchRotationToGround);
        }

        protected virtual void JumpingMoveAction(float input)
        {
            SetOrientation(input);
            mover.MoveFreefall(mover.CurrentOrientation);
        }

        protected virtual void FreefallMoveAction(float input)
        {
            SetOrientation(input);
            mover.MoveFreefall(mover.CurrentOrientation);
        }


        //DEATH AND DESTROY HANDLERS

        public virtual void OnDeath()
        {
            TempFixedUpdate = OnFixedUpdate;
            TempUpdate = OnUpdate;
            OnFixedUpdate = null;
            OnUpdate = null;
            moveInput = 0;
            movementManager.Freeze();
            mover.OnDeath();
        }

        public virtual void OnRevival()
        {
            movementManager.Unfreeze();
            mover.OnRevival();
            OnFixedUpdate = TempFixedUpdate;
            OnUpdate = TempUpdate;
        }

        protected virtual void OnDestroy()
        {
            CurrentMoveAction = null;
            //GroundedMoveAction = null;
            OnUpdate = null;
            OnFixedUpdate = null;
        }
    }
}