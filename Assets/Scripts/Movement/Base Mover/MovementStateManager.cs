using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class MovementStateManager : MovementStateManager<MovementStateGraph, MovementStateMachine, Mover>
    {
        public MovementStateManager(Mover mover, AnimationControl animationControl) 
            : base(mover, animationControl) { }
    }

    public class MovementStateManager<T0, T1, T2> : StateManager<T0, T1, T2>
        where T0 : MovementStateGraph
        where T1 : MovementStateMachine<T0>
        where T2 : Mover
    {
        protected AnimationControl animationControl;

        public MovementStateManager(T2 mover, AnimationControl animationControl) 
            : base(null, mover)
        {
            this.animationControl = animationControl;
        }

        public override void Configure()
        {
            base.Configure();

            StateGraph.grounded.OnEntry += AnimateLanding;
            StateGraph.swimming.OnEntry += BeginAnimateSwimming;
            StateGraph.swimming.OnExit += EndAnimateSwimming;

            //animate freefall will get called by movement controller after freefall verification completes
        }

        public void AnimateFreefall()
        {
            animationControl.SetTrigger("freefall");
            animationControl.ResetTrigger("land");
        }
        public void AnimateLanding()
        {
            animationControl.SetTrigger("land");
        }

        public void BeginAnimateSwimming()
        {
            animationControl.SetBool("swimming", true);
        }

        public void EndAnimateSwimming()
        {
            animationControl.SetBool("swimming", false);
        }
    }
}