using UnityEngine;

namespace RPGPlatformer.Movement
{
    public enum DropOffHandlingOption
    {
        ignore, stop, tryToJump
    }

    public abstract class GenericAIMovementController<T0, T1, T2, T3> 
        : GenericAdvancedMovementController<T0, T1, T2, T3>, IAIMovementController
        where T0 : AIMover
        where T1 : AdvancedMovementStateGraph
        where T2 : AdvancedMovementStateMachine<T1>
        where T3 : AdvancedMovementStateManager<T1, T2, T0>
    {
        [SerializeField] protected DropOffHandlingOption dropOffHandlingOption = DropOffHandlingOption.stop;
        [SerializeField] protected bool canMoveDuringFreefall;
        [SerializeField] protected float maxPermissibleDropOffHeightFactor = 3;
        [SerializeField] protected float dropOffStopDistance = 0.5f;
        [SerializeField] protected Transform rightBound;
        [SerializeField] protected Transform leftBound;

        protected bool stuckAtLedge;

        public Transform CurrentTarget { get; set; }
        public Transform LeftMovementBound
        {
            get => leftBound;
            set => leftBound = value;
        }
        public Transform RightMovementBound
        {
            get => rightBound;
            set => rightBound = value;
        }
        public float MaxPermissibleDropOffHeight { get; protected set; }

        //public override Vector3 MoveInput
        //{
        //    get => base.MoveInput;
        //    protectedset
        //    {
        //        if (CanSetMoveInput())
        //        {
        //            base.MoveInput = value;
        //        }
        //        else
        //        {
        //            if (stateDriver.FacingWall)
        //            {
        //                SoftStop();
        //            }
        //        }
        //    }
        //}

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

        //public void SetMoveInput(Vector3 moveInput)
        //{
        //    if (CanSetMoveInput())
        //    {
        //        MoveInput = moveInput;
        //    }
        //    else if (stateDriver.FacingWall)
        //    {
        //        SoftStop();
        //    }
        //}

        //protected virtual bool CanSetMoveInput()
        //{
        //    return Grounded;
        //}

        protected virtual bool CanMove(Vector3 moveInput)
        {
            if (rightBound && moveInput.x > 0 && transform.position.x > rightBound.position.x) return false;
            if (leftBound && moveInput.x < 0 && transform.position.x < leftBound.position.x) return false;
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

        protected override void Move(Vector3 moveInput)
        {
            if (CanMove(moveInput))
            {
                base.Move(moveInput);
            }
        }

        //This only gave you a soft stop if you were NOT grounded and facing wall?
        //(I guess we were dealing with ai clinging to wall when falling,
        //but we can just disable canMoveDuringFreefall).
        //For now let's just leave it out.
        //public override void MoveTowards(Vector2 point)
        //{
        //    if (CanSetMoveInput())
        //    {
        //        base.MoveTowards(point);
        //    }
        //    else if (stateDriver.FacingWall)
        //    {
        //        SoftStop();
        //    }
        //}

        //public override void MoveAwayFrom(Vector2 point)
        //{
        //    if (CanSetMoveInput())
        //    {
        //        base.MoveAwayFrom(point);
        //    }
        //    else if (stateDriver.FacingWall)
        //    {
        //        SoftStop();
        //    }
        //}


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
                        stateDriver.MoveWithoutAcceleration(false, (int)CurrentOrientation * transform.right,
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

        protected override void HandleAdjacentWallInteraction(/*bool airborne*/)
        {
            if (stateDriver.FacingWall)
            {
                SoftStop();
            }
        }
    }
}