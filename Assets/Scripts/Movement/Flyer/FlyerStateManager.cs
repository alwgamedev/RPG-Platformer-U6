using RPGPlatformer.Core;
namespace RPGPlatformer.Movement
{
    public class FlyerStateManager : FlyerStateManager<FlyerStateGraph, FlyerStateMachine, Flyer>
    {
        public FlyerStateManager(Flyer flyer, AnimationControl animationControl) : base(flyer, animationControl) { }
    }

    public class FlyerStateManager<T0, T1, T2> : AdvancedMovementStateManager<T0, T1, T2>
        where T0 : FlyerStateGraph
        where T1 : FlyerStateMachine<T0>
        where T2 : Flyer
    {
        public FlyerStateManager(T2 flyer, AnimationControl animationControl) : base(flyer, animationControl) { }

        //public override void Configure()
        //{
        //    base.Configure();
        //}
    }
}