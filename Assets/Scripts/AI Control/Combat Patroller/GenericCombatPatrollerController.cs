using UnityEngine;
using RPGPlatformer.Combat;

namespace RPGPlatformer.AIControl
{
    public class GenericCombatPatrollerController<T0, T1, T2, T3> : GenericAIPatrollerController<T0, T1, T2, T3>
        where T0 : CombatPatroller
        where T1 : CombatPatrollerStateGraph
        where T2 : CombatPatrollerStateMachine<T1>
        where T3 : CombatPatrollerStateManager<T1, T2, T0>
    {
        [SerializeField] protected bool playerEnemy = true;

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.suspicion.OnExit += OnSuspicionExit;
            stateManager.StateGraph.pursuit.OnEntry += OnPursuitEntry;
            stateManager.StateGraph.pursuit.OnExit += OnPursuitExit;
            stateManager.StateGraph.attack.OnEntry += OnAttackEntry;
            stateManager.StateGraph.attack.OnExit += OnAttackExit;

            StateBehavior[stateManager.StateGraph.suspicion] = patroller.SuspicionBehavior;
            StateBehavior[stateManager.StateGraph.pursuit] = patroller.PursuitBehavior;
            StateBehavior[stateManager.StateGraph.attack] = patroller.AttackBehavior;
        }

        protected override void InitializeState()
        {
            if (playerEnemy)
            {
                patroller.SetCombatTarget(GameObject.Find("Player").GetComponent<IHealth>());
            }
            else
            {
                patroller.SetCombatTarget(null);
            }
        }

        protected virtual void OnSuspicionExit()
        {
            patroller.ResetSuspicionTimer();
        }

        protected virtual void OnPursuitEntry()
        {
            patroller.MovementController.SetRunning(true);
        }

        protected virtual void OnPursuitExit()
        {
            patroller.MovementController.SetRunning(false);
            patroller.MovementController.SoftStop();
        }

        protected virtual void OnAttackEntry()
        {
            patroller.StartAttacking();
        }

        protected virtual void OnAttackExit()
        {
            patroller.StopAttacking();
        }
    }
}