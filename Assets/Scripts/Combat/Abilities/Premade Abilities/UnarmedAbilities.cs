using System;
using System.Collections.Generic;

namespace RPGPlatformer.Combat
{
    using static CombatStyles;

    public static class UnarmedAbilities
    {
        public enum UnarmedAbilitiesEnum
        {
            Punch
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
            if (Enum.TryParse(typeof(UnarmedAbilitiesEnum), abilityName, out var obj))
            {
                ability = obj as AttackAbility;
                return true;
            }
            return false;
        }

        public static List<AbilityBarItem> DefaultAbilityBarData()
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
    }
}