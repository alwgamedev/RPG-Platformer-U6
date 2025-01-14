using System;
using UnityEngine;

namespace RPGPlatformer.Skills
{
    //NOTE: keep this is as a class rather than a struct, as having these stored by reference 
    //makes gaining experience more straightforward
    [Serializable]
    public class SkillProgressionData
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

        public int GainExperience(int xp, XPTable xpTable)
        {
            int startingXP = XP;
            XP = Math.Min(XP + xp, xpTable.MaxXP());
            RecomputeLevel(xpTable);
            return XP - startingXP;
        }

        public void RecomputeLevel(XPTable xpTable)
        {
            Level = xpTable.LevelAtXP(XP);
        }
    }
}