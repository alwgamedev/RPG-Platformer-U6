using System;
using System.Collections.Generic;
using System.Linq;
using RPGPlatformer.Core;
using RPGPlatformer.Effects;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static IProjectileAbility;

    public static class RangedAbilities
    {
        public enum RangedAbilitiesEnum
        {
            Pierce, PrecisionShot, Ensnare, Bombard, FesteringWound, MagicBullet, Ambush
        }

        public static AttackAbility GetAbility(string abilityName)
        {
            return GetAbility((RangedAbilitiesEnum)Enum.Parse(typeof(AttackAbility), abilityName));
        }

        public static AttackAbility GetAbility(RangedAbilitiesEnum ability)
        {
            return ability switch
            {
                RangedAbilitiesEnum.Pierce => Pierce,
                RangedAbilitiesEnum.PrecisionShot => PrecisionShot,
                RangedAbilitiesEnum.Ensnare => Ensnare,
                RangedAbilitiesEnum.Bombard => Bombard,
                RangedAbilitiesEnum.FesteringWound => FesteringWound,
                RangedAbilitiesEnum.MagicBullet => MagicBullet,
                RangedAbilitiesEnum.Ambush => Ambush,
                _ => null
            };
        }

        public static List<AbilityBarItem> DefaultAbilityBarItems()
        {
            return new()
            {
                new(PrecisionShot, false),
                new(Ensnare, false),
                new(Bombard, false),
                new(FesteringWound, false),
                new(MagicBullet, false),
                new(Ambush, false),
                new(Pierce, true)
            };
        }

        public static bool TryGetAbility(string abilityName, out AttackAbility ability)
        {
            ability = null;
            string formattedName = string.Concat(abilityName.Where(x => x != ' '));
            if (Enum.TryParse(typeof(RangedAbilitiesEnum), formattedName, out var obj))
            {
                ability = GetAbility((RangedAbilitiesEnum)obj);
                return true;
            }
            return false;
        }

        public static GrenadeLikeAbility Pierce = new()//basic
        {
            ObeyGCD = true,
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Basic Arrow"),
            CombatStyle = CombatStyle.Ranged,
            AnimationState = "Bow QuickShot",
            DisplayName = "Pierce",
            Cooldown = 0.48f,
            DamageMultiplier = 0.6f,
            StaminaFractionChange = -0.01f,
            WrathFractionChange = 0.05f
        };

        public static GrenadeLikeAbilityWithPowerUp PrecisionShot = new()//basic
        {
            CombatStyle = CombatStyle.Ranged,
            DisplayName = "Precision Shot",
            AnimationState = "Bow Shot",
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Basic Snipe Arrow"),
            HasPowerUpAnimation = true,
            UseActiveAimingWhilePoweringUp = true,
            HoldAimOnRelease = true,
            DamageMultiplier = 1.65f,
            Cooldown = 3,
            StaminaFractionChange = -0.08f,
            WrathFractionChange = 0.05f
        };

        public static GrenadeLikeAbility FesteringWound = new()//basic bleed
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Ranged,
            DisplayName = "Festering Wound",
            AnimationState = "Bow QuickShot",
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Basic Arrow"),
            GetHitAction = (ability) => GetHitActionBleedDamage(ability),
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Festering Wound Hit Effect"),
            DamageMultiplier = 0.8f,
            Cooldown = 8,
            StaminaFractionChange = -0.08f,
            WrathFractionChange = 0.08f,
            BleedCount = 5,
            BleedRate = 1200
        };

        public static GrenadeLikeAbility Ensnare = new()//basic stun
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Ranged,
            DisplayName = "Ensnare",
            AnimationState = "Bow QuickShot",
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Basic Arrow"),
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Rope Coil Effect"),
            DamageMultiplier = 1.35f,
            Cooldown = 8,
            StunDuration = 1.2f,
            StaminaFractionChange = .18f,//gain stamina, because you can catch your breath while opponent is stunned
            WrathFractionChange = 0.05f
        };

        public static GrenadeLikeAbilityWithPowerUp Bombard = new()//threshold AoE
        {
            CombatStyle = CombatStyle.Ranged,
            DisplayName = "Bombard",
            AnimationState = "Bow Shot",
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Exploding Arrow"),
            GetHitAction = (ability) => GetHitActionAoeDamage(ability, 2, true),
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Bombard Hit Effect"),
            HasPowerUpAnimation = true,
            UseActiveAimingWhilePoweringUp = true,
            HoldAimOnRelease = true,
            DamageMultiplier = 2.2f,
            Cooldown = 10,
            StaminaFractionChange = -0.18f,
            WrathFractionChange = -0.25f
        };

        public static GrenadeLikeAbilityWithPowerUp MagicBullet = new()//threshold (multi-hit projectile)
        {
            CombatStyle = CombatStyle.Ranged,
            DisplayName = "Magic Bullet",
            AnimationState = "Bow Shot",
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Basic Snipe Arrow"),
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Magic Bullet Hit Effect"),
            HasPowerUpAnimation = true,
            UseActiveAimingWhilePoweringUp = true,
            HoldAimOnRelease = true,
            DamageMultiplier = 2.25f,
            MaxHits = 4,
            Cooldown = 8,
            StaminaFractionChange = -0.18f,
            WrathFractionChange = -0.35f
        };

        public static GrenadeLikeAbilityWithPowerUp Ambush = new()//ultimate bleed
        {
            CombatStyle = CombatStyle.Ranged,
            DisplayName = "Ambush",
            AnimationState = "Bow Shot",
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Basic Snipe Arrow"),
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Ambush Hit Effect"),
            GetHitAction = (ability) => GetHitActionBleedDamage(ability, true),
            HasPowerUpAnimation = true,
            UseActiveAimingWhilePoweringUp = true,
            HoldAimOnRelease = true,
            DamageMultiplier = 1,
            TicksToAchieveMaxPower = 20,//24 ticks = 1.2sec (longer power up than Invoke, because it delivers damage faster)
            PowerGainRate = 40,//maximum power multiplier 1.5x
            Cooldown = 15,
            StaminaFractionChange = 0.25f,
            WrathFractionChange = -0.75f,
            BleedCount = 6,
            BleedRate = 500
        };


        public static IEnumerable<AttackAbility> AllAbilities = new List<AttackAbility>()
        {
            Pierce, PrecisionShot, Ensnare, Bombard, FesteringWound, MagicBullet, Ambush
        };
    }
}