using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class GenericHybridFlyerController<T0, T1, T2, T3> : GenericAIMovementController<T0, T1, T2, T3>
        where T0 : HybridFlyer
        where T1 : HybridFlyerStateGraph
        where T2 : HybridFlyerStateMachine<T1>
        where T3 : HybridFlyerStateManager<T1, T2, T0>
    {
        public bool Flying => movementManager.StateMachine.CurrentState == movementManager.StateGraph.flying;

        public override Vector2 MoveInput
        {
            set
            {
                if (Flying)
                {
                    value = value.normalized;
                    //this is the only time where the move action directly uses the move input as its direction
                }

                base.MoveInput = value;
            }
        }

        protected override void Start()
        {
            base.Start();
            mover.FlyingVerified += OnFlyingVerified;
        }

        protected override void ConfigureMovementManager()
        {
            base.ConfigureMovementManager();

            movementManager.StateGraph.flying.OnEntry += OnFlyingEntry;
            movementManager.StateGraph.flying.OnExit += OnFlyingExit;
        }

        // BASIC FUNCTIONS

        public virtual void BeginFlying()
        {
            mover.BeginFlying();
        }

        protected override void UpdateMover()
        {
            mover.UpdateGroundHits();
            mover.UpdateState(Flying, Jumping, Freefalling);
        }

        //STATE CHANGE HANDLERS

        protected override void UpdateMaxSpeed()
        {
            if (Flying)
            {
                mover.MaxSpeed = mover.FlightSpeed;
            }
            else
            {
                base.UpdateMaxSpeed();
            }
        }

        protected override void AnimateMovement()
        {
            if (!Flying)
            {
                base.AnimateMovement();
            }
        }

        protected virtual void OnFlyingVerified()
        {
            if (Flying)
            {
                mover.SetLinearDamping(true);
            }
        }

        protected virtual void OnFlyingEntry()
        {
            movementManager.AnimateMovement(0);
            UpdateMaxSpeed();
            CurrentMoveAction = FlyingMoveAction;
            movementManager.OnFlyingEntry();
        }

        protected virtual void OnFlyingExit()
        {
            UpdateMaxSpeed();
            movementManager.OnFlyingExit();
            mover.SetLinearDamping(false);
        }


        //MOVE ACTIONS

        protected virtual void FlyingMoveAction(Vector2 input)
        {
            SetOrientation(input);
            mover.MoveFlying(input);
        }
    }
}