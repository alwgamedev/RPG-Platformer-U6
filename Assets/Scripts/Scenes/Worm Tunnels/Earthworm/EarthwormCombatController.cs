using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class EarthwormCombatController : AICombatController
    {
        [SerializeField] Transform nose;

        public Transform Nose => nose;

        protected override void InitializeAbilityBars()
        {
            abilityBarManager.SetAbilityBar(CombatStyle.Unarmed, EarthwormAbilities.EarthwormAbilityBar(this));
        }
    }
}