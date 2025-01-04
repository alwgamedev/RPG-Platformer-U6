using System;

namespace RPGPlatformer.Skills
{
    //NOTE: keep this is as a class rather than a struct, as having these stored by reference 
    //makes gaining experience more straightforward
    [Serializable]
    public class SkillProgressionData
    {
        public int Level { get; set; }
        public int XP { get; set; }

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