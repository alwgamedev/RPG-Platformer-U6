using RPGPlatformer.Combat;
using System.Collections.Generic;
using UnityEngine;

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
                recipient.GainExperience(CharacterSkillBook.Fitness, (int)(0.25f * xp));
                recipient.GainExperience(CharacterSkillBook.Defense, (int)(0.75f * xp));
            }
            else
            {
                recipient.GainExperience(CharacterSkillBook.Fitness, (int)(0.25f * xp));
                recipient.GainExperience(CharacterSkillBook.Defense, (int)(0.35f * xp));
                
                foreach (var entry in dmgByStyle)
                {
                    recipient.GainExperience(CharacterSkillBook.GetCombatSkill(entry.Key),
                        (int)Mathf.Ceil(0.4f * xp * entry.Value / totalDamage));
                }
            }
        }
    }
}