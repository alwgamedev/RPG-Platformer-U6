using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class CombatPatrollerStateManager 
        : CombatPatrollerStateManager<CombatPatrollerStateGraph, CombatPatrollerStateMachine, 
            AIMovementController, AdvancedMover, AdvancedMovementStateGraph, AdvancedMovementStateMachine,
            AdvancedMovementStateManager, AICombatController, CombatPatroller>
    {
        public CombatPatrollerStateManager(CombatPatrollerStateMachine stateMachine, CombatPatroller patroller) 
            : base(stateMachine, patroller) { }
    }

    public class CombatPatrollerStateManager<T0, T1, T2, T20, T21, T22, T23, T3, T4> 
        : AIPatrollerStateManager<T0, T1, T2, T20, T21, T22, T23, T4>
        where T0 : CombatPatrollerStateGraph
        where T1 : CombatPatrollerStateMachine<T0>
        where T2 : GenericAIMovementController<T20, T21, T22, T23>
        where T20 : AdvancedMover
        where T21 : AdvancedMovementStateGraph
        where T22 : AdvancedMovementStateMachine<T21>
        where T23 : AdvancedMovementStateManager<T21, T22, T20>
        where T3 : AICombatController
        where T4 : GenericCombatPatroller<T2, T20, T21, T22, T23, T3>
    {
        public CombatPatrollerStateManager(T1 stateMachine, T4 patroller) : base(stateMachine, patroller) { }
    }
}