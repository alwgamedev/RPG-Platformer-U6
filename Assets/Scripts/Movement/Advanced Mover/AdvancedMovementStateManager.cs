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

            GetState(typeof(Grounded).Name).OnEntry += driver.ResetJumpNum;
            GetState(typeof(Grounded).Name).OnEntry += AnimateLanding;

            GetState(typeof(Jumping).Name).OnEntry += AnimateJumping;
            GetState(typeof(Jumping).Name).OnEntryToSameState += AnimateDoubleJump;
        }

        public void AnimateMovement(float value)
        {
            animationControl.animator.SetFloat("speedFraction", value, 0.1f, Time.deltaTime);
            //animationControl.animator.SetFloat("moveMotionTime", Time.time);
        }

        public void AnimateJumping()
        {
            animationControl.animator.SetTrigger("jump");
            animationControl.animator.ResetTrigger("land");//just in case
        }

        public void AnimateDoubleJump()
        {
            animationControl.animator.SetTrigger("doubleJump");
        }

        public bool IsWallClinging()
        {
            return animationControl.animator.GetBool("wallCling");
        }

        public void AnimateWallCling(bool val)
        {
            animationControl.animator.SetBool("wallCling", val);
        }

        public bool IsWallScrambling()
        {
            return animationControl.animator.GetBool("wallScramble");
        }

        public void AnimateWallScramble(bool val)
        {
            animationControl.animator.SetBool("wallScramble", val);
        }

        public void SetDownSpeed(float yVelocity)
        {
            animationControl.animator.SetFloat("downSpeed", Mathf.Clamp(-yVelocity / 2, 0, 1));
        }
    }
}