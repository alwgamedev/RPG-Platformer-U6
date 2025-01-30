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

        public SkillProgressionData GetProgressionData(CharacterSkill skill)
        {
            if (SkillLookup == null)
            {
                Configure();
            }
            return SkillLookup[skill];
        }

        ////this is mainly to allow flexible initialization of the dictionary
        //public bool TryGetProgressionData(CharacterSkill skill, out SkillProgressionData data)
        //{
        //    data = null;
        //    if (skill == null)
        //    if (SkillLookup == null)
        //    {
        //        Configure();
        //    }

        //    if (skill == null || !SkillLookup.ContainsKey(skill))
        //    {
        //        Debug.Log($"{GetType().Name} could not find data for that skill.");
        //        return false;
        //    }

        //    data = SkillLookup[skill];
        //    return true;
        //}

        public void Configure()
        {
            BuildLookup();
            RecomputeLevels();
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

        //protected void RecomputeLevel(CharacterSkill skill)
        //{
        //    SkillLookup[skill].RecomputeLevel(skill.XPTable);
        //}

        protected void RecomputeLevels()
        {
            foreach(var entry in SkillLookup)
            {
                entry.Value.RecomputeLevel(entry.Key.XPTable);
            }
        }
    }
}