using UnityEngine;
using RPGPlatformer.Combat;

namespace RPGPlatformer.Movement
{
    public enum DropOffHandlingOption
    {
        ignore, stop, tryToJump
    }

    //[RequireComponent(typeof(AIMover))]
    public class AIMovementController : AdvancedMovementController
    {
        [SerializeField] protected DropOffHandlingOption dropOffHandlingOption = DropOffHandlingOption.stop;
        [SerializeField] protected bool canMoveDuringFreefall;
        [SerializeField] protected float maxPermissibleDropOffHeightFactor = 3;
        [SerializeField] protected float dropOffStopDistance = 0.5f;

        protected bool jumpingEnabled;
        protected bool stuckAtLedge;
        protected AIMover aiMover;

        public IHealth currentTarget;

        public float MaxPermissibleDropOffHeight => maxPermissibleDropOffHeightFactor * mover.Height;

        public override float MoveInput 
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
                    base.MoveInput = 0;
                }
            }
        }

        //protected override void Awake()
        //{
        //    base.Awake();
        //}

        //protected override void Start()
        //{
        //    base.Start();
        //}

        protected override void InitializeMover()
        {
            aiMover = GetComponent<AIMover>();
            mover = aiMover;
        }

        protected override void GroundedMoveAction(float input)
            //I am doing the drop off check here (rather than at the point where MoveInput is set)
            //so that orientation is accurate (without having to do an unecessary SetOrientation every time we
            //set MoveInput)
        {
            SetOrientation(input);

            if (stuckAtLedge) return;

            if (input != 0 && dropOffHandlingOption != DropOffHandlingOption.ignore 
                && movementManager.StateMachine.HasState(typeof(Grounded))
                && aiMover.DropOffInFront(MaxPermissibleDropOffHeight, out var dist))
            {
                if (dropOffHandlingOption == DropOffHandlingOption.stop && dist < dropOffStopDistance)
                {
                    mover.Stop();
                    stuckAtLedge = true;
                    return;
                }
                else if (dropOffHandlingOption == DropOffHandlingOption.tryToJump)
                {
                    if (jumpingEnabled && aiMover.CanJumpGap(out var landingPt))
                    {

                        if (currentTarget == null
                            || Vector2.Distance(landingPt, currentTarget.Transform.position) <
                            Vector2.Distance(mover.ColliderCenterBottom, currentTarget.Transform.position))
                        {
                            mover.MoveGrounded();//before jump to get speed up
                            mover.Jump();
                            return;
                        }
                        else if (dist < dropOffStopDistance)
                        {
                            mover.Stop();
                            return;
                            //stopped, but not considered stuck at ledge, so it can continue re-evaluating whether
                            //jumping will bring it closer to target
                        }
                    }
                    else if (dist < dropOffStopDistance)
                    {
                        mover.Stop();
                        stuckAtLedge = true;
                        return;
                    }
                }
            }

            mover.MoveGrounded();
        }

        //public void SetDetectWalls(bool val)
        //{
        //    if (val && !detectWalls/*!mover.DetectWalls*/)
        //    {
        //        //aiMover.SetDetectWalls(true);
        //        detectWalls = true;
        //        OnUpdate += HandleAdjacentWallInteraction;
        //    }
        //    else if (!val && detectWalls/*mover.DetectWalls*/)
        //    {
        //        //aiMover.SetDetectWalls(false);
        //        detectWalls = false;
        //        OnUpdate -= HandleAdjacentWallInteraction;
        //    }
        //}

        public void EnableJumping(bool val)
        {
            jumpingEnabled = val;
        }

        public override void SetOrientation(float input, bool updateXScale = true)
        {
            HorizontalOrientation oldOrientation = mover.CurrentOrientation;
            base.SetOrientation(input, updateXScale);

            if (mover.CurrentOrientation != oldOrientation)
            {
                stuckAtLedge = false;
            }
        }

        //protected override void OnGroundedEntry()
        //{
        //    //SetDetectWalls(false);

        //    base.OnGroundedEntry();
        //}

        //protected override void OnFreefallEntry()
        //{
        //    //SetDetectWalls(true);

        //    //if (dropOffHandlingOption == DropOffHandlingOption.tryToJump)
        //    //{
        //    //    if (mover.CanJump())
        //    //    {
        //    //        mover.Stop();
        //    //        SetOrientation(-(int)CurrentOrientation);
        //    //        mover.Jump(mover.OrientForce(aiMover.EmergencyJumpForce()));
        //    //    }
        //    //}
        //}

        //protected override void OnJumpingEntry()
        //{
        //    //SetDetectWalls(true);

        //    base.OnJumpingEntry();
        //}

        protected override void HandleAdjacentWallInteraction()
        {
            if (mover.FacingWall)
            {
                moveInput = 0;
            }
        }

        protected override void FreefallMoveAction(float input)
        {
            if (!canMoveDuringFreefall) return;
            //so that if ai do fall off a ledge, they don't continue trying to move forward and grip against
            //a wall

            base.FreefallMoveAction(input);
        }
    }
}