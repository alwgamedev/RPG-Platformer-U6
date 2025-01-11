using System;

namespace RPGPlatformer.Skills
{
    public class CharacterSkill
    //holds all the information relevant to a particular skill (e.g. Defence, Magic, ...)
    //for now will just have the XPTable for that skill,
    //later on it could also have information about achievements unlocked at certain levels etc.
    {

        public string SkillName { get; init; }
        public XPTable XPTable { get; init; }

        public CharacterSkill(string skillName, int MaxLevel = 40)
        {
            SkillName = skillName;
            XPTable = new(MaxLevel);
        }
        //with standard levelling rule, level 40 is about 1.15mil xp
    }
}