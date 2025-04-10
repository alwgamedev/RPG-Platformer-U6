using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class CombatantAbilityBarManager
    {
        ICombatController cc;

        protected AbilityBar currentAbilityBar;
        protected Dictionary<CombatStyle, AbilityBar> GetAbilityBar = new()
        {
            [CombatStyle.Mage] = null,
            [CombatStyle.Melee] = null,
            [CombatStyle.Ranged] = null,
            [CombatStyle.Unarmed] = null
        };

        public AbilityBar CurrentAbilityBar => currentAbilityBar;

        public CombatantAbilityBarManager(ICombatController cc)
        {
            this.cc = cc;
        }

        public void EquipAbilityBar(CombatStyle? combatStyle)
        {
            if (combatStyle.HasValue)
            {
                currentAbilityBar = GetAbilityBar[combatStyle.Value];
            }
            else
            {
                currentAbilityBar = null;
            }
        }

        public void UpdateAbilityBars(SerializableCharacterAbilityBarData data)
        {
            foreach(var combatStyle in CombatStyles.CoreCombatStyles)
            {
                if (!GetAbilityBar.TryGetValue(combatStyle, out var bar) || bar == null || !bar.Configured)
                {
                    var newBar = data?.CreateAbilityBar(combatStyle, cc);
                    GetAbilityBar[combatStyle] = newBar;
                    newBar?.Configure();
                }
                else
                {
                    var items = data?.CreateAbilityBarItems(combatStyle);
                    GetAbilityBar[combatStyle].MatchItems(items);
                }
            }
        }
    }
}