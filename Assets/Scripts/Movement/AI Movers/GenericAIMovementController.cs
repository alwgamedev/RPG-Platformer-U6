using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public enum DropOffHandlingOption
    {
        ignore, stop, tryToJump
    }

    public abstract class GenericAIMovementController<T0, T1, T2, T3> : GenericAdvancedMovementController<T0, T1, T2, T3>
        where T0 : AIMover
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
        //I am doing the drop off check here (rather than at the point where MoveInput is set)
        //so that orientation is accurate (without having to do an unecessary SetOrientation every time we
        //set MoveInput)
        {
            SetOrientation(input);

            if (stuckAtLedge) return;

            if (input != Vector2.zero && DropOffAhead(CurrentOrientation, out var dist))
            {
                if (dropOffHandlingOption == DropOffHandlingOption.stop && dist < dropOffStopDistance)
                {
                    HardStop();
                    stuckAtLedge = true;
                    return;
                }
                else if (dropOffHandlingOption == DropOffHandlingOption.tryToJump)
                {
                    //note can jump assumes you are moving at maxSpeed
                    if (jumpingEnabled && mover.CanJumpGap(out var landingPt))
                    {
                        if (currentTarget == null
                            || Vector2.Distance(landingPt, currentTarget.Transform.position) <
                            Vector2.Distance(mover.ColliderCenterBottom, currentTarget.Transform.position))
                        {
                            mover.MoveGroundedWithoutAcceleration(false);//get up to maxSpeed
                            mover.Jump();
                            return;
                        }
                        else if (dist < dropOffStopDistance)
                        {
                            HardStop();
                            return;
                            //stopped, but not considered stuck at ledge, so it can continue re-evaluating whether
                            //jumping will bring it closer to target
                        }
                    }
                    else if (dist < dropOffStopDistance)
                    {
                        HardStop();
                        stuckAtLedge = true;
                        return;
                    }
                }
            }

            mover.MoveGrounded(matchRotationToGround);
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