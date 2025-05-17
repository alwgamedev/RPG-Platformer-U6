using RPGPlatformer.Combat;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class GenericCombatPatrollerController<T0, T1, T2, T3, T4, T5> 
        : GenericAIPatrollerController<T0, T2, T3, T4, T5>, ICombatPatrollerController
        where T0 : IAIMovementController
        where T1 : AICombatController
        where T2 : GenericCombatPatroller<T0, T1>
        where T3 : CombatPatrollerStateGraph
        where T4 : CombatPatrollerStateMachine<T3>
        where T5 : CombatPatrollerStateManager<T3, T4, T0, T1, T2>
    {
        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.suspicion.OnExit += OnSuspicionExit;
            stateManager.StateGraph.pursuit.OnEntry += OnPursuitEntry;
            stateManager.StateGraph.pursuit.OnExit += OnPursuitExit;
            stateManager.StateGraph.attack.OnEntry += OnAttackEntry;
            stateManager.StateGraph.attack.OnExit += OnAttackExit;
        }

        protected override void BuildStateBehaviorDict()
        {
            base.BuildStateBehaviorDict();

            StateBehavior[stateManager.StateGraph.suspicion] = stateDriver.SuspicionBehavior;
            StateBehavior[stateManager.StateGraph.pursuit] = stateDriver.PursuitBehavior;
            StateBehavior[stateManager.StateGraph.attack] = stateDriver.AttackBehavior;
        }

        protected virtual void OnSuspicionExit()
        {
            stateDriver.ResetSuspicionTimer();
        }

        protected virtual void OnPursuitEntry()
        {
            stateDriver.MovementController.SetRunning(true);
        }

        protected virtual void OnPursuitExit()
        {
            stateDriver.MovementController.SetRunning(false);
            stateDriver.MovementController.SoftStop();
        }

        protected virtual void OnAttackEntry()
        {
            stateDriver.StartAttacking();
        }

        protected virtual void OnAttackExit()
        {
            stateDriver.StopAttacking();
        }
    }
}