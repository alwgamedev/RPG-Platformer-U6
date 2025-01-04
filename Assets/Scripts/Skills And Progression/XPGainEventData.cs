namespace RPGPlatformer.Skills
{
    public readonly struct XPGainEventData
    {
        public readonly CharacterSkill skill;
        public readonly SkillProgressionData progressionData;
        public readonly int xpGained;

        //Note: the classes sending this data will be responsible for making sure the parameters are valid
        //(i.e. don't exceed skill's max xp and max level, and that Level is accurate to XP)

        public XPGainEventData(CharacterSkill skill, SkillProgressionData progressionData, int xpGained)
        {
            this.skill = skill;
            this.progressionData = progressionData;
            this.xpGained = xpGained;
        }
    }
}