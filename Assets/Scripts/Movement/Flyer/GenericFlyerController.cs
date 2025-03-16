namespace RPGPlatformer.Movement
{
    public class GenericFlyerController<T0, T1, T2, T3> : GenericAdvancedMovementController<T0, T1, T2, T3>
        where T0 : Flyer
        where T1 : FlyerStateGraph
        where T2 : FlyerStateMachine<T1>
        where T3 : FlyerStateManager<T1, T2, T0>
    {

    }
}