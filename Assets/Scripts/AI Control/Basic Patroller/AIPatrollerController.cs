using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class AIPatrollerController : GenericAIPatrollerController<IAIMovementController,
        GenericAIPatroller<IAIMovementController>, AIPatrollerStateGraph, AIPatrollerStateMachine, 
        AIPatrollerStateManager<AIPatrollerStateGraph, AIPatrollerStateMachine, IAIMovementController,
            GenericAIPatroller<IAIMovementController>>>
    //GenericAIPatrollerController<AdvancedMovementController, AdvancedMover,
    //AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
    //AIPatroller, AIPatrollerStateGraph, AIPatrollerStateMachine, AIPatrollerStateManager>
    { }
}