using System.Collections.Generic;

namespace RPGPlatformer.Combat
{
    public class CharacterAbilityBarManager
    {
        ICombatController cc;

        protected AbilityBar currentAbilityBar;

        public Dictionary<CombatStyle, AbilityBar> GetAbilityBar = new();

        public AbilityBar CurrentAbilityBar => currentAbilityBar;

        public CharacterAbilityBarManager(ICombatController cc)
        {
            this.cc = cc;
            RebuildDictionary();
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

        public void Configure(SerializableCharacterAbilityBarData data)
        {
            foreach(var combatStyle in CombatStyles.CoreCombatStyles)
            {
                GetAbilityBar[combatStyle] = data?.CreateAbilityBar(combatStyle, cc);
            }
        }

        private void RebuildDictionary()
        {
            GetAbilityBar.Clear();

            GetAbilityBar.Add(CombatStyle.Unarmed, null);
            GetAbilityBar.Add(CombatStyle.Melee, null);
            GetAbilityBar.Add(CombatStyle.Mage, null);
            GetAbilityBar.Add(CombatStyle.Ranged, null);
        }
    }
}