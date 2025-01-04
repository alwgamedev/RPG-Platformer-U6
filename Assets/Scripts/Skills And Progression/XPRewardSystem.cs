using RPGPlatformer.Combat;
using System.Collections.Generic;

namespace RPGPlatformer.Skills
{
    using static CombatStyles;

    public static class XPRewardSystem
    {
        public static void AwardXPForCombatKill(int xp, IXPGainer recipient,
            Dictionary<CombatStyle, float> dmgByStyle)
        {
            float totalDamage = DamageTakenTracker.TotalDamageDealt(dmgByStyle);
            //(when dmgByStyle dictionary is null, this will be zero)
            
            if (totalDamage <= 0)
            {
                recipient.GainExperience(CharacterSkillBook.Health, (int)(0.5f * xp));
                recipient.GainExperience(CharacterSkillBook.Defense, (int)(0.5f * xp));
            }
            else
            {
                recipient.GainExperience(CharacterSkillBook.Health, (int)(0.3f * xp));
                recipient.GainExperience(CharacterSkillBook.Defense, (int)(0.3f * xp));
                
                foreach (var entry in dmgByStyle)
                {
                    recipient.GainExperience(CharacterSkillBook.GetCombatSkill(entry.Key),
                        (int)(0.4f * xp * entry.Value / totalDamage));
                }
            }
        }
    }
}