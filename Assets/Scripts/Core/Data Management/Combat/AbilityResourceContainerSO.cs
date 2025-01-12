using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Combat;
using System.Linq;

namespace RPGPlatformer.Core
{
    [CreateAssetMenu(menuName = "Data Management/Ability Resource Container", fileName = "New Ability Resource Container")]
    public class AbilityResourceContainerSO : ScriptableObject
    {
        [Header("Mage Abilities")]
        [SerializeField] List<MageAbilityResource> mageAbilityResources;

        [Header("Melee Abilities")]
        [SerializeField] List<MeleeAbilityResource> meleeAbilityResources;

        [Header("Range Abilities")]
        [SerializeField] List<RangedAbilityResource> rangedAbilityResources;

        [Header("Unarmed Abilities")]
        [SerializeField] List<UnarmedAbilityResource> unarmedAbilityResources;

        [Header("Other Abilities")]
        [SerializeField] List<FlexibleAbilityResource> otherAbilityResources;

        public IEnumerable<AbilityResource> GetResourcesList(CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyle.Mage => mageAbilityResources,
                CombatStyle.Melee => meleeAbilityResources,
                CombatStyle.Ranged => rangedAbilityResources,
                _ => otherAbilityResources
            };
        }

        public bool TryGetResources(AttackAbility ability, out AbilityResourceData data)
        {
            IEnumerable<AbilityResource> resources = GetResourcesList(ability.CombatStyle);
            data = resources?.FirstOrDefault(x => x.AbilityName == ability.GetAbilityName())?.AbilityData;
            return data != null;
        }
    }
}