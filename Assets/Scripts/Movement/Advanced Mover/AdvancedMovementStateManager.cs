using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class AdvancedMovementStateManager : AdvancedMovementStateManager<AdvancedMovementStateGraph,
        AdvancedMovementStateMachine, AdvancedMover>
    {
        public AdvancedMovementStateManager(AdvancedMover mover, AnimationControl animationControl)
            : base(mover, animationControl) { }
    }

    public class AdvancedMovementStateManager<T0, T1, T2> : MovementStateManager<T0, T1, T2>
        where T0 : AdvancedMovementStateGraph
        where T1 : AdvancedMovementStateMachine<T0>
        where T2 : AdvancedMover
    {
        public AdvancedMovementStateManager(T2 mover, AnimationControl animationControl)
            : base(mover, animationControl) { }

        public override void Configure()
        {
            base.Configure();

            StateGraph.jumping.OnExit += stateDriver.ResetJumpNum;
            StateGraph.grounded.OnEntry += AnimateLanding;

            StateGraph.jumping.OnEntry += AnimateJumping;
            StateGraph.jumping.OnEntryToSameState += AnimateDoubleJump;
        }

        public void AnimateMovement(float value)
        {
            animationControl.SetFloat("speedFraction", value, 0.1f, Time.deltaTime);
        }

        public void AnimateJumping()
        {
            animationControl.SetTrigger("jump");
            animationControl.ResetTrigger("land");//just in case
        }

        public void AnimateDoubleJump()
        {
            animationControl.SetTrigger("doubleJump");
        }

        public bool IsWallClinging()
        {
            return animationControl.GetBool("wallCling");
        }

        public void AnimateWallCling(bool val)
        {
            animationControl.SetBool("wallCling", val);
        }

        public bool IsWallScrambling()
        {
            return animationControl.GetBool("wallScramble");
        }

        public void AnimateWallScramble(bool val)
        {
            animationControl.SetBool("wallScramble", val);
        }

        public void SetDownSpeed(float yVelocity)
        {
            animationControl.SetFloat("downSpeed", Mathf.Clamp(-yVelocity / 2, 0, 1));
        }
    }
}