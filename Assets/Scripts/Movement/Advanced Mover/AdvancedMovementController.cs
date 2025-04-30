namespace RPGPlatformer.Movement
{
    //[RequireComponent(typeof(AnimationControl))]
    //[RequireComponent(typeof(AdvancedMover))]
    public class AdvancedMovementController : GenericAdvancedMovementController<AdvancedMover,
        AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager>
    { }
}