using System.Collections.Generic;

namespace RPGPlatformer.Skills
{
    public interface IXPTable
    {
        int MaxLevel { get; }

        int LevelAtXP(int xp);
        int LevelXPDelta(int level);
        int MaxXP();
        float PercentProgressTowardNextLevel(int currentXP, int? currentLevel = null);
        int XPAtLevel(int lvl);
    }

    public class XPTable : IXPTable
    {
        readonly int maxLevel;
        readonly Dictionary<int, int> LevelToXPDict = new();//key: level, value: xp at beginning of level

        public int MaxLevel => maxLevel;

        public XPTable(int maxLevel)
        {
            this.maxLevel = maxLevel;
        }

        public int MaxXP()
        {
            return XPAtLevel(maxLevel + 1) - 1;
        }

        public int LevelAtXP(int xp)
        {
            for (int i = 1; i < maxLevel; i++)
            {
                if (xp < XPAtLevel(i + 1))
                {
                    return i;
                }
            }
            return maxLevel;
        }

        public int XPAtLevel(int lvl)
        {
            if (LevelToXPDict.TryGetValue(lvl, out var xp))
            {
                return xp;
            }

            if (lvl <= 1)
            {
                xp = 0;
            }
            else if (lvl == 2)
            {
                xp = 100;
            }
            else
            {
                xp = (int)(0.4f * XPAtLevel(lvl - 1) + XPAtLevel(lvl - 2) + 200);
            }

            LevelToXPDict[lvl] = xp;
            return xp;
        }

        public int LevelXPDelta(int level)
        {
            //if(level >= MaxLevel)
            //{
            //    return 0;
            //}

            return XPAtLevel(level + 1) - XPAtLevel(level);
        }

        public float PercentProgressTowardNextLevel(int currentXP, int? currentLevel = null)
        {
            int level = currentLevel.HasValue ? currentLevel.Value : LevelAtXP(currentXP);

            if (currentLevel >= maxLevel)
            {
                return 0;
            }

            float delta = XPAtLevel(level + 1) - XPAtLevel(level);
            float progress = currentXP - XPAtLevel(level);

            return progress / delta;
        }
    }
}