using RPGPlatformer.Combat;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    //[RequireComponent(typeof(CombatPatroller))]
    public class CombatPatrollerController : GenericCombatPatrollerController<IAIMovementController,
        AICombatController, CombatPatroller, CombatPatrollerStateGraph, CombatPatrollerStateMachine,
        CombatPatrollerStateManager>
    //GenericCombatPatrollerController<AIMovementController,
    //AdvancedMover, AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
    //AICombatController, CombatPatroller, CombatPatrollerStateGraph, CombatPatrollerStateMachine,
    //CombatPatrollerStateManager>
    { }
}