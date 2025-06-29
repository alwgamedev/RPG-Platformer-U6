using RPGPlatformer.Combat;
using System;
using System.Collections.Generic;

namespace RPGPlatformer.Skills
{
    public enum StandardCharacterSkill
    {
        Fitness, Defense, Magic, Melee, Ranged
    }

    public static class CharacterSkillBook
    {
        public static CharacterSkill Fitness = new("Fitness");
        public static CharacterSkill Defense = new("Defense");
        public static CharacterSkill Magic = new("Magic");
        public static CharacterSkill Melee = new("Melee");
        public static CharacterSkill Ranged = new("Ranged");

        public static IEnumerable<CharacterSkill> AllSkills()
        {
            foreach (var s in Enum.GetValues(typeof(StandardCharacterSkill)))
            {
                yield return GetCharacterSkill((StandardCharacterSkill)s);
            }
        }

        public static CharacterSkill GetCharacterSkill(StandardCharacterSkill skill)
        {
            return skill switch
            {
                StandardCharacterSkill.Fitness => Fitness,
                StandardCharacterSkill.Defense => Defense,
                StandardCharacterSkill.Magic => Magic,
                StandardCharacterSkill.Melee => Melee,
                StandardCharacterSkill.Ranged => Ranged,
                _ => null
            };
        }

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