using System.Collections.Generic;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class PlayerAbilityBarUI : AbilityBarUI
    {
        IAbilityBarOwner player;

        //protected override void Awake()
        //{
        //    base.Awake();

        //    player = FindAnyObjectByType<PlayerCombatController>();
        //    player.AbilityBarResetEvent += DisplayPlayerAbilityBar;
        //    player.OnCooldownStarted += OnCooldownStart;
        //}

        private void Start()
        {
            SubscribeToPlayer();
            DisplayPlayerAbilityBar();
            //^do an initial display in case we do Start after player and missed the first ability bar reset event
        }

        private void SubscribeToPlayer()
        {
            player = (IAbilityBarOwner)GlobalGameTools.Instance.Player;
            player.AbilityBarResetEvent += DisplayPlayerAbilityBar;
            player.OnCooldownStarted += OnCooldownStart;
        }

        private void DisplayPlayerAbilityBar()
        {
            ConnectAbilityBar(player?.CurrentAbilityBar, new List<CombatStyle>());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (player != null)
            {
                player.AbilityBarResetEvent -= DisplayPlayerAbilityBar;
                player.OnCooldownStarted -= OnCooldownStart;
            }
        }
    }
}