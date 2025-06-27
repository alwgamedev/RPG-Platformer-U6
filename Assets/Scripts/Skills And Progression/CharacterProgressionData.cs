using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Skills
{
    [Serializable]
    public class CharacterProgressionData
    {
        Dictionary<CharacterSkill, SkillProgressionData> SkillLookup;

        [SerializeField] SkillProgressionData fitness = new();
        [SerializeField] SkillProgressionData defense = new();
        [SerializeField] SkillProgressionData magic = new();
        [SerializeField] SkillProgressionData melee = new();
        [SerializeField] SkillProgressionData ranged = new();

        public bool ForceLevels { get; set; }

        //properties for save system
        public SkillProgressionData Fitness
        {
            get => fitness;
            set => fitness = value ?? new();
        }
        public SkillProgressionData Defense
        {
            get => defense;
            set => defense = value ?? new();
        }
        public SkillProgressionData Magic
        {
            get => magic;
            set => magic = value ?? new();
        }
        public SkillProgressionData Melee
        {
            get => melee;
            set => melee = value ?? new();
        }
        public SkillProgressionData Ranged
        {
            get => ranged;
            set => ranged = value ?? new();
        }

        public int GetLevel(CharacterSkill skill)
        {
            return SkillLookup[skill].Level;
        }

        public int GetXP(CharacterSkill skill)
        {
            return SkillLookup[skill].XP;
        }

        public float GetXPFraction(CharacterSkill skill)
        {
            return skill.XPTable.PercentProgressTowardNextLevel(GetXP(skill), GetLevel(skill));
        }

        public int TotalLevel()
        {
            int result = 0;
            foreach (var entry in SkillLookup)
            {
                result += entry.Value.Level;
            }
            return result;
        }

        public int CombatLevel()
        {
            return fitness.Level + defense.Level + Math.Max(Math.Max(magic.Level, melee.Level), ranged.Level);
        }

        public int AutoCalculatedHealthPoints()
        {
            return 4000 + (312 * (GetLevel(CharacterSkillBook.Fitness) - 1));
        }

        public SkillProgressionData GetProgressionData(CharacterSkill skill)
        {
            if (SkillLookup == null)
            {
                Configure();
            }
            return SkillLookup[skill];
        }

        public void Configure()
        {
            BuildLookup();
            if (!ForceLevels)
            {
                RecomputeLevels();
            }
        }

        protected void BuildLookup()
        {
            SkillLookup = new()
            {
                [CharacterSkillBook.Fitness] = fitness,
                [CharacterSkillBook.Defense] = defense,
                [CharacterSkillBook.Magic] = magic,
                [CharacterSkillBook.Melee] = melee,
                [CharacterSkillBook.Ranged] = ranged
            };
        }

        protected void RecomputeLevels()
        {
            foreach(var entry in SkillLookup)
            {
                entry.Value.RecomputeLevel(entry.Key.XPTable);
            }
        }
    }
}