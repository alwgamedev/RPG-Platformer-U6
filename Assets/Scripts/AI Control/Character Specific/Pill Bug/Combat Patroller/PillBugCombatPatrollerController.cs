using RPGPlatformer.Combat;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    using T0 = PillBugMovementController;
    using T1 = AICombatController;
    using T2 = PillBugCombatPatroller;
    using T3 = CombatPatrollerStateGraph;
    using T4 = CombatPatrollerStateMachine<CombatPatrollerStateGraph>;

    public class PillBugCombatPatrollerController : GenericCombatPatrollerController<T0, T1, T2, T3, T4,
        CombatPatrollerStateManager<T3, T4, T0, T1, T2>>
    {
        protected override void OnPursuitExit()
        {
            stateDriver.MovementController.SetCurled(false);

            base.OnPursuitExit();
        }
    }
}