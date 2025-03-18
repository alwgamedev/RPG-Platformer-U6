using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class HybridFlyer : AdvancedMover
    {
        [SerializeField] float inFlightLinearDamping = 30;
        [SerializeField] float flightAcceleration = 100;
        [SerializeField] float flightSpeed = 3;
        [SerializeField] Vector2 takeOffForce = 400 * Vector2.up;

        protected float defaultLinearDamping;
        protected bool awaitingFlightTakeOff;//for time between beginflying and takoff jump
        protected bool verifyingFlight;

        public event Action OnBeginFlying;
        public event Action FlyingVerified;

        public float FlightSpeed => flightSpeed;

        private void Start()
        {
            defaultLinearDamping = myRigidbody.linearDamping;
        }

        public virtual void MoveFlying(Vector2 direction)
        {
            Move(flightAcceleration, MaxSpeed, direction, false);
        }

        public virtual void UpdateState(bool flying, bool jumping, bool freefalling)
        {
            if (rightGroundHit || leftGroundHit)
            {
                if ((flying && !verifyingFlight && !awaitingFlightTakeOff) 
                    || (jumping && !verifyingJump) 
                    || (freefalling && !verifyingFreefall))
                {
                    TriggerLanding();
                }
            }
            else if (!flying && !jumping && !freefalling)
            {
                TriggerFreefall();
            }
        }

        public void SetLinearDamping(bool flying)
        {
            myRigidbody.linearDamping = flying ? inFlightLinearDamping : defaultLinearDamping;
        }

        public virtual void BeginFlying()
        {
            OnBeginFlying?.Invoke();
            awaitingFlightTakeOff = true;
            Trigger(typeof(Flying).Name);
            //idea: this will trigger takeoff animation, and then jump will be triggered in animation
            //take off animation will transition automatically to looped flying animation
        }
        
        //trigger in animation
        public virtual void FlightTakeOffJump()
        {
            awaitingFlightTakeOff = false;
            Jump(takeOffForce);
            VerifyFlight();
        }

        protected async void VerifyFlight()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);
            try
            {
                PrepareFlightVerification(cts);
                verifyingFlight = true;
                await Task.Delay(200, cts.Token);
                verifyingFlight = false;
                if (!leftGroundHit && !rightGroundHit)
                {
                    FlyingVerified?.Invoke();
                }
            }
            catch (TaskCanceledException)
            {
                FlightVerificationCatch();
            }
            finally
            {
                FlightVerificationFinally(cts);
            }
        }

        protected virtual void PrepareFlightVerification(CancellationTokenSource cts)
        {
            OnFreefall += cts.Cancel;
            OnJump += cts.Cancel;
            OnBeginFlying += cts.Cancel;
            //these are in case you somehow land and then jump or freefall during verification period
            //to make sure that you end the verification and don't have a bunch of overlapping verifications 
            //confusing each others' outcomes
            //(very unlikely to happen)
        }

        protected virtual void FlightVerificationCatch()
        {
            verifyingFlight = false;
            return;
        }

        protected virtual void FlightVerificationFinally(CancellationTokenSource cts)
        {
            OnFreefall -= cts.Cancel;
            OnJump -= cts.Cancel;
            OnBeginFlying -= cts.Cancel;
        }

        protected override void PrepareJumpVerification(CancellationTokenSource cts)
        {
            base.PrepareJumpVerification(cts);
            OnBeginFlying += cts.Cancel;
        }

        protected override void JumpVerificationFinally(CancellationTokenSource cts)
        {
            base.JumpVerificationFinally(cts);
            OnBeginFlying -= cts.Cancel;
        }

        protected override void PrepareFreefallVerification(CancellationTokenSource cts)
        {
            base.PrepareFreefallVerification(cts);
            OnBeginFlying += cts.Cancel;
        }

        protected override void FreefallVerificationFinally(CancellationTokenSource cts)
        {
            base.FreefallVerificationFinally(cts);
            OnBeginFlying -= cts.Cancel;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnBeginFlying = null;
            FlyingVerified = null;
        }
    }
}