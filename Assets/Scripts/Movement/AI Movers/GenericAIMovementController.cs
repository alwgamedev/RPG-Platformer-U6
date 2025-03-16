using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public enum DropOffHandlingOption
    {
        ignore, stop, tryToJump
    }

    [RequireComponent(typeof(AdvancedMover))]
    public abstract class GenericAIMovementController<T0, T1, T2, T3> : GenericAdvancedMovementController<T0, T1, T2, T3>
        //where T0 : AIMover
        where T0 : AdvancedMover
        where T1 : AdvancedMovementStateGraph
        where T2 : AdvancedMovementStateMachine<T1>
        where T3 : AdvancedMovementStateManager<T1, T2, T0>
    {
        [SerializeField] protected DropOffHandlingOption dropOffHandlingOption = DropOffHandlingOption.stop;
        [SerializeField] protected bool canMoveDuringFreefall;
        [SerializeField] protected float maxPermissibleDropOffHeightFactor = 3;
        [SerializeField] protected float dropOffStopDistance = 0.5f;

        protected bool jumpingEnabled = true;
        protected bool stuckAtLedge;

        public IHealth currentTarget;

        public float MaxPermissibleDropOffHeight { get; protected set; }

        public override Vector2 MoveInput
        {
            get => base.MoveInput;
            set
            {
                if (movementManager.StateMachine.HasState(typeof(Grounded)))
                //ai will never change move input while airborne
                //(except if they hit wall then move input is set to 0 so they don't cling)
                {
                    base.MoveInput = value;
                }
                else if (mover.FacingWall)
                {
                    SoftStop();
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            MaxPermissibleDropOffHeight = maxPermissibleDropOffHeightFactor * mover.Height;
        }

        protected override void InitializeMover()
        {
            base.InitializeMover();

            mover.DirectionChanged += o => { stuckAtLedge = false; };
        }

        protected override void ConfigureWallDetection()
        {
            base.ConfigureWallDetection();
            mover.AwkwardWallMoment += SoftStop;
        }

        public bool DropOffAhead(HorizontalOrientation direction, out float distance)
        {
            if (dropOffHandlingOption == DropOffHandlingOption.ignore)
            {
                distance = Mathf.Infinity;
                return false;
            }
            return mover.DropOffAhead(MaxPermissibleDropOffHeight, direction, out distance);
        }

        protected override void GroundedMoveAction(Vector2 input)
        {
            SetOrientation(input);

            if (stuckAtLedge) return;

            if (input != Vector2.zero && DropOffAhead(CurrentOrientation, out var dist)
                && !HandleDropoffAhead(dist, out stuckAtLedge)) return;

            mover.MoveGrounded(matchRotationToGround);
        }

        protected virtual bool HandleDropoffAhead(float dist, out bool stuckAtLedge)
        {
            stuckAtLedge = false;

            if (dropOffHandlingOption == DropOffHandlingOption.stop && dist < dropOffStopDistance)
            {
                HardStop();
                stuckAtLedge = true;
                return false;
            }
            else if (dropOffHandlingOption == DropOffHandlingOption.tryToJump)
            {
                Debug.Log($"velocity before: {mover.Rigidbody.linearVelocity}");
                mover.MoveGroundedWithoutAcceleration(mover.RunSpeed, false);
                Debug.Log($"velocity after: {mover.Rigidbody.linearVelocity}");
                //note can jump assumes you are moving at maxSpeed
                if (jumpingEnabled && mover.CanJumpGap(out var landingPt))
                {
                    if (currentTarget == null
                        || Vector2.Distance(landingPt, currentTarget.Transform.position) <
                        Vector2.Distance(mover.ColliderCenterBottom, currentTarget.Transform.position))
                    {
                        //mover.MoveGroundedWithoutAcceleration(mover.RunSpeed, false);//get up to maxSpeed
                        mover.Jump();
                        return true;
                    }
                    else if (dist < dropOffStopDistance)
                    {
                        HardStop();
                        return false;
                        //stopped, but not considered stuck at ledge, so it can continue re-evaluating whether
                        //jumping will bring it closer to target
                    }
                }
                else if (dist < dropOffStopDistance)
                {
                    HardStop();
                    stuckAtLedge = true;
                    return false;
                }

                return true;
            }

            return true;
        }

        public void EnableJumping(bool val)
        {
            jumpingEnabled = val;
        }

        protected override void HandleAdjacentWallInteraction(bool airborne)
        {
            if (mover.FacingWall)
            {
                SoftStop();
            }
        }

        protected override void FreefallMoveAction(Vector2 input)
        {
            if (canMoveDuringFreefall)
            {
                base.FreefallMoveAction(input);
            }
        }
    }
}