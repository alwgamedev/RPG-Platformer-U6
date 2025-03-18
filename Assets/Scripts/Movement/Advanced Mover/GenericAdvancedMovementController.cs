using UnityEngine;

namespace RPGPlatformer.Movement
{
    public abstract class GenericAdvancedMovementController<T0, T1, T2, T3> 
        : GenericMovementController<T0, T1, T2, T3>, IMovementController
        where T0 : AdvancedMover
        where T1 : AdvancedMovementStateGraph
        where T2 : AdvancedMovementStateMachine<T1>
        where T3 : AdvancedMovementStateManager<T1, T2, T0>
    {
        [SerializeField] protected bool detectWalls;

        public bool Grounded => movementManager.StateMachine.CurrentState == movementManager.StateGraph.grounded;
        public bool Jumping => movementManager.StateMachine.CurrentState == movementManager.StateGraph.jumping;
        public bool Freefalling => movementManager.StateMachine.CurrentState == movementManager.StateGraph.freefall;

        protected override void Start()
        {
            base.Start();

            UpdateMaxSpeed();
            OnUpdate += AnimateMovement;

            if (detectWalls)
            {
                ConfigureWallDetection();
            }
        }

        protected override void ConfigureMovementManager()
        {
            base.ConfigureMovementManager();

            movementManager.StateGraph.jumping.OnEntry += OnJumpingEntry;
        }

        protected virtual void ConfigureWallDetection()
        {
            OnUpdate += UpdateAndHandleWallInteraction;
        }

        protected override void InitializeMover()
        {
            base.InitializeMover();
        }


        //BASIC FUNCTIONS

        protected override void UpdateMover()
        {
            mover.UpdateGroundHits();
            mover.UpdateState(Jumping, Freefalling);
        }

        public virtual void ToggleRunning()
        {
            SetRunning(!mover.Running);
        }

        public virtual void SetRunning(bool val)
        {
            mover.Running = val;
            UpdateMaxSpeed();
        }


        //STATE CHANGE HANDLERS

        protected virtual void UpdateMaxSpeed()
        {
            mover.MaxSpeed = mover.Running ? mover.RunSpeed : mover.WalkSpeed;
        }

        protected virtual void AnimateMovement()
        {
            movementManager.AnimateMovement(mover.SpeedFraction());
        }

        protected virtual void SetDownSpeed()
        {
            movementManager.SetDownSpeed(mover.Rigidbody.linearVelocityY);
        }

        protected virtual void UpdateAndHandleWallInteraction()
        {
            mover.UpdateAdjacentWall(!Grounded);
            SetDownSpeed();
            HandleAdjacentWallInteraction(!Grounded);
        }

        protected virtual void HandleAdjacentWallInteraction(bool airborne)
        {
            if (moveInput != Vector2.zero && mover.FacingWall)
            {
                movementManager.AnimateWallScramble(false);
                if (!movementManager.IsWallClinging())
                {
                    BeginWallCling(airborne);
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

        protected virtual void BeginWallCling(bool airborne)
        {
            movementManager.AnimateWallCling(true);
            mover.BeginWallCling(airborne);
        }

        protected virtual void EndWallCling()
        {
            movementManager.AnimateWallCling(false);
            mover.EndWallCling();
        }

        protected virtual void OnJumpingEntry()
        {
            CurrentMoveAction = JumpingMoveAction;
        }


        //MOVE ACTIONS

        protected override void GroundedMoveAction(Vector2 input)
        {
            SetOrientation(input);
            mover.MoveGrounded(matchRotationToGround);
        }

        protected virtual void JumpingMoveAction(Vector2 input)
        {
            SetOrientation(input);
            mover.MoveFreefall(/*mover.CurrentOrientation*/);
        }

        protected override void FreefallMoveAction(Vector2 input)
        {
            SetOrientation(input);
            mover.MoveFreefall(/*mover.CurrentOrientation*/);
        }
    }
}