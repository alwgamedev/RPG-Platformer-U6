using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace RPGPlatformer.Combat
{
    using static CombatStyles;

    public static class UnarmedAbilities
    {
        public enum UnarmedAbilitiesEnum
        {
            Punch
        }

        public static AttackAbility GetAbility(string abilityName)
        {
            return GetAbility((UnarmedAbilitiesEnum)Enum.Parse(typeof(AttackAbility), abilityName));
        }

        public static AttackAbility GetAbility(UnarmedAbilitiesEnum ability)
        {
            return ability switch
            {
                UnarmedAbilitiesEnum.Punch => Punch,
                _ => null
            };
        }

        public static bool TryGetAbility(string abilityName, out AttackAbility ability)
        {
            ability = null;
            string formattedName = string.Concat(abilityName.Where(x => x != ' '));
            if (Enum.TryParse(typeof(UnarmedAbilitiesEnum), formattedName, out var obj))
            {
                ability = GetAbility((UnarmedAbilitiesEnum)obj);
                return true;
            }
            return false;
        }

        public static List<AbilityBarItem> DefaultAbilityBarItems()
        {
            return new()
            {
                new(Punch, true)
            };
        }

        public static CloseRangeAbility Punch = new()
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Unarmed,
            AnimationState = "Punch",
            Cooldown = 3
        };

        public static IEnumerable<AttackAbility> AllAbilities = new List<AttackAbility>()
        {
            Punch
        };
    }
}