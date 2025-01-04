using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static RangedAbilities;
    using static MageAbilities;
    using static MeleeAbilities;
    using static UnarmedAbilities;

    public abstract class SerializedAbilityBarItem
    {
        [SerializeField] protected bool includeInAutoCastCycle;

        public virtual bool IncludeInAutoCastCycle => includeInAutoCastCycle;

        public abstract AbilityBarItem CreateAbilityBarItem();
    }

    [Serializable]
    public class SerializedRangedAbilityBarItem : SerializedAbilityBarItem
    {
        [SerializeField] RangedAbilitiesEnum abilityEnum;

        public override AbilityBarItem CreateAbilityBarItem()
        {
            AttackAbility ability = GetAbility(abilityEnum);
            return new(ability, includeInAutoCastCycle);
        }
    }

    [Serializable]
    public class SerializedMageAbilityBarItem : SerializedAbilityBarItem
    {
        [SerializeField] MageAbilitiesEnum abilityEnum;

        public override AbilityBarItem CreateAbilityBarItem()
        {
            AttackAbility ability = GetAbility(abilityEnum);
            return new(ability, includeInAutoCastCycle);
        }
    }

    [Serializable]
    public class SerializedMeleeAbilityBarItem : SerializedAbilityBarItem
    {
        [SerializeField] MeleeAbilitiesEnum abilityEnum;

        public override AbilityBarItem CreateAbilityBarItem()
        {
            AttackAbility ability = GetAbility(abilityEnum);
            return new(ability, includeInAutoCastCycle);
        }
    }

    [Serializable]
    public class SerializedUnarmedAbilityBarItem : SerializedAbilityBarItem
    {
        [SerializeField] UnarmedAbilitiesEnum abilityEnum;

        public override AbilityBarItem CreateAbilityBarItem()
        {
            AttackAbility ability = GetAbility(abilityEnum);
            return new(ability, includeInAutoCastCycle);
        }
    }
}