namespace RPGPlatformer.Skills
{
    public class CharacterSkill : ICharacterSkill

    //holds all the information relevant to a particular skill (e.g. Defence, Magic, ...)
    //for now will just have the XPTable for that skill,
    //later on it could also have information about achievements unlocked at certain levels etc.
    {

        public string SkillName { get; init; }
        public IXPTable XPTable { get; init; }

        public CharacterSkill(string skillName, int MaxLevel = 40)
        {
            SkillName = skillName;
            XPTable = new XPTable(MaxLevel);
        }
        //with standard levelling rule, level 40 is about 1.15mil xp
    }
}