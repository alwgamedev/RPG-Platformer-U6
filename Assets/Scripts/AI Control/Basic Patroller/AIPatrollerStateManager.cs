using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class AIPatrollerStateManager
        : AIPatrollerStateManager<AIPatrollerStateGraph, AIPatrollerStateMachine, AdvancedMovementController, 
            AdvancedMover, AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
            AIPatroller>
    {
        public AIPatrollerStateManager(AIPatrollerStateMachine stateMachine, AIPatroller patroller)
            : base(stateMachine, patroller) { }
    }

    public class AIPatrollerStateManager<T0, T1, T2, T20, T21, T22, T23, T3> : StateManager<T0, T1, T3>
        where T0 : AIPatrollerStateGraph
        where T1 : AIPatrollerStateMachine<T0>
        where T2 : GenericAdvancedMovementController<T20, T21, T22, T23>
        where T20 : AdvancedMover
        where T21 : AdvancedMovementStateGraph
        where T22 : AdvancedMovementStateMachine<T21>
        where T23 : AdvancedMovementStateManager<T21, T22, T20>
        where T3 : GenericAIPatroller<T2, T20, T21, T22, T23>
    {
        public AIPatrollerStateManager(T1 stateMachine, T3 patroller) : base(stateMachine, patroller) { }
    }
}