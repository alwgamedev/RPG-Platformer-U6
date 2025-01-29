using System;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class AdvancedMovementStateManager : MovementStateManager<MovementStateGraph, MovementStateMachine, AdvancedMover>
    {
        AnimationControl animationControl;

        public AdvancedMovementStateManager(AdvancedMover mover, AnimationControl animationControl) : base(null, mover)
        {
            this.animationControl = animationControl;
        }

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
            animationControl.animator.SetFloat("horSpeedFraction", value, 0.1f, Time.deltaTime);
        }

        public void AnimateJumping()
        {
            animationControl.animator.SetTrigger("jump");
            animationControl.animator.ResetTrigger("land");//just in case
        }

        private void AnimateDoubleJump()
        {
            animationControl.animator.SetTrigger("doubleJump");
        }

        public void AnimateFreefall()
        {
            animationControl.animator.SetTrigger("freefall");
            animationControl.animator.ResetTrigger("land");
        }
        public void AnimateLanding()
        {
            animationControl.animator.SetTrigger("land");
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