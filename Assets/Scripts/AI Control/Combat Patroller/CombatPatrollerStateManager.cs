using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class CombatPatrollerStateManager : CombatPatrollerStateManager<CombatPatrollerStateGraph, CombatPatrollerStateMachine, CombatPatroller>
    {
        public CombatPatrollerStateManager(CombatPatrollerStateMachine stateMachine, CombatPatroller patroller) 
            : base(stateMachine, patroller) { }
    }

    public class CombatPatrollerStateManager<T0, T1, T2> : StateManager<T0, T1, T2>
        where T0 : CombatPatrollerStateGraph
        where T1 : CombatPatrollerStateMachine<T0>
        where T2 : CombatPatroller
    {
        public CombatPatrollerStateManager(T1 stateMachine, T2 patroller) : base(stateMachine, patroller) { }
    }
}