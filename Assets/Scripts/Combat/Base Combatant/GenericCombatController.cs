using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    [RequireComponent(typeof(TickTimer))]
    [RequireComponent(typeof(AnimationControl))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class GenericCombatController<T0, T1, T2, T3, T4> : StateDrivenController<T0, T1, T2, T3>
        where T0 : CombatStateManager<T1, T2, T3, T4>
        where T1 : CombatStateGraph
        where T2 : CombatStateMachine<T1>
        where T3 : Combatant
        where T4 : AnimationControl
    {

    }
}