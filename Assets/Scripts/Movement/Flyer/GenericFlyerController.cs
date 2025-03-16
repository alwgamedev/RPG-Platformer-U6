using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class GenericFlyerController<T0, T1, T2, T3> : GenericAdvancedMovementController<T0, T1, T2, T3>
        where T0 : Flyer
        where T1 : FlyerStateGraph
        where T2 : FlyerStateMachine<T1>
        where T3 : FlyerStateManager<T1, T2, T0>
    {
        public bool Flying => movementManager.StateMachine.CurrentState == movementManager.StateGraph.flying;

        public override Vector2 MoveInput 
        { 
            get => base.MoveInput; 
            set
            {
                if (Flying)
                {
                    moveInput = value.normalized;//rn this is the only time it needs to be normalized
                    //because other movement actions already use a unit direction vector
                    //(GroundDirectionVector() gets normalized,
                    //while flying uses the move input as the direction vector)
                }
                else
                {
                    base.MoveInput = value;
                }
            }
        }

        protected override void ConfigureMovementManager()
        {
            base.ConfigureMovementManager();

            movementManager.StateGraph.flying.OnEntry += OnFlyingEntry;
            movementManager.StateGraph.flying.OnExit += OnFlyingExit;
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

        protected virtual void OnFlyingEntry()
        {
            UpdateMaxSpeed();
            CurrentMoveAction = FlyingMoveAction;
        }

        protected virtual void OnFlyingExit()
        {
            UpdateMaxSpeed();
        }


        //MOVE ACTIONS

        protected virtual void FlyingMoveAction(Vector2 input)
        {
            SetOrientation(input);
            mover.MoveFlying(input);
        }
    }
}