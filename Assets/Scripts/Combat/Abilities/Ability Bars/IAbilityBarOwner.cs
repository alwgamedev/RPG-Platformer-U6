using System;

namespace RPGPlatformer.Combat
{
    public interface IAbilityBarOwner
    {
        public AbilityBar CurrentAbilityBar { get; }

        public event Action AbilityBarResetEvent;
        public event Action<AttackAbility> OnCooldownStarted;
    }
}