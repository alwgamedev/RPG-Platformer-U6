using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    [RequireComponent(typeof(TickTimer))]
    [RequireComponent(typeof(AnimationControl))]
    public class CombatController : GenericCombatController<CombatStateManager, CombatStateGraph, CombatStateMachine,
        Combatant, AnimationControl>
    { }
}