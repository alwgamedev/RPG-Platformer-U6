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
            return combatStyle switch
            {
                CombatStyle.Mage => MageAbilities.GetAbility(abilityName),
                CombatStyle.Melee => MeleeAbilities.GetAbility(abilityName),
                CombatStyle.Ranged => RangedAbilities.GetAbility(abilityName),
                CombatStyle.Unarmed => UnarmedAbilities.GetAbility(abilityName),
                _ => null,
            };
        }

        public static bool TryGetAbility(CombatStyle combatStyle, string abilityName, out AttackAbility ability)
        {
            ability = null;
            return combatStyle switch
            {
                CombatStyle.Mage => MageAbilities.TryGetAbility(abilityName, out ability),
                CombatStyle.Melee => MeleeAbilities.TryGetAbility(abilityName, out ability),
                CombatStyle.Ranged => RangedAbilities.TryGetAbility(abilityName, out ability),
                CombatStyle.Unarmed => UnarmedAbilities.TryGetAbility(abilityName, out ability),
                _ => false,
            };
        }

        public static bool TryGetResources(AttackAbility ability, out AbilityResourceData resources)
        {
            resources = null;
            if (GlobalGameTools.Instance == null) return false;

            return GlobalGameTools.Instance.ResourcesManager
                .AbilityResources.TryGetResources(ability, out resources);
        }
    }
}