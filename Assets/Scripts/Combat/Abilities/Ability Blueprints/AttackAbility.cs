using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Effects;
using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    using static IProjectileAbility;

    public static class AbilityTag
    {
        public const string AoeDamage = "AoE Damage";
        public const string Bleed = "Bleed";
        public const string PowerUpAbility = "Power-Up Ability";
        public const string ProjectileAbility = "Projectile Ability";
        public const string Stun = "Stun";
        public const string AutoCastable = "Auto-Castable";

    }

    //public enum AbilityTag
    //{
    //    AutoCastable, AoE_Damage, Bleed, Power_Up_Ability, Projectile_Ability, Stun
    //}

    public class AttackAbility
    {
        public CombatStyle CombatStyle { get; init; }//should coincide with the animation layer name
        public string DisplayName { protected get; init; }
        //^USE GetAbilityName INSTEAD! (DisplayName sometimes left null)
        public string Description { get; init; }//mainly for description of ability in tooltips
        public List<string> AbilityTags { get; init; } = new();
        public string AnimationState { get; init; }//can leave null if no animation (will just get a warning in the editor)
        public Func<PoolableEffect> GetCombatantExecuteEffect { get; init; }
        public Func<PoolableEffect> GetHitEffect { get; init; }
        public bool CanBeIncludedInAutoCastCycle { get; init; }
        //public virtual bool IsAsyncAbility => false;//for convenience
        public bool ObeyGCD { get; init; }
        public bool UseActiveAimingWhilePoweringUp { get; init; }
        public bool HoldAimOnRelease { get; init; }
        public float Cooldown { get; init; }
        [Range(0, 1)] public float StaminaFractionChange { get; init; }
        [Range(0, 1)] public float WrathFractionChange { get; init; }
        public float DamageMultiplier { get; init; } = 1;
        //public int? StunDurationInMilliseconds { get; init; } = null;
        public float? StunDuration { get; init; } = null;
        public bool FreezeAnimationDuringStun { get; init; }
        //public bool ExecuteTriggeredInAnimation { get; init; }
        public int BleedCount { get; init; }
        //many different types of abilities (aoe, projectile, auto targeted) can have a bleed,
        //so it's easiest to just have every ability carry bleed stats (even though most don't need them).
        public float BleedRate { get; init; }//in seconds now that we're delaying game time
        public Func<int, float, float> DamagePerBleedIteration { get; init; } = (i, x) => x;
        public Action<ICombatController> OnExecute { get; init; }
        //OnExecute should be instructions for how the combat controller should execute the ability.
        //(Using Action instead of virtual method so that we can customize instance by instance)

        public string GetAbilityName()
        {
            return DisplayName ?? AnimationState;
        }

        public string GetTrimmedAbilityName()
        {
            return GetAbilityName().Replace(" ", "");
        }

        public virtual void Execute(ICombatController controller)
        {
            if (!CanBeExecuted(controller)) return;

            controller.Combatant.TriggerCombat();
            OnExecute?.Invoke(controller);
            controller.OnAbilityExecute(this);

            PoolableEffect effect = GetCombatantExecuteEffect?.Invoke();
            if (effect)
            {
                effect.PlayAtPosition(controller.Combatant.transform);
            }

            UpdateCombatantStats(controller.Combatant);
        }

        public float ComputeDamage(ICombatant combatant)
        {
            return DamageMultiplier * (combatant.AdditiveDamageBonus() 
                + combatant.EquippedWeapon.WeaponStats.BaseDamage);
        }


        //MANA AND WRATH CHECKS

        public bool CombatantHasSufficientStamina(ICombatant combatant)
        {
            return combatant.Stamina.FractionOfMax + StaminaFractionChange >= 0;
        }
        public bool CombatantHasSufficientWrath(ICombatant combatant)
        {
            return combatant.Wrath.FractionOfMax + WrathFractionChange >= 0;
        }

        public bool CanBeExecuted(ICombatController controller)
        {
            if (!CombatantHasSufficientStamina(controller.Combatant))
            {
                controller.OnInsufficientStamina();
                return false;
            }
            if (!CombatantHasSufficientWrath(controller.Combatant))
            {
                controller.OnInsufficientWrath();
                return false;
            }
            return true;
        }

        public void UpdateCombatantStamina(ICombatant combatant)
        {
            combatant.Stamina.AddValueClamped(StaminaFractionChange * combatant.Stamina.MaxValue);
        }
        public void UpdateCombatantWrath(ICombatant combatant)
        {
            combatant.Wrath.AddValueClamped(WrathFractionChange * combatant.Wrath.MaxValue);
        }

        public void UpdateCombatantStats(ICombatant combatant)
        {
            UpdateCombatantStamina(combatant);
            UpdateCombatantWrath(combatant);
        }


        //STATIC HELPER METHODS//


        //DEAL DAMAGE

        public static void DealDamage(IDamageDealer damageDealer, IHealth targetHealth, float damage, float? stunDuration = null, 
            bool freezeAnimationDuringStun = true, Func<PoolableEffect> getHitEffect = null)
        {
            if (targetHealth == null || targetHealth.IsDead) return;

            targetHealth.ReceiveDamage(damage, damageDealer);
            if (stunDuration.HasValue)
            {
                targetHealth.ReceiveStun(stunDuration.Value, freezeAnimationDuringStun);
            }

            PoolableEffect hitEffect = getHitEffect?.Invoke();
            if (hitEffect)
            {
                hitEffect.PlayAtPosition(targetHealth.HitEffectTransform);
            }
        }

        public static async Task Bleed(IDamageDealer damageDealer, IHealth targetHealth, float baseDamage, 
            int count, float rate, Func<int, float, float> damagePerBleedIteration = null, 
            Func<PoolableEffect> getHitEffect = null, bool useHitEffectOnlyOnFirstHit = false)
        {
            damagePerBleedIteration ??= (i, x) => x;

            void BleedHit(int j)
            {
                if (useHitEffectOnlyOnFirstHit && j > 0)
                {
                    DealDamage(damageDealer, targetHealth, damagePerBleedIteration(j, baseDamage));
                }
                else
                {
                    DealDamage(damageDealer, targetHealth, damagePerBleedIteration(j, baseDamage), 
                        null, false, getHitEffect);
                }
            }
            for (int i = 0; i < count; i++)
            {
                if (targetHealth == null || targetHealth.IsDead) break;
                BleedHit(i);
                if (i < count - 1)
                {
                    await MiscTools.DelayGameTime(rate, GlobalGameTools.Instance.TokenSource.Token);
                }
            }
        }


        //PROJECTILES

        public static void PrepareProjectile(ICombatController controller, IProjectile projectile, 
            Func<Vector2> getAimPos, float powerMultiplier, GetHitActionDelegate getHitAction, 
            DelayedAbilityExecutionOptions storeOptions, int maxHits = 1)
        {
            controller.Combatant.PrepareProjectile(projectile, getAimPos, powerMultiplier, 
                getHitAction(controller, projectile), maxHits);
            controller.StoreAction(controller.Combatant.ShootQueuedProjectile, 
                storeOptions.channelDuringDelay, storeOptions.endChannelOnExecute);
        }

        public static void PrepareProjectileWithStandardAiming(ICombatController controller, IProjectile projectile,
            float powerMultiplier, GetHitActionDelegate getHitAction, 
            DelayedAbilityExecutionOptions storeOptions, int maxHits = 1)
        {
            Func<Vector2> getAimPos = () => controller.GetAimPosition();
            PrepareProjectile(controller, projectile, getAimPos, powerMultiplier, getHitAction, 
                storeOptions, maxHits);
        }
    }
}