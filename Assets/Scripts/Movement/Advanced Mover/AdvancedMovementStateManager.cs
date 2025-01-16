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

            GetState(typeof(Airborne).Name).OnEntry += AnimateFreefall;
        }

        public void AnimateMovement(float value)
        {
            animationControl.animator.SetFloat("speed", value, 0.1f, Time.deltaTime);
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

        public void AnimateWallCling(bool val)
        {
            animationControl.animator.SetBool("wallCling", val);
        }
    }
}