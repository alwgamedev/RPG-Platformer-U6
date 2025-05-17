using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public interface ICombatPatroller : IAIPatroller
    {
        public ICombatController CombatController { get; }
        public Transform LeftAttackBound { get; set; }
        public Transform RightAttackBound { get; set; }
    }
}