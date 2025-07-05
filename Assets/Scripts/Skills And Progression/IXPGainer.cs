namespace RPGPlatformer.Skills
{
    public interface IXPGainer
    {
        public void GainExperience(ICharacterSkill skill, int xp);
    }
}