using System.Collections.Generic;

namespace RPGPlatformer.Skills
{
    public class XPTable
    {
        public readonly int MaxLevel;

        public readonly Dictionary<int, int> XPAtLevel = new();//key: level, value: xp at beginning of level

        public XPTable(int MaxLevel)
        {
            this.MaxLevel = MaxLevel;
            InitializeXPTable();
        }

        public int MaxXP()
        {
            return XPAtLevel[MaxLevel];
        }

        public int LevelAtXP(int xp)
        {
            for (int i = 1; i < MaxLevel; i ++)
            {
                if(xp < XPAtLevel[i + 1])
                {
                    return i;
                }
            }
            return MaxLevel;
        }

        public int LevelXPDelta(int level)
        {
            if(level >= MaxLevel)
            {
                return 0;
            }

            return XPAtLevel[level + 1] - XPAtLevel[level];
        }

        public float PercentProgressTowardNextLevel(int currentXP, int? currentLevel = null)
        {
            int level = currentLevel.HasValue ? currentLevel.Value : LevelAtXP(currentXP);

            if(currentLevel >= MaxLevel)
            {
                return 0;
            }

            float delta = XPAtLevel[level + 1] - XPAtLevel[level];
            float progress = currentXP - XPAtLevel[level];

            return progress / delta;
        }

        private void InitializeXPTable()
        {
            XPAtLevel.Clear();

            for (int i = 1; i <= MaxLevel; i++)
            {
                if (i == 1)
                {
                    XPAtLevel[i] = 0;
                }
                else if (i == 2)
                {
                    XPAtLevel[i] = 100;
                }
                else
                {
                    XPAtLevel[i] = (int)(0.4f * XPAtLevel[i - 1] + XPAtLevel[i - 2] + 200);
                }
            }
        }
    }
}