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
        [SerializeField] protected float maxPermissibleDropOffHeightFactor = 2.5f;

        //protected bool jumpingGap;
        protected bool gapJumpingEnabled;
        protected bool stuckAtLedge;

        public IHealth currentTarget;

        public float MaxPermissibleDropOffHeight => maxPermissibleDropOffHeightFactor * mover.Height;

        public override float MoveInput 
        { 
            get => base.MoveInput;
            set
            {
                if (!movementManager.StateMachine.HasState(typeof(Jumping)))
                    //don't change move input while jumping
                {
                    base.MoveInput = value;
                }
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void GroundedMoveAction(float input)
            //I am doing the drop off check here (rather than at the point where MoveInput is set)
            //so that orientation is accurate (without having to do an unecessary SetOrientation every time we
            //set MoveInput)
        {
            SetOrientation(input);

            if (stuckAtLedge) return;

            if (input != 0 && dropOffHandlingOption != DropOffHandlingOption.ignore 
                && !movementManager.StateMachine.HasState(typeof(Jumping))
                && mover.DropOffInFront(MaxPermissibleDropOffHeight))
            {
                if (dropOffHandlingOption == DropOffHandlingOption.stop)
                {
                    mover.Stop();
                    stuckAtLedge = true;
                    return;
                }
                else if (dropOffHandlingOption == DropOffHandlingOption.tryToJump)
                {
                    if (CanJumpGap(out var landingPt))
                    {

                        if (currentTarget == null
                            || Vector2.Distance(landingPt, currentTarget.Transform.position) <
                            Vector2.Distance(mover.ColliderCenterBottom, currentTarget.Transform.position))
                        {
                            mover.Jump();
                            //jumpingGap = true;
                        }
                        else
                        {
                            mover.Stop();
                            return;
                            //stopped, but not considered stuck at ledge, so it can continue re-evaluating whether
                            //jumping will bring it closer to target
                        }
                    }
                    else
                    {
                        mover.Stop();
                        stuckAtLedge = true;
                        return;
                    }
                }
            }

            mover.MoveGrounded();
        }

        public void EnableGapJumping(bool val)
        {
            gapJumpingEnabled = val;
        }

        protected virtual bool CanJumpGap(out Vector2 landingPoint)
        {
            landingPoint = default;

            if (!mover.CanJump() || !gapJumpingEnabled)
            {
                return false;
            }

            Trajectory jumpTrajectory =
                MovementTools.ImpulseForceTrajectory(mover, mover.OrientForce(mover.JumpForce()));

            float dt = jumpTrajectory.timeToReturnToLevel / 20;

            for (int i = 10; i <= 30; i++)
            {
                Vector2 hitOrigin = jumpTrajectory.position(i * dt);
                var hit = Physics2D.Raycast(hitOrigin, -Vector2.up, 0.5f * mover.Height,
                        LayerMask.GetMask("Ground"));

                //Debug.DrawLine(hitOrigin, hitOrigin - 0.5f * mover.Height * Vector2.up, Color.blue, 3);

                if (hit && hit.distance > 0)
                {
                    landingPoint = hit.point;

                    //check if landing area is level ground
                    var hit1Origin = hitOrigin + ((int)mover.CurrentOrientation) * mover.Width * Vector2.right;
                    var hit1 = Physics2D.Raycast(hit1Origin, -Vector2.up, 0.5f * mover.Height,
                        LayerMask.GetMask("Ground"));

                    //Debug.DrawLine(hit1Origin, hit1Origin - 0.5f * mover.Height * Vector2.up, Color.red, 3);

                    if (!hit1 || hit1.distance == 0)
                    {
                        continue;
                    }

                    Vector2 ground = hit1.point - hit.point;
                    //Debug.DrawLine(hit.point, hit1.point, Color.yellow, 3);

                    float groundAngle = Mathf.Rad2Deg * Mathf.Atan2(ground.y, ground.x);
                    groundAngle = Mathf.Abs(groundAngle);
                    if (mover.CurrentOrientation == HorizontalOrientation.left)
                    {
                        groundAngle = 180 - groundAngle;
                    }

                    if (groundAngle > 60)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
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

        protected override void OnGroundedEntry()
        {
            base.OnGroundedEntry();
            //jumpingGap = false;
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