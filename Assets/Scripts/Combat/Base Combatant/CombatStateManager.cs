﻿using System;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    public class CombatStateManager : CombatStateManager<CombatStateGraph, CombatStateMachine, Combatant, AnimationControl>
    {
        public CombatStateManager(CombatStateMachine stateMachine, Combatant combatant, AnimationControl combatAnimator,
            float timeToLeaveCombat)
            : base(stateMachine, combatant, combatAnimator, timeToLeaveCombat) { }
    }

    public class CombatStateManager<T0, T1, T2, T3> : StateManager<T0, T1, T2>
        where T0 : CombatStateGraph
        where T1 : CombatStateMachine<T0>
        where T2 : Combatant
        where T3 : AnimationControl
    {
        public readonly T3 animationControl;
        public readonly float timeToLeaveCombat;//measured in ticks

        public event Action OnWeaponTick;

        protected int combatExitTimer;
        protected int weaponTickTimer;

        public CombatStateManager(T1 stateMachine, T2 combatant, T3 animationControl,
            float timeToLeaveCombat)
            : base(stateMachine, combatant)
        {
            this.animationControl = animationControl;
            this.timeToLeaveCombat = timeToLeaveCombat;
        }

        public override void Configure()
        {
            base.Configure();

            StateGraph.inCombat.OnEntry += OnInitialCombatEntry;
            StateGraph.inCombat.OnEntryToSameState += OnCombatEntry;
            StateGraph.inCombat.OnExit += OnCombatExit;
            StateGraph.notInCombat.OnEntry += OnNotInCombatEntry;
            StateGraph.dead.OnEntry += OnDeath;
            StateGraph.dead.OnExit += OnRevival;
        }

        public override void Freeze()
        {
            base.Freeze();
            animationControl.Freeze(true);
        }

        public override void Unfreeze()
        {
            base.Unfreeze();
            animationControl.Freeze(false);
        }

        public virtual void OnNewTick()
        {
            //NOTE: (just to save myself from thinking there's a "bug" again) it only resets basic cycle while IN COMBAT.
            //so if you are dead and cannot transition into combat, you will not be able to fire basic cycle (except maybe once if was already off cd)
            if (StateMachine.IsFrozen) return;
            if (StateMachine.HasState(typeof(InCombat)))
            {
                combatExitTimer++;
                weaponTickTimer++;

                if (stateDriver.EquippedWeapon != null 
                    && weaponTickTimer % stateDriver.EquippedWeapon.WeaponStats.BaseAttackRate == 0)
                {
                    OnWeaponTick?.Invoke();
                }
                if (combatExitTimer >= timeToLeaveCombat)
                {
                    StateMachine.SetCurrentState(StateGraph.notInCombat);
                }
            }
        }

        public void InstallWeaponAnimOverride()
        {
            if (stateDriver.EquippedWeapon != null)
            {
                animationControl.SetRuntimeAnimatorController(
                    ((Weapon)stateDriver.EquippedWeapon).AnimatorOverrideController);
            }
        }

        public void OnWeaponEquip()
        {
            if (StateMachine.HasState(typeof(InCombat)))
            {
                InstallWeaponAnimOverride();
            }

            ResetWeaponTickTimer();
        }

        protected void ResetWeaponTickTimer()
        {
            weaponTickTimer = -1;
        }

        protected void ResetCombatExitTimer()
        {
            combatExitTimer = 0;
        }

        protected virtual void OnInitialCombatEntry()
        {
            InstallWeaponAnimOverride();
            stateDriver.Health.Stat.autoReplenish = false;
            stateDriver.Wrath.autoReplenish = false;
            ResetWeaponTickTimer();
            ResetCombatExitTimer();
        }

        protected virtual void OnCombatEntry()
        {
            ResetCombatExitTimer();
        }

        protected virtual void OnCombatExit()
        {
            ResetWeaponTickTimer();
            ResetCombatExitTimer();
            animationControl.RevertAnimatorOverride();
        }

        public virtual void CeaseAttack()
        {
            animationControl.SetTrigger("ceaseAttack");
        }

        protected virtual void OnNotInCombatEntry()
        {
            stateDriver.Health.Stat.autoReplenish = true;
            stateDriver.Wrath.autoReplenish = true;
        }

        protected virtual void OnDeath()
        {
            stateDriver.Health.Stat.autoReplenish = false;
            animationControl.PlayAnimationState("Die", "Base Layer", 0);
            animationControl.ResetTrigger("revive");
        }

        protected virtual void OnRevival()
        {
            //animationControl.PlayAnimationState("Idle", "Base Layer", 0);
            animationControl.SetTrigger("revive");
        }

        public virtual void AnimateHealthChange(float damage)
        {
            if (damage > 0)
            {
                animationControl.PlayAnimationState("Take Damage", "Top Layer", 0);
            }
        }
    }
}