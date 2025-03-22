using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    //[RequireComponent(typeof(AIMovementController))]
    [RequireComponent(typeof(AINavigator))]
    //[RequireComponent(typeof(AICombatController))]
    public class CombatPatroller : GenericCombatPatroller<AIMovementController, 
        AdvancedMover, AdvancedMovementStateGraph, AdvancedMovementStateMachine, AdvancedMovementStateManager,
        AICombatController>
    { }
}