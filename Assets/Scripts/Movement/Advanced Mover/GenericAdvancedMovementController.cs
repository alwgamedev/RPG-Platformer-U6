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
        [SerializeField] protected WallDetectionOptions wallDetectionOptions;

        public override bool Jumping => movementManager.StateMachine.CurrentState == movementManager.StateGraph.jumping;

        protected override void Start()
        {
            base.Start();

            UpdateMaxSpeed();
        }
        protected override void InitializeUpdate()
        {
            base.InitializeUpdate();

            if (wallDetectionOptions.DetectWalls)
            {
                ConfigureWallDetection();
            }
        }

        protected override void InitializeFixedUpdate()
        {
            OnFixedUpdate += AnimateMovement;
            base.InitializeFixedUpdate();
        }

        protected virtual void ConfigureWallDetection()
        {
            OnUpdate += UpdateAndHandleWallInteraction;
            mover.AwkwardWallMoment += IgnoreMoveInputNextUpdate;
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
            if (Grounded)
            {
                if (MoveInput == Vector2.zero)
                {
                    movementManager.AnimateMovement(0);
                    return;
                    //animator will transition to idle, but the transition is smooth!
                }
                movementManager.AnimateMovement(SpeedFraction(mover.RunSpeed));
            }
        }

        protected virtual void SetDownSpeed()
        {
            movementManager.SetDownSpeed(mover.Rigidbody.linearVelocityY);
        }

        protected virtual void UpdateAndHandleWallInteraction()
        {
            mover.UpdateAdjacentWall(Grounded, wallDetectionOptions.NumWallCastsPerThird, 
                wallDetectionOptions.WallCastDistanceFactor);
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
                    mover.MaintainWallCling(wallDetectionOptions.WallClingRotationSpeed);
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
    }
}