namespace RPGPlatformer.Skills
{
    public interface IXPGainer
    {
        public void GainExperience(CharacterSkill skill, int xp);
    }
}