using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public enum DropOffHandlingOption
    {
        ignore, stop, tryToJump
    }

    public abstract class GenericAIMovementController<T0, T1, T2, T3> 
        : GenericAdvancedMovementController<T0, T1, T2, T3>, IAIMovementController
        where T0 : AdvancedMover
        where T1 : AdvancedMovementStateGraph
        where T2 : AdvancedMovementStateMachine<T1>
        where T3 : AdvancedMovementStateManager<T1, T2, T0>
    {
        [SerializeField] protected DropOffHandlingOption dropOffHandlingOption = DropOffHandlingOption.stop;
        [SerializeField] protected bool canMoveDuringFreefall;
        [SerializeField] protected float maxPermissibleDropOffHeightFactor = 3;
        [SerializeField] protected float dropOffStopDistance = 0.5f;

        protected bool stuckAtLedge;

        public Transform CurrentTarget { get; set; }
        public float MaxPermissibleDropOffHeight { get; protected set; }

        public override Vector2 MoveInput
        {
            get => base.MoveInput;
            set
            {
                if (CanSetMoveInput())
                {
                    base.MoveInput = value;
                }
                else if (stateDriver.FacingWall)
                {
                    SoftStop();
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            MaxPermissibleDropOffHeight = maxPermissibleDropOffHeightFactor * stateDriver.Height;
        }

        protected override void InitializeDriver()
        {
            base.InitializeDriver();

            stateDriver.DirectionChanged += o => { stuckAtLedge = false; };
        }

        //protected override void ConfigureWallDetection()
        //{
        //    base.ConfigureWallDetection();
        //    mover.AwkwardWallMoment += SoftStop;
        //}

        protected virtual bool CanSetMoveInput()
        {
            return Grounded;
        }

        protected virtual bool CanMove()
        {
            if (Freefalling && !canMoveDuringFreefall) return false;
            if (Grounded && stuckAtLedge) return false;
            if (Grounded && DropOffAhead(CurrentOrientation, out var dist)
                && !HandlingDropoffAhead(dist)) return false;

            return true;
        }

        public bool DropOffAhead(HorizontalOrientation direction, out float distance)
        {
            if (dropOffHandlingOption == DropOffHandlingOption.ignore)
            {
                distance = Mathf.Infinity;
                return false;
            }
            return stateDriver.DropOffAhead(MaxPermissibleDropOffHeight, direction, out distance);
        }

        //protected override void HandleMoveInput()
        //{
        //    if (moveInput != Vector2.zero)
        //    {
        //        SetOrientation(moveInput, currentMovementOptions.FlipSprite);

        //        if (CanMove())
        //        {
        //            mover.Move(GetMoveDirection(moveInput), currentMovementOptions);
        //        }
        //    }
        //}

        protected override void Move(Vector2 moveInput)
        {
            if (CanMove())
            {
                base.Move(moveInput);
            }
        }

        protected virtual bool HandlingDropoffAhead(float dist)
        {
            if (dropOffHandlingOption == DropOffHandlingOption.stop && dist < dropOffStopDistance)
            {
                HardStop();
                stuckAtLedge = true;
                return true;
            }
            else if (dropOffHandlingOption == DropOffHandlingOption.tryToJump)
            {
                //CanJumpGap assumes you are moving at runSpeed in direction +/- transform.right 
                if (stateDriver.CanJumpGap(out var landingPt))
                {
                    if (!CurrentTarget
                        || Vector2.Distance(landingPt, CurrentTarget.position) <
                        Vector2.Distance(stateDriver.ColliderCenterBottom, CurrentTarget.position))
                    {
                        stateDriver.MoveWithoutAcceleration((int)CurrentOrientation * transform.right,
                            stateDriver.RunSpeed, currentMovementOptions);//get up to run speed
                        stateDriver.Jump();
                        stuckAtLedge = false;
                        return true;
                    }
                    else if (dist < dropOffStopDistance)
                    {
                        HardStop();
                        stuckAtLedge = false;
                        return true;
                        //stopped, but not considered stuck at ledge, so it can continue re-evaluating whether
                        //jumping will bring it closer to target
                    }
                }
                else if (dist < dropOffStopDistance)
                {
                    HardStop();
                    stuckAtLedge = true;
                    return true;
                }
            }
            stuckAtLedge = false;
            return false;
        }

        protected override void HandleAdjacentWallInteraction(bool airborne)
        {
            if (stateDriver.FacingWall)
            {
                SoftStop();
            }
        }
    }
}