using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class AIPatrollerController : GenericAIPatrollerController<AdvancedMovementController, AdvancedMover,
        AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
        AIPatroller, AIPatrollerStateGraph, AIPatrollerStateMachine, AIPatrollerStateManager>
    { }
}