using System;
using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    using static MageAbilities;
    using static MeleeAbilities;
    using static RangedAbilities;
    using static UnarmedAbilities;

    public static class AbilityTools
    {
        public static AttackAbility GetAbility(CombatStyle combatStyle, string abilityName)
        {
            if(combatStyle == CombatStyle.Mage)
            {
                return MageAbilities.GetAbility(
                    (MageAbilitiesEnum)Enum.Parse(typeof(MageAbilitiesEnum), abilityName));
            }
            else if (combatStyle == CombatStyle.Melee)
            {
                return MeleeAbilities.GetAbility(
                    (MeleeAbilitiesEnum)Enum.Parse(typeof(MeleeAbilitiesEnum), abilityName));
            }
            else if (combatStyle == CombatStyle.Ranged)
            {
                return RangedAbilities.GetAbility(
                    (RangedAbilitiesEnum)Enum.Parse(typeof(RangedAbilitiesEnum), abilityName));
            }
            else if (combatStyle == CombatStyle.Unarmed)
            {
                return UnarmedAbilities.GetAbility(
                    (UnarmedAbilitiesEnum)Enum.Parse(typeof(UnarmedAbilities), abilityName));
            }
            return null;
        }

        public static bool TryGetAbility(CombatStyle combatStyle, string abilityName, out AttackAbility ability)
        {
            ability = null; 
            if (combatStyle == CombatStyle.Mage)
            {
                return MageAbilities.TryGetAbility(abilityName, out ability);
            }
            else if (combatStyle == CombatStyle.Melee)
            {
                return MeleeAbilities.TryGetAbility(abilityName, out ability);
            }
            else if (combatStyle == CombatStyle.Ranged)
            {
                return RangedAbilities.TryGetAbility(abilityName, out ability);
            }
            else if (combatStyle == CombatStyle.Unarmed)
            {
                return UnarmedAbilities.TryGetAbility(abilityName, out ability);
            }
            return false;
        }

        public static bool TryGetResources(AttackAbility ability, out AbilityResourceData abilityData)
        {
            abilityData = null;
            if (GlobalGameTools.Instance == null) return false;

            return GlobalGameTools.Instance.ResourcesManager
                .AbilityResources.TryGetResources(ability, out abilityData);
        }
    }
}