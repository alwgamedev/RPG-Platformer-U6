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
                unarmedAbilityBarItems = new(UnarmedAbilities.DefaultAbilityBarData()
                    .Select(x => (SerializableUnarmedAbilityBarItem)CreateSerializableVersion(x))),
                mageAbilityBarItems = new(MageAbilities.DefaultAbilityBarData()
                    .Select(x => (SerializableMageAbilityBarItem)CreateSerializableVersion(x))),
                meleeAbilityBarItems = new(MeleeAbilities.DefaultAbilityBarData()
                    .Select(x => (SerializableMeleeAbilityBarItem)CreateSerializableVersion(x))),
                rangedAbilityBarItems = new(RangedAbilities.DefaultAbilityBarData()
                    .Select(x => (SerializableRangedAbilityBarItem)CreateSerializableVersion(x)))
            };
            return data;
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
                CombatStyle.Mage => mageAbilityBarItems.Where(x => x != null),
                CombatStyle.Melee => meleeAbilityBarItems.Where(x => x != null),
                CombatStyle.Ranged => rangedAbilityBarItems.Where(x => x != null),
                CombatStyle.Unarmed => unarmedAbilityBarItems.Where(x => x != null),
                _ => null
            };
        }
    }
}