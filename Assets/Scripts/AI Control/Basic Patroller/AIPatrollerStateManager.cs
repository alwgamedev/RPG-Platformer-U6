using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class AIPatrollerStateManager : AIPatrollerStateManager<AIPatrollerStateGraph, AIPatrollerStateMachine, AIPatroller>
    {
        public AIPatrollerStateManager(AIPatrollerStateMachine stateMachine, AIPatroller patroller) : base(stateMachine, patroller) { }
    }

    public class AIPatrollerStateManager<T0, T1, T2> : StateManager<T0, T1, T2>
        where T0 : AIPatrollerStateGraph
        where T1 : AIPatrollerStateMachine<T0>
        where T2 : AIPatroller
    {
        public AIPatrollerStateManager(T1 stateMachine, T2 patroller) : base(stateMachine, patroller) { }
    }
}