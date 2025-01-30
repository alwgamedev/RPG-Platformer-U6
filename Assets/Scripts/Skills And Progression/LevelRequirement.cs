using System;
using UnityEngine;

namespace RPGPlatformer.Skills
{
    [Serializable]
    public struct LevelRequirement
    {
        [SerializeField] StandardCharacterSkill skill;
        [SerializeField] int level;

        public StandardCharacterSkill Skill => skill;
        public int Level => level;
    }
}