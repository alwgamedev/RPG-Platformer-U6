using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Skills
{
    [Serializable]
    public class CharacterProgressionData
    {
        Dictionary<CharacterSkill, SkillProgressionData> SkillLookup;

        //all need to be public get/set in order to serialize to json
        public SkillProgressionData Health { get; set; } = new();
        public SkillProgressionData Defense { get; set; } = new();
        public SkillProgressionData Magic { get; set; } = new();
        public SkillProgressionData Melee { get; set; } = new();
        public SkillProgressionData Range { get; set; } = new(); 

        //below this is mainly to allow flexible initialization of the dictionary
        public bool TryGetProgressionData(CharacterSkill skill, out SkillProgressionData data)
        {
            data = null;

            if (SkillLookup == null)
            {
                Configure();
            }

            if (!SkillLookup.ContainsKey(skill))
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
                [CharacterSkillBook.Health] = Health,
                [CharacterSkillBook.Defense] = Defense,
                [CharacterSkillBook.Magic] = Magic,
                [CharacterSkillBook.Melee] = Melee,
                [CharacterSkillBook.Range] = Range
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