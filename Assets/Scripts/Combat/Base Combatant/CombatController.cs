using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.Combat
{
    [RequireComponent(typeof(TickTimer))]
    [RequireComponent(typeof(AnimationControl))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class CombatController : GenericCombatController<CombatStateManager, CombatStateGraph, CombatStateMachine,
        Combatant, AnimationControl>
    { }
}