using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class AIPatrollerStateManager
        : AIPatrollerStateManager<AIPatrollerStateGraph, AIPatrollerStateMachine, IAIMovementController,
            AIPatroller>
    {
        public AIPatrollerStateManager(AIPatrollerStateMachine stateMachine, AIPatroller patroller)
            : base(stateMachine, patroller) { }
    }

    public class AIPatrollerStateManager<T0, T1, T2, T3> : StateManager<T0, T1, T3>
        where T0 : AIPatrollerStateGraph
        where T1 : AIPatrollerStateMachine<T0>
        where T2 : IAIMovementController
        where T3 : GenericAIPatroller<T2>
    {
        public AIPatrollerStateManager(T1 stateMachine, T3 patroller) : base(stateMachine, patroller) { }
    }
}