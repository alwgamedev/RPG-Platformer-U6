using System;
using System.Text.Json.Nodes;

namespace RPGPlatformer.Skills
{
    public interface ICharacterProgressionManager : IXPGainer
    {
        int AutoCalculatedHealthPoints { get; }
        bool CanGainXP { get; }
        int CombatLevel { get; }
        int TotalLevel { get; }

        event Action<XPGainEventData> ExperienceGained;

        int GetLevel(ICharacterSkill skill);
        int GetLevel(StandardCharacterSkill skill);
        int GetXP(ICharacterSkill skill);
        float GetXPFraction(ICharacterSkill skill);
    }
}