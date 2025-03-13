using UnityEngine;
using RPGPlatformer.Combat;

namespace RPGPlatformer.Movement
{
    public enum DropOffHandlingOption
    {
        ignore, stop, tryToJump
    }

    public class AIMovementController : AdvancedMovementController
    {
        [SerializeField] protected DropOffHandlingOption dropOffHandlingOption = DropOffHandlingOption.stop;
        [SerializeField] protected bool canMoveDuringFreefall;
        [SerializeField] protected float maxPermissibleDropOffHeightFactor = 3;
        [SerializeField] protected float dropOffStopDistance = 0.5f;

        protected bool jumpingEnabled = true;
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

        protected override void InitializeMover()
        {
            aiMover = GetComponent<AIMover>();
            mover = aiMover;

            mover.DirectionChanged += o => { stuckAtLedge = false; };
        }

        protected override void ConfigureWallDetection()
        {
            base.ConfigureWallDetection();
            mover.AwkwardWallMoment += () => MoveInput = 0;
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
                    HardStop();
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
                            mover.MoveGrounded(matchRotationToGround);
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

        //public override void SetOrientation(float input, bool updateDirectionFaced = true)
        //{
        //    HorizontalOrientation oldOrientation = mover.CurrentOrientation;

        //    base.SetOrientation(input, updateDirectionFaced);
        //}

        protected override void HandleAdjacentWallInteraction()
        {
            if (mover.FacingWall)
            {
                MoveInput = 0;
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