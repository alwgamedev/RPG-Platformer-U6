using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Skills
{
    [Serializable]
    public class CharacterProgressionData
    {
        Dictionary<CharacterSkill, SkillProgressionData> SkillLookup;

        [SerializeField] SkillProgressionData health = new();
        [SerializeField] SkillProgressionData defense = new();
        [SerializeField] SkillProgressionData magic = new();
        [SerializeField] SkillProgressionData melee = new();
        [SerializeField] SkillProgressionData range = new();

        //TO-DO: cache total level and combat level and only recompute when needed (minor performance improvement)

        //all need to be public get/set in order to serialize to json
        public SkillProgressionData Health
        {
            get => health;
            set => health = value ?? new();
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
        public SkillProgressionData Range
        {
            get => range;
            set => range = value ?? new();
        }

        public int GetLevel(CharacterSkill skill)
        {
            return GetProgressionData(skill).Level;
        }

        public int TotalLevel()
        {
            int result = 0;
            foreach (var entry in SkillLookup)
            {
                if(entry.Value != null)
                {
                    result += entry.Value.Level;
                }
            }
            return result;
        }

        public int CombatLevel()
        {
            return Health.Level + Defense.Level + Math.Max(Math.Max(Magic.Level, Melee.Level), Range.Level);
        }

        public SkillProgressionData GetProgressionData(CharacterSkill skill)
        {
            if(SkillLookup == null)
            {
                Configure();
            }
            return SkillLookup[skill];
        }

        //below this is mainly to allow flexible initialization of the dictionary
        public bool TryGetProgressionData(CharacterSkill skill, out SkillProgressionData data)
        {
            data = null;
            if (skill == null)
            if (SkillLookup == null)
            {
                Configure();
            }

            if (skill == null || !SkillLookup.ContainsKey(skill))
            {
                Debug.Log($"{GetType().Name} could not find data for that skill.");
                return false;
            }

            data = SkillLookup[skill];
            return true;
        }

        public void Configure()
        {
            BuildLookup();
            InitializeLevels();
        }

        protected void BuildLookup()
        {
            SkillLookup = new()
            {
                [CharacterSkillBook.Fitness] = Health,
                [CharacterSkillBook.Defense] = Defense,
                [CharacterSkillBook.Magic] = Magic,
                [CharacterSkillBook.Melee] = Melee,
                [CharacterSkillBook.Ranged] = Range
            };
        }

        protected void InitializeLevels()
        {
            foreach(var entry in SkillLookup)
            {
                entry.Value.RecomputeLevel(entry.Key.XPTable);
            }
        }
    }
}