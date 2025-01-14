using System.Collections.Generic;
using System.Linq;
using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    public static class AbilityTools
    {
        public static List<AbilityBarItem> DefaultAbilityBarItems(CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyle.Mage => MageAbilities.DefaultAbilityBarItems(),
                CombatStyle.Melee => MeleeAbilities.DefaultAbilityBarItems(),
                CombatStyle.Ranged => RangedAbilities.DefaultAbilityBarItems(),
                CombatStyle.Unarmed => UnarmedAbilities.DefaultAbilityBarItems(),
                _ => new List<AbilityBarItem>()
            };
        }

        public static IEnumerable<AttackAbility> GetAllAbilities(CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyle.Mage => MageAbilities.AllAbilities,
                CombatStyle.Melee => MeleeAbilities.AllAbilities,
                CombatStyle.Ranged => RangedAbilities.AllAbilities,
                CombatStyle.Unarmed => UnarmedAbilities.AllAbilities,
                _ => new List<AttackAbility>()
            };
        }

        public static AttackAbility GetAbility(CombatStyle combatStyle, string abilityName)
        {
            switch(combatStyle)
            {
                case CombatStyle.Mage:
                    return MageAbilities.GetAbility(abilityName);
                case CombatStyle.Melee:
                    return MeleeAbilities.GetAbility(abilityName);
                case CombatStyle.Ranged:
                    return RangedAbilities.GetAbility(abilityName);
                case CombatStyle.Unarmed:
                    return UnarmedAbilities.GetAbility(abilityName);
            }
            return null;
        }

        public static bool TryGetAbility(CombatStyle combatStyle, string abilityName, out AttackAbility ability)
        {
            ability = null; 
            switch(combatStyle)
            {
                case CombatStyle.Mage:
                    return MageAbilities.TryGetAbility(abilityName, out ability);
                case CombatStyle.Melee:
                    return MeleeAbilities.TryGetAbility(abilityName, out ability);
                case CombatStyle.Ranged:
                    return RangedAbilities.TryGetAbility(abilityName, out ability);
                case CombatStyle.Unarmed:
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