namespace RPGPlatformer.Skills
{
    public readonly struct XPGainEventData
    {
        public readonly ICharacterSkill skill;
        public readonly ISkillProgressionData progressionData;
        public readonly int xpGained;

        //Note: the classes sending this data will be responsible for making sure the parameters are valid
        //(i.e. don't exceed skill's max xp and max level, and that Level is accurate to XP)

        public XPGainEventData(ICharacterSkill skill, ISkillProgressionData progressionData, int xpGained)
        {
            this.skill = skill;
            this.progressionData = progressionData;
            this.xpGained = xpGained;
        }
    }
}