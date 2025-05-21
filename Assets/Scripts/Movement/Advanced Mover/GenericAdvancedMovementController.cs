using RPGPlatformer.Core;
using System.Runtime.InteropServices;
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
            stateDriver.UpdateState(stateManager.StateMachine.CurrentState.name);
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

        protected override void UpdateMoveOptions(State state)
        {
            base.UpdateMoveOptions(state);
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
            if (Swimming)
            {
                stateDriver.MaxSpeed = stateDriver.Running ? 
                    stateDriver.SwimmingRunSpeed :stateDriver.SwimmingWalkSpeed;
            }
            else
            {
                stateDriver.MaxSpeed = stateDriver.Running ? stateDriver.RunSpeed : stateDriver.WalkSpeed;
            }
        }

        protected virtual void AnimateMovement()
        {
            if (Grounded)
            {
                if (!Moving)
                {
                    stateManager.AnimateMovement(0);
                    return;
                    //animator will transition to idle, but the transition is smooth!
                }
                stateManager.AnimateMovement(SpeedFraction(stateDriver.RunSpeed));
            }
            else if (Swimming)
            {
                if (!Moving)
                {
                    stateManager.AnimateMovement(0);
                    return;
                }

                stateManager.AnimateMovement(Mathf.Abs(HorizontalSpeedFraction(stateDriver.SwimmingWalkSpeed)));
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
            HandleAdjacentWallInteraction();
        }

        protected virtual void HandleAdjacentWallInteraction()
        {
            if (Moving && stateDriver.FacingWall)
            {
                stateManager.AnimateWallScramble(false);
                if (CanBeginWallCling())
                {
                    BeginWallCling();
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

            if (CanAnimateWallScramble())
            {
                stateManager.AnimateWallScramble(true);
            }
            else
            {
                stateManager.AnimateWallScramble(false);
            }
        }

        protected virtual bool CanBeginWallCling()
        {
            return !stateManager.IsWallClinging();
        }

        protected virtual bool CanAnimateWallScramble()
        {
            return !Jumping && stateDriver.FacingWall;
        }

        protected virtual void BeginWallCling()
        {
            stateManager.AnimateWallCling(true);
            stateDriver.BeginWallCling(wallDetectionOptions.WallClingRotationSpeed);
        }

        protected virtual void EndWallCling()
        {
            stateManager.AnimateWallCling(false);
            stateDriver.EndWallCling();
        }
    }
}