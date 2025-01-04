using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface IDamageDealer 
    {
        public Transform Transform { get; }

        public CombatStyles.CombatStyle? CurrentCombatStyle { get; }
    }
}