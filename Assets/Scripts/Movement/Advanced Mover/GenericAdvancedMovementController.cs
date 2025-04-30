using UnityEngine;

namespace RPGPlatformer.Movement
{
    public abstract class GenericAdvancedMovementController<T0, T1, T2, T3> 
        : GenericMovementController<T0, T1, T2, T3>
        where T0 : AdvancedMover
        where T1 : AdvancedMovementStateGraph
        where T2 : AdvancedMovementStateMachine<T1>
        where T3 : AdvancedMovementStateManager<T1, T2, T0>
    {
        [SerializeField] protected WallDetectionOptions wallDetectionOptions;

        public override bool Jumping => stateManager.StateMachine.CurrentState == stateManager.StateGraph.jumping;

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
            stateDriver.AwkwardWallMoment += IgnoreMoveInputNextUpdate;
        }

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.jumping.OnEntry += OnJumpingEntry;
        }

        //BASIC FUNCTIONS

        protected override void UpdateMover()
        {
            stateDriver.UpdateGroundHits();
            stateDriver.UpdateState(Jumping, Freefalling);
        }

        public virtual void ToggleRunning()
        {
            SetRunning(!stateDriver.Running);
        }

        public override void SetRunning(bool val)
        {
            stateDriver.Running = val;
            UpdateMaxSpeed();
        }


        //STATE CHANGE HANDLERS

        protected virtual void OnJumpingEntry()
        {
            if (stateManager.IsWallClinging())
            {
                EndWallCling();
            }
        }

        protected virtual void UpdateMaxSpeed()
        {
            stateDriver.MaxSpeed = stateDriver.Running ? stateDriver.RunSpeed : stateDriver.WalkSpeed;
        }

        protected virtual void AnimateMovement()
        {
            if (Grounded)
            {
                if ((Vector2)MoveInput == Vector2.zero)
                {
                    stateManager.AnimateMovement(0);
                    return;
                    //animator will transition to idle, but the transition is smooth!
                }
                stateManager.AnimateMovement(SpeedFraction(stateDriver.RunSpeed));
            }
        }

        protected virtual void SetDownSpeed()
        {
            stateManager.SetDownSpeed(stateDriver.Rigidbody.linearVelocityY);
        }

        protected virtual void UpdateAndHandleWallInteraction()
        {
            stateDriver.UpdateAdjacentWall(Grounded, wallDetectionOptions.NumWallCastsPerThird, 
                wallDetectionOptions.WallCastDistanceFactor);
            SetDownSpeed();
            HandleAdjacentWallInteraction(/*!Grounded*/);
        }

        protected virtual void HandleAdjacentWallInteraction(/*bool airborne*/)
        {
            if ((Vector2)moveInput != Vector2.zero && stateDriver.FacingWall)
            {
                stateManager.AnimateWallScramble(false);
                if (!stateManager.IsWallClinging())
                {
                    BeginWallCling(/*airborne*/);
                }
                else
                {
                    stateDriver.MaintainWallCling(wallDetectionOptions.WallClingRotationSpeed);
                }
                return;
            }

            if (stateManager.IsWallClinging())
            {
                EndWallCling();
                return;
            }

            if (!stateManager.StateMachine.HasState(typeof(Jumping))
                && stateDriver.FacingWall)
            {
                stateManager.AnimateWallScramble(true);
            }
            else
            {
                stateManager.AnimateWallScramble(false);
            }
        }

        protected virtual void BeginWallCling(/*bool airborne*/)
        {
            stateManager.AnimateWallCling(true);
            stateDriver.BeginWallCling(/*airborne,*/ wallDetectionOptions.WallClingRotationSpeed);
        }

        protected virtual void EndWallCling()
        {
            stateManager.AnimateWallCling(false);
            stateDriver.EndWallCling();
        }
    }
}