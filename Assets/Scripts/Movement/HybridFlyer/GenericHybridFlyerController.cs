using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class GenericHybridFlyerController<T0, T1, T2, T3> 
        : GenericAIMovementController<T0, T1, T2, T3>, IHybridFlyerController
        where T0 : HybridFlyer
        where T1 : HybridFlyerStateGraph
        where T2 : HybridFlyerStateMachine<T1>
        where T3 : HybridFlyerStateManager<T1, T2, T0>
    {
        public bool Flying => stateManager.StateMachine.CurrentState == stateManager.StateGraph.flying;

        public event Action OnFlightEntry;
        public event Action OnFlightExit;

        protected override void Start()
        {
            base.Start();

            stateDriver.FlyingVerified += OnFlyingVerified;
        }

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.flying.OnEntry += OnFlyingEntry;
            stateManager.StateGraph.flying.OnExit += OnFlyingExit;
        }


        // BASIC FUNCTIONS

        public override void MoveTowards(Vector2 point)
        {
            if (Flying)
            {
                MoveInput = point - (Vector2)transform.position;
            }
            else
            {
                base.MoveTowards(point);
            }
        }

        public override void MoveAwayFrom(Vector2 point)
        {
            if (Flying)
            {
                MoveInput = new Vector3(transform.position.x - point.x, transform.position.y - point.y, -1);
            }
            else
            {
                base.MoveAwayFrom(point);
            }
        }

        public virtual void BeginFlying()
        {
            if (Flying) return;

            stateDriver.BeginFlying();
        }

        protected override void UpdateMover()
        {
            stateDriver.UpdateGroundHits();
            stateDriver.UpdateState(Flying, Jumping, Freefalling);
        }

        //protected override bool CanSetMoveInput()
        //{
        //    return Grounded || Flying;
        //}

        //STATE CHANGE HANDLERS

        protected override void UpdateMaxSpeed()
        {
            if (Flying)
            {
                stateDriver.MaxSpeed = stateDriver.FlightSpeed;
            }
            else
            {
                base.UpdateMaxSpeed();
            }
        }

        protected override void AnimateMovement()
        {
            if (Flying)
            {

                stateManager.AnimateMovement(Mathf.Max(VerticalSpeedFraction(stateDriver.FlightSpeed), 0));
            }
            else
            {
                base.AnimateMovement();
            }
        }

        protected virtual void OnFlyingEntry()
        {
            stateManager.AnimateMovement(0);
            UpdateMaxSpeed();
            //mover.SetLinearDamping(true);
            stateManager.OnFlyingEntry();
            OnFlightEntry?.Invoke();
        }

        protected virtual void OnFlyingVerified()
        {
            if (Flying)
            {
                stateDriver.SetLinearDamping(true);
            }
        }

        protected virtual void OnFlyingExit()
        {
            UpdateMaxSpeed();
            stateManager.OnFlyingExit();
            stateDriver.SetLinearDamping(false);
            OnFlightExit?.Invoke();
            transform.rotation = Quaternion.identity;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnFlightEntry = null;
            OnFlightExit = null;
        }
    }
}