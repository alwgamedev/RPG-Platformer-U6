using RPGPlatformer.Combat;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    //[RequireComponent(typeof(CombatPatroller))]
    public class CombatPatrollerController : GenericCombatPatrollerController<AIMovementController,
        AdvancedMover, AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
        AICombatController, CombatPatroller, CombatPatrollerStateGraph, CombatPatrollerStateMachine,
        CombatPatrollerStateManager>
    { }
}