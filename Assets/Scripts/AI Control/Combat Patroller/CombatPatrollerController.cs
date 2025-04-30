using RPGPlatformer.Combat;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    using T0 = IAIMovementController;
    using T1 = AICombatController;
    using T2 = GenericCombatPatroller<IAIMovementController, AICombatController>;
    using T3 = CombatPatrollerStateGraph;
    using T4 = CombatPatrollerStateMachine;
    //[RequireComponent(typeof(CombatPatroller))]
    public class CombatPatrollerController : GenericCombatPatrollerController<T0, T1, T2, T3, T4,
        CombatPatrollerStateManager<T3, T4, T0, T1, T2>>
    //GenericCombatPatrollerController<AIMovementController,
    //AdvancedMover, AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
    //AICombatController, CombatPatroller, CombatPatrollerStateGraph, CombatPatrollerStateMachine,
    //CombatPatrollerStateManager>
    { }
}