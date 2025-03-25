using System;
using UnityEditor;
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

        public event Action OnFlightEntry;
        public event Action OnFlightExit;

        protected override void ConfigureMovementManager()
        {
            base.ConfigureMovementManager();

            movementManager.StateGraph.flying.OnEntry += OnFlyingEntry;
            movementManager.StateGraph.flying.OnExit += OnFlyingExit;
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

        public virtual void BeginFlying()
        {
            mover.BeginFlying();
        }

        protected override void UpdateMover()
        {
            mover.UpdateGroundHits();
            mover.UpdateState(Flying, Jumping, Freefalling);
        }

        protected override bool CanSetMoveInput()
        {
            return Grounded || Flying;
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
            if (Flying)
            {
                movementManager.AnimateMovement(Mathf.Max(VerticalSpeedFraction(mover.FlightSpeed), 0));
            }
            else
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
            mover.SetLinearDamping(true);
            movementManager.OnFlyingEntry();
            OnFlightEntry?.Invoke();
        }

        protected virtual void OnFlyingExit()
        {
            UpdateMaxSpeed();
            movementManager.OnFlyingExit();
            mover.SetLinearDamping(false);
            OnFlightExit?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnFlightEntry = null;
            OnFlightExit = null;
        }
    }
}