using System;
using UnityEngine;

namespace RPGPlatformer.Skills
{
    public interface ISkillProgressionData
    {
        int Level { get; set; }
        int XP { get; set; }

        int GainExperience(int xp, IXPTable xpTable);
        void RecomputeLevel(IXPTable xpTable);
    }

    //NOTE: keep this is as a class rather than a struct, as having these stored by reference 
    //makes gaining experience more straightforward
    [Serializable]
    public class SkillProgressionData : ISkillProgressionData
    {
        [Min(1)][SerializeField] int level = 1;
        [Min(0)][SerializeField] int xp;

        public int Level
        {
            get => level;
            set => level = value;
        }
        public int XP
        {
            get => xp;
            set => xp = value;
        }

        public int GainExperience(int xp, IXPTable xpTable)
        {
            int startingXP = XP;
            XP = Math.Min(XP + xp, xpTable.MaxXP());
            RecomputeLevel(xpTable);
            return XP - startingXP;
        }

        public void RecomputeLevel(IXPTable xpTable)
        {
            Level = xpTable.LevelAtXP(XP);
        }
    }
}