using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class PillBugMovementStateManager 
        : StateManager<PillBugMovementStateGraph, PillBugMovementStateMachine, PillBugMover>
    {
        AnimationControl animationControl;

        public PillBugMovementStateManager(PillBugMover mover,  AnimationControl animationControl) 
            : base(null, mover)
        {
            this.animationControl = animationControl;
        }

        public override void Configure()
        {
            base.Configure();

            StateMachine.stateGraph.curled.OnEntry += AnimateCurl;
            StateMachine.stateGraph.uncurled.OnEntry += AnimateUncurl;
        }

        public void AnimateMovement(float speedFraction)
        {
            animationControl.SetFloat("speedFraction", speedFraction, 0.1f, Time.deltaTime);
        }

        public void AnimateCurl()
        {
            animationControl.SetTrigger("curlUp");
            animationControl.ResetTrigger("unCurl");
        }

        public void AnimateUncurl()
        {
            animationControl.SetTrigger("unCurl");
            animationControl.ResetTrigger("curlUp");
        }
    }
}