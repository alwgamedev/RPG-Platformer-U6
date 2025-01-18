using System.Collections.Generic;
using RPGPlatformer.Combat;

namespace RPGPlatformer.Skills
{
    using static CombatStyles;
    public static class CharacterSkillBook
    {
        public static CharacterSkill Fitness = new("Health");
        public static CharacterSkill Defense = new("Defense");
        public static CharacterSkill Magic = new("Magic");
        public static CharacterSkill Melee = new("Melee");
        public static CharacterSkill Ranged = new("Range");

        public static readonly List<CharacterSkill> SkillList = new()
        {
            Fitness, Defense, Magic, Melee, Ranged
        };

        public static CharacterSkill GetCombatSkill(CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyle.Mage => Magic,
                CombatStyle.Melee => Melee,
                CombatStyle.Ranged => Ranged,
                CombatStyle.Unarmed => Melee,
                _ => null
            };
        }
    }
}