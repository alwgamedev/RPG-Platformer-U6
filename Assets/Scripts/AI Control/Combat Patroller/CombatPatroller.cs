using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AINavigator))]
    public class CombatPatroller : GenericCombatPatroller<IAIMovementController, AICombatController>
        //GenericCombatPatroller<AIMovementController, 
        //AdvancedMover, AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
        //AICombatController>
    {
        //[SerializeField] bool playerEnemy = true;

        //public override void InitializeState()
        //{
        //    if (playerEnemy)
        //    {
        //        SetCombatTarget(GlobalGameTools.Instance.Player.Combatant.Health);
        //    }
        //    else
        //    {
        //        SetCombatTarget(null);
        //    }
        //}
    }
}