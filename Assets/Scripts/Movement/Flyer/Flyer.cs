using System;
using System.Threading;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class Flyer : AdvancedMover
    {
        [SerializeField] float flightAcceleration;
        [SerializeField] float flightSpeed;
        [SerializeField] Vector2 takeOffForce;

        protected bool verifyingFlight;

        public event Action FlightTakeoff;

        public float FlightSpeed => flightSpeed;

        public virtual void MoveFlying(Vector2 direction)
        {
            Move(flightAcceleration, MaxSpeed, direction, false);
        }

        public virtual void UpdateState(bool flying, bool jumping, bool freefalling)
        {
            if (rightGroundHit || leftGroundHit)
            {
                if ((flying && !verifyingFlight) || (jumping && !verifyingJump) || (freefalling && !verifyingFreefall))
                {
                    TriggerLanding();
                }
            }
            else if (!flying && !jumping && !freefalling)
            {
                TriggerFreefall();
            }
        }

        protected virtual void TriggerFlying()
        {

        }
        //to do add flight takeoff verification

        protected override void PrepareJumpVerification(CancellationTokenSource cts)
        {
            base.PrepareJumpVerification(cts);
            FlightTakeoff += cts.Cancel;
        }

        protected override void JumpVerificationFinally(CancellationTokenSource cts)
        {
            base.JumpVerificationFinally(cts);
            FlightTakeoff -= cts.Cancel;
        }

        protected override void PrepareFreefallVerification(CancellationTokenSource cts)
        {
            base.PrepareFreefallVerification(cts);
            FlightTakeoff += cts.Cancel;
        }

        protected override void FreefallVerificationFinally(CancellationTokenSource cts)
        {
            base.FreefallVerificationFinally(cts);
            FlightTakeoff -= cts.Cancel;
        }
    }
}