using System.Collections.Generic;
using System.Linq;

namespace RPGPlatformer.Combat
{
    public class CharacterAbilityBarManager
    {
        ICombatController cc;
        bool configured;

        protected AbilityBar currentAbilityBar;
        protected Dictionary<CombatStyle, AbilityBar> GetAbilityBar = new()
        {
            [CombatStyle.Mage] = null,
            [CombatStyle.Melee] = null,
            [CombatStyle.Ranged] = null,
            [CombatStyle.Unarmed] = null
        };

        public AbilityBar CurrentAbilityBar => currentAbilityBar;

        public CharacterAbilityBarManager(ICombatController cc)
        {
            this.cc = cc;
            //RebuildDictionary();
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
                    GetAbilityBar[combatStyle] = data?.CreateAbilityBar(combatStyle, cc);
                    newBar.Configure();
                }
                else
                {
                    var items = data?.CreateAbilityBarItems(combatStyle);
                    GetAbilityBar[combatStyle].MatchItems(items);
                }
            }
        }

        //private void RebuildDictionary()
        //{
        //    GetAbilityBar.Clear();

        //    GetAbilityBar.Add(CombatStyle.Unarmed, null);
        //    GetAbilityBar.Add(CombatStyle.Melee, null);
        //    GetAbilityBar.Add(CombatStyle.Mage, null);
        //    GetAbilityBar.Add(CombatStyle.Ranged, null);
        //}
    }
}