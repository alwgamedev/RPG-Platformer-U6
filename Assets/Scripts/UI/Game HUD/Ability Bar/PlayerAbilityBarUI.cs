using System.Collections.Generic;
using RPGPlatformer.Combat;

namespace RPGPlatformer.UI
{
    public class PlayerAbilityBarUI : AbilityBarUI
    {
        IAbilityBarOwner player;

        protected override void Awake()
        {
            base.Awake();

            player = FindAnyObjectByType<PlayerCombatController>();
            player.AbilityBarResetEvent += DisplayPlayerAbilityBar;
            player.OnCooldownStarted += OnCooldownStart;
        }

        private void DisplayPlayerAbilityBar()
        {
            ConnectAbilityBar(player?.CurrentAbilityBar, new List<CombatStyle>());
        }
    }
}