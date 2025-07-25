﻿using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface ICombatController
    {
        public bool IsInCombat { get; }
        public bool ChannelingAbility { get; }
        public bool PoweringUp { get; }
        public bool FireButtonIsDown { get; }
        public bool HasStoredAction { get; }
        public TickTimer TickTimer { get; }
        public ICombatant Combatant { get; }
        public ICombatantMovementController MovementController{ get; }

        public event Action CombatEntered;
        public event Action CombatExited;
        public event Action OnDeath;
        public event Action OnRevive;
        public event Action<AttackAbility> OnCooldownStarted;
        public event Action OnFireButtonDown;
        public event Action OnFireButtonUp;
        public event Action OnChannelStarted;
        public event Action OnChannelEnded;
        public event Action OnPowerUpStarted;
        public event Action OnPowerUpEnded;
        public event Action OnMaximumPowerAchieved;
        public event Action<float> HealthChangeEffected;

        public void StartChannel();
        public void EndChannel(bool delayReleaseUntilFireButtonUp = false);
        public void StartPowerUp(AttackAbility ability);
        public void EndPowerUp();
        public void OnAbilityExecute(AttackAbility ability);
        public void MaximumPowerAchieved();
        public void StoreAction(Action action, bool channelWhileStored = true, 
            bool endChannelOnExecute = true);
        public void OnWeaponEquip();
        public void OnCombatEntry();
        public void OnCombatExit();
        public void OnInsufficientStamina();
        public void OnInsufficientWrath();
        public Vector2 GetAimPosition();
        //public void HoldAim(int duration);
        public void PlayAnimation(string stateName, CombatStyle combatStyle);
        public void PlayPowerUpAnimation(string stateName, CombatStyle combatStyle);
        public void PlayChannelAnimation(string stateName, CombatStyle combatStyle);
        public void StartCooldown(AttackAbility ability);
    }
}