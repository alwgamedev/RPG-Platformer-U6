using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    //[RequireComponent(typeof(AIMovementController))]
    //[RequireComponent(typeof(PatrolNavigator))]
    public class AIPatroller : GenericAIPatroller<AdvancedMovementController, AdvancedMover,
        AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager>
    { }
}
