using UnityEngine;
using RPGPlatformer.UI;

namespace RPGPlatformer.Combat
{
    public class PlayerCombatant : Combatant
    {
        public override bool IsPlayer => true;

        protected override void ConfigureReplenishableStats()
        {
            var healthBar = GameObject.Find("Player Health Bar");
            var staminaBar = GameObject.Find("Player Stamina Bar");
            var wrathBar = GameObject.Find("Player Wrath Bar");

            if (healthBar)
            {
                Health.Stat.statBar = healthBar.GetComponent<StatBarItem>();
            }
            if (staminaBar)
            {
                Stamina.statBar = staminaBar.GetComponent<StatBarItem>();
            }
            if (wrathBar)
            {
                Wrath.statBar = wrathBar.GetComponent<StatBarItem>();
            }

            base.ConfigureReplenishableStats();
        }

        public override void OnEquipmentLevelReqFailed()
        {
            GameLog.Log("You do not meet the level requirements to equip that item.");
        }
    }
}