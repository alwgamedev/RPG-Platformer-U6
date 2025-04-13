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

        //in the future this is going to be an issue when we want to include e.g. defensive abilities on
        //a mage ability bar. Try having them all just be List<SerializableAbilityBarItem>()
        //-- get that to work (they should still use their overriden functionality for their combat style)

        //also to avoid having to directly list out each combat style you could make an
        //array of List<seritem> where the index in the array corresponds to combat style as int
        //(then you just have to know the int to correctly set things up in inspector)

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

        public SerializableCharacterAbilityBarData(
            List<SerializableUnarmedAbilityBarItem> unarmedAbilityBarItems = null,
            List<SerializableMageAbilityBarItem> mageAbilityBarItems = null,
            List<SerializableMeleeAbilityBarItem> meleeAbilityBarItems = null,
            List<SerializableRangedAbilityBarItem> rangedAbilityBarItems = null) 
        {
            this.unarmedAbilityBarItems = unarmedAbilityBarItems ?? new();
            this.mageAbilityBarItems = mageAbilityBarItems ?? new();
            this.meleeAbilityBarItems = meleeAbilityBarItems ?? new();
            this.rangedAbilityBarItems = rangedAbilityBarItems ?? new();
        }

        public SerializableCharacterAbilityBarData(Dictionary<CombatStyle, List<AbilityBarItem>> itemsLookup)
        {
            if (itemsLookup.TryGetValue(CombatStyle.Unarmed, out var unarmedAbilities))
            {
                unarmedAbilityBarItems = unarmedAbilities.Select(
                    x => (SerializableUnarmedAbilityBarItem)CreateSerializableVersion(x)).ToList();
            }
            if (itemsLookup.TryGetValue(CombatStyle.Mage, out var mageAbilities))
            {
                mageAbilityBarItems = mageAbilities.Select(
                    x => (SerializableMageAbilityBarItem)CreateSerializableVersion(x)).ToList();
            }
            if (itemsLookup.TryGetValue(CombatStyle.Melee, out var meleeAbilities))
            {
                meleeAbilityBarItems = meleeAbilities.Select(
                    x => (SerializableMeleeAbilityBarItem)CreateSerializableVersion(x)).ToList();
            }
            if (itemsLookup.TryGetValue(CombatStyle.Ranged, out var rangedAbilities))
            {
                rangedAbilityBarItems = rangedAbilities.Select(
                    x => (SerializableRangedAbilityBarItem)CreateSerializableVersion(x)).ToList();
            }
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
            AbilityBar bar = new(cc, GetAbilityBarItems(combatStyle)?.Select(x => x?.CreateAbilityBarItem()).ToList());
            return bar;
        }

        public IEnumerable<AbilityBarItem> CreateAbilityBarItems(CombatStyle combatStyle)
        {
            return GetAbilityBarItems(combatStyle).Select(x => x?.CreateAbilityBarItem());
        }

        public IEnumerable<SerializableAbilityBarItem> GetAbilityBarItems(CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyle.Mage => mageAbilityBarItems ?? new List<SerializableMageAbilityBarItem>(),
                CombatStyle.Melee => meleeAbilityBarItems ?? new List<SerializableMeleeAbilityBarItem>(),
                CombatStyle.Ranged => rangedAbilityBarItems ?? new List<SerializableRangedAbilityBarItem>(),
                CombatStyle.Unarmed => unarmedAbilityBarItems ?? new List<SerializableUnarmedAbilityBarItem>(),
                _ => new List<SerializableAbilityBarItem>()
            };
        }
    }
}