using RPGPlatformer.Core;
namespace RPGPlatformer.Movement
{
    public class HybridFlyerStateManager 
        : HybridFlyerStateManager<HybridFlyerStateGraph, HybridFlyerStateMachine, HybridFlyer>
    {
        public HybridFlyerStateManager(HybridFlyer flyer, AnimationControl animationControl) 
            : base(flyer, animationControl) { }
    }

    public class HybridFlyerStateManager<T0, T1, T2> : AdvancedMovementStateManager<T0, T1, T2>
        where T0 : HybridFlyerStateGraph
        where T1 : HybridFlyerStateMachine<T0>
        where T2 : HybridFlyer
    {
        public HybridFlyerStateManager(T2 flyer, AnimationControl animationControl) : base(flyer, animationControl) { }

        public void OnFlyingEntry()
        {
            animationControl.animator.SetTrigger("flightTakeOff");
            animationControl.animator.ResetTrigger("land");
        }

        public void OnFlyingExit()
        {
            animationControl.animator.SetTrigger("land");
        }
    }
}