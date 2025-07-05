namespace RPGPlatformer.Skills
{
    public interface ICharacterSkill
    {
        string SkillName { get; }
        IXPTable XPTable { get; }
    }
}