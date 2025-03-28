using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface IDamageDealer 
    {
        public Transform transform { get; }

        public CombatStyle CurrentCombatStyle { get; }
    }
}