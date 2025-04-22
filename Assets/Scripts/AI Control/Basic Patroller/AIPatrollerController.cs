using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class AIPatrollerController : GenericAIPatrollerController<IMovementController,
        AIPatroller, AIPatrollerStateGraph, AIPatrollerStateMachine, AIPatrollerStateManager>
    //GenericAIPatrollerController<AdvancedMovementController, AdvancedMover,
    //AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
    //AIPatroller, AIPatrollerStateGraph, AIPatrollerStateMachine, AIPatrollerStateManager>
    { }
}