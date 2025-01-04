using System.Collections.Generic;
using RPGPlatformer.Combat;

namespace RPGPlatformer.Skills
{
    using static CombatStyles;
    public static class CharacterSkillBook
    {
        public static CharacterSkill Health = new("Health");
        public static CharacterSkill Defense = new("Defense");
        public static CharacterSkill Magic = new("Magic");
        public static CharacterSkill Melee = new("Melee");
        public static CharacterSkill Range = new("Range");

        public static readonly List<CharacterSkill> SkillList = new()
        {
            Health, Defense, Magic, Melee, Range
        };

        public static CharacterSkill GetCombatSkill(CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyle.Mage => Magic,
                CombatStyle.Melee => Melee,
                CombatStyle.Ranged => Range,
                CombatStyle.Unarmed => Melee,
                _ => null
            };
        }
    }
}