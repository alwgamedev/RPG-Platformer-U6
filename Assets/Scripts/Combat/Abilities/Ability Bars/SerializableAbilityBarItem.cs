using System;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static RangedAbilities;
    using static MageAbilities;
    using static MeleeAbilities;
    using static UnarmedAbilities;

    public abstract class SerializableAbilityBarItem
    {
        [SerializeField] protected bool includeInAutoCastCycle; 
        
        //yes we need both the [SF] and the public { get; set; } so that we can both edit these in inspector
        //and serialize/deserialize to JSON
        public bool IncludeInAutoCastCycle
        {
            get => includeInAutoCastCycle;
            set => includeInAutoCastCycle = value;
        }

        public SerializableAbilityBarItem(bool includeInAutoCastCycle)
        {
            this.includeInAutoCastCycle = includeInAutoCastCycle;
        }

        public abstract AbilityBarItem CreateAbilityBarItem();

        public static SerializableAbilityBarItem CreateSerializableVersion(AbilityBarItem abilityBarItem)
        {
            return CreateSerializableAbilityBarItem(abilityBarItem.ability.CombatStyle, abilityBarItem.ability.GetAbilityName(),
                abilityBarItem.includeInAutoCastCycle);
        }

        public static SerializableAbilityBarItem CreateSerializableAbilityBarItem(CombatStyle combatStyle,
            string abilityName, bool includeInAutoCastCycle)
        {
            string formattedName = string.Concat(abilityName.Where(x => x != ' '));

            if (combatStyle == CombatStyle.Mage)
            {
                if (Enum.TryParse(typeof(MageAbilitiesEnum), formattedName, out var enumValue))
                {
                    return new SerializableMageAbilityBarItem((MageAbilitiesEnum)enumValue, 
                        includeInAutoCastCycle);
                }
            }
            else if (combatStyle == CombatStyle.Melee)
            {
                if (Enum.TryParse(typeof(MeleeAbilitiesEnum), formattedName, out var enumValue))
                {
                    return new SerializableMeleeAbilityBarItem((MeleeAbilitiesEnum)enumValue, 
                        includeInAutoCastCycle);
                }
            }
            else if (combatStyle == CombatStyle.Ranged)
            {
                if (Enum.TryParse(typeof(RangedAbilitiesEnum), formattedName, out var enumValue))
                {
                    return new SerializableRangedAbilityBarItem((RangedAbilitiesEnum)enumValue, 
                        includeInAutoCastCycle);
                }
            }
            else if (combatStyle == CombatStyle.Unarmed)
            {
                if (Enum.TryParse(typeof(UnarmedAbilitiesEnum), formattedName,
                    out var enumValue))
                {
                    return new SerializableUnarmedAbilityBarItem((UnarmedAbilitiesEnum)enumValue,
                        includeInAutoCastCycle);
                }
            }
            Debug.Log($"could not create serializable {combatStyle} ability named {formattedName}");
            return null;
        }
    }

    [Serializable]
    public class SerializableRangedAbilityBarItem : SerializableAbilityBarItem
    {
        [SerializeField] RangedAbilitiesEnum ability;

        public RangedAbilitiesEnum Ability
        {
            get => ability;
            set => ability = value;
        }

        public SerializableRangedAbilityBarItem(RangedAbilitiesEnum ability, bool includeInAutoCastCycle)
            : base(includeInAutoCastCycle)
        {
            this.ability = ability;
        }

        public override AbilityBarItem CreateAbilityBarItem()
        {
            AttackAbility ability = GetAbility(this.ability);
            return new(ability, includeInAutoCastCycle);
        }
    }

    [Serializable]
    public class SerializableMageAbilityBarItem : SerializableAbilityBarItem
    {
        [SerializeField] MageAbilitiesEnum ability;

        public MageAbilitiesEnum Ability
        {
            get => ability;
            set => ability = value;
        }

        public SerializableMageAbilityBarItem(MageAbilitiesEnum ability, bool includeInAutoCastCycle)
            : base(includeInAutoCastCycle)
        {
            this.ability = ability;
        }

        public override AbilityBarItem CreateAbilityBarItem()
        {
            AttackAbility ability = GetAbility(this.ability);
            return new(ability, includeInAutoCastCycle);
        }
    }

    [Serializable]
    public class SerializableMeleeAbilityBarItem : SerializableAbilityBarItem
    {
        [SerializeField] MeleeAbilitiesEnum ability;

        public MeleeAbilitiesEnum Ability
        {
            get => ability;
            set => ability = value;
        }

        public SerializableMeleeAbilityBarItem(MeleeAbilitiesEnum ability, bool includeInAutoCastCycle)
            : base(includeInAutoCastCycle)
        {
            this.ability = ability;
        }

        public override AbilityBarItem CreateAbilityBarItem()
        {
            AttackAbility ability = GetAbility(this.ability);
            return new(ability, includeInAutoCastCycle);
        }
    }

    [Serializable]
    public class SerializableUnarmedAbilityBarItem : SerializableAbilityBarItem
    {
        [SerializeField] UnarmedAbilitiesEnum ability;

        public UnarmedAbilitiesEnum Ability
        {
            get => ability;
            set => ability = value;
        }

        public SerializableUnarmedAbilityBarItem(UnarmedAbilitiesEnum ability, bool includeInAutoCastcycle)
            : base(includeInAutoCastcycle)
        {
            this.ability = ability;
        }

        public override AbilityBarItem CreateAbilityBarItem()
        {
            AttackAbility ability = GetAbility(this.ability);
            return new(ability, includeInAutoCastCycle);
        }
    }
}