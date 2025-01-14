using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static SerializableAbilityBarItem;

    [Serializable]
    public class SerializableCharacterAbilityBarData
    {
        [SerializeField] List<SerializableUnarmedAbilityBarItem> unarmedAbilityBarItems = new();
        [SerializeField] List<SerializableMageAbilityBarItem> mageAbilityBarItems = new();
        [SerializeField] List<SerializableMeleeAbilityBarItem> meleeAbilityBarItems = new();
        [SerializeField] List<SerializableRangedAbilityBarItem> rangedAbilityBarItems = new();

        public List<SerializableUnarmedAbilityBarItem> UnarmedAbilityBarItems
        {
            get => unarmedAbilityBarItems;
            set => unarmedAbilityBarItems = value;
        }

        public List<SerializableMageAbilityBarItem> MageAbilityBarItems
        {
            get => mageAbilityBarItems;
            set => mageAbilityBarItems = value;
        }

        public List<SerializableMeleeAbilityBarItem> MeleeAbilityBarItems
        {
            get => meleeAbilityBarItems;
            set => meleeAbilityBarItems = value;
        }

        public List<SerializableRangedAbilityBarItem> RangedAbilityBarItems
        {
            get => rangedAbilityBarItems;
            set => rangedAbilityBarItems = value;
        }

        public static SerializableCharacterAbilityBarData DefaultAbilityBarData()
        {
            SerializableCharacterAbilityBarData data = new()
            {
                unarmedAbilityBarItems = new(UnarmedAbilities.DefaultAbilityBarItems()
                    .Select(x => (SerializableUnarmedAbilityBarItem)CreateSerializableVersion(x))),
                mageAbilityBarItems = new(MageAbilities.DefaultAbilityBarItems()
                    .Select(x => (SerializableMageAbilityBarItem)CreateSerializableVersion(x))),
                meleeAbilityBarItems = new(MeleeAbilities.DefaultAbilityBarItems()
                    .Select(x => (SerializableMeleeAbilityBarItem)CreateSerializableVersion(x))),
                rangedAbilityBarItems = new(RangedAbilities.DefaultAbilityBarItems()
                    .Select(x => (SerializableRangedAbilityBarItem)CreateSerializableVersion(x)))
            };
            return data;
        }

        public Dictionary<CombatStyle, AbilityBar> BuildAllAbilityBars(ICombatController cc = null)
        {
            Dictionary<CombatStyle, AbilityBar> GetAbilityBar = new();
            foreach (var combatStyle in CombatStyles.CoreCombatStyles)
            {
                GetAbilityBar[combatStyle] = CreateAbilityBar(combatStyle, cc);
            }
            return GetAbilityBar;
        }

        public AbilityBar CreateAbilityBar(CombatStyle combatStyle, ICombatController cc)
        {
            AbilityBar bar = new(cc, GetAbilityBarItems(combatStyle)?.Select(x => x.CreateAbilityBarItem()).ToList());
            return bar;
        }

        public IEnumerable<SerializableAbilityBarItem> GetAbilityBarItems(CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyle.Mage => mageAbilityBarItems?.Where(x => x != null) 
                    ?? new List<SerializableMageAbilityBarItem>(),
                CombatStyle.Melee => meleeAbilityBarItems?.Where(x => x != null)
                    ?? new List<SerializableMeleeAbilityBarItem>(),
                CombatStyle.Ranged => rangedAbilityBarItems?.Where(x => x != null)
                    ?? new List<SerializableRangedAbilityBarItem>(),
                CombatStyle.Unarmed => unarmedAbilityBarItems?.Where(x => x != null)
                    ?? new List<SerializableUnarmedAbilityBarItem>(),
                _ => null
            };
        }
    }
}