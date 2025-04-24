namespace RPGPlatformer.Movement
{
    using T0 = AIMover;
    using T1 = AdvancedMovementStateGraph;
    using T2 = AdvancedMovementStateMachine;

    public class AIMovementController : GenericAIMovementController<T0, T1, T2, 
        AdvancedMovementStateManager<T1, T2, T0>>
    { }
}