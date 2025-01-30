using UnityEngine;
using RPGPlatformer.UI;

namespace RPGPlatformer.Combat
{
    public class PlayerCombatant : Combatant
    {
        public override bool IsPlayer => true;

        protected override void ConfigureReplenishableStats()
        {
            Health.Stat.statBar = GameObject.Find("Player Health Bar").GetComponent<StatBarItem>();
            Stamina.statBar = GameObject.Find("Player Stamina Bar").GetComponent<StatBarItem>();
            Wrath.statBar = GameObject.Find("Player Wrath Bar").GetComponent<StatBarItem>();

            base.ConfigureReplenishableStats();
        }

        public override void OnEquipmentLevelReqFailed()
        {
            GameLog.Log("You do not meet the level requirements to equip that item.");
        }
    }
}