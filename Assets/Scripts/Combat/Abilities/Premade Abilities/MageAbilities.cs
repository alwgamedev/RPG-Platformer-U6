
using System;
using System.Collections.Generic;
using System.Linq;
using RPGPlatformer.Core;
using RPGPlatformer.Effects;

namespace RPGPlatformer.Combat
{
    using static AutoTargetedAbility;
    using static IProjectileAbility;

    public static class MageAbilities
    {
        //it is useful to have the enum rather than just a Dictionary<string, AttackAbility>
        //so that we can have the correct abilities appear in an inspector drop down menu e.g.
        //for SOs
        public enum MageAbilitiesEnum
        {
            Blast, Afflict, Daze, Demolish, Captivate, Desecrate, Invoke
        }

        public static AttackAbility GetAbility(string abilityName)
        {
            return GetAbility((MageAbilitiesEnum)Enum.Parse(typeof(AttackAbility), abilityName));
        }

        public static AttackAbility GetAbility(MageAbilitiesEnum ability)
        {
            return ability switch
            {
                MageAbilitiesEnum.Blast => Blast,
                MageAbilitiesEnum.Afflict => Afflict,
                MageAbilitiesEnum.Daze => Daze,
                MageAbilitiesEnum.Demolish => Demolish,
                MageAbilitiesEnum.Captivate => Captivate,
                MageAbilitiesEnum.Desecrate => Desecrate,
                MageAbilitiesEnum.Invoke => Invoke,
                _ => null
            };
        }

        public static bool TryGetAbility(string abilityName, out AttackAbility ability)
        {
            ability = null;
            string formattedName = string.Concat(abilityName.Where(x => x != ' '));
            if (Enum.TryParse(typeof(MageAbilitiesEnum), formattedName, out var obj))
            {
                ability = GetAbility((MageAbilitiesEnum)obj);
                return true;
            }
            return false;
        }

        public static List<AbilityBarItem> DefaultAbilityBarItems()
        {
            return new()
            {
                new(Afflict, false),
                new(Daze, false),
                new(Demolish, false),
                new(Captivate, false),
                new(Desecrate, false),
                new(Invoke, false),
                new(Blast, true)
            };

        }

        public static GrenadeLikeAbility Blast = new()//basic
        {
            Description = "Every wizard's first spell, a rapid-fire projectile ability.",
            AbilityTags = new()
            {
                AbilityTag.ProjectileAbility,
                AbilityTag.AutoCastable
            },
            CanBeIncludedInAutoCastCycle = true,
            DelayedReleaseOfChannel = false,
            ObeyGCD = true,
            GetProjectile = () => 
                (Projectile)GlobalGameTools.Instance.ProjectilePooler.ReleaseObject("Green Trail Projectile"),
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Blast",
            Cooldown = 0.3f,
            DamageMultiplier = 0.45f,
            StaminaFractionChange = -0.01f,
            WrathFractionChange = 0.04f
        };

        public static AutoTargetOnNextFireButtonDownBleed Afflict = new(DelayedAbilityExecutionOptions.NoDelay)
        //basic bleed
        {
            Description = "Afflict your target with a persistent bleed.",
            AbilityTags = new()
            {
                AbilityTag.Bleed
            },
            ObeyGCD = true,
            CombatStyle = CombatStyle.Mage,
            DisplayName = "Afflict",
            AnimationState = "Captivate",
            GetHitEffect = () => 
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.ReleaseObject("Afflict Hit Effect"),
            AutoTarget = TargetAimPosition,
            DamageMultiplier = 0.8f,
            Cooldown = 8,
            StaminaFractionChange = -0.08f,
            WrathFractionChange = 0.08f,
            BleedCount = 5,
            BleedRate = 1.2f
        };

        public static AutoTargetOnNextFireButtonDownSingleDamage Daze = new(DelayedAbilityExecutionOptions.NoDelay)
        //basic stun
        {
            Description = "Briefly stun your opponent and benefit from the respite by gaining stamina.",
            AbilityTags = new()
            {
                AbilityTag.Stun
            },
            ObeyGCD = true,
            CombatStyle = CombatStyle.Mage,
            DisplayName = "Daze",
            AnimationState = "Blast",
            GetHitEffect = () => 
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.ReleaseObject("Rope Coil Effect"),
            AutoTarget = TargetAimPosition,
            DamageMultiplier = 1.35f,
            Cooldown = 8,
            StunDuration = 1.2f,
            StaminaFractionChange = .18f,//gain stamina, because you can catch your breath while opponent is stunned
            WrathFractionChange = 0.05f
            //FreezeAnimationDuringStun = false
        };

        public static GrenadeLikeAbilityWithPowerUp Demolish = new()//threshold AoE
        {
            //NOTE: this class has default power-up stats (see class)
            Description = "Demolish your opponents with a bigly large wrecking ball that explodes upon impact.",
            AbilityTags = new()
            {
                AbilityTag.AoeDamage,
                AbilityTag.PowerUpAbility,
                AbilityTag.ProjectileAbility
            },
            GetProjectile = () => 
                (Projectile)GlobalGameTools.Instance.ProjectilePooler.ReleaseObject("Grenade Projectile"),
            GetHitAction = (ability) => GetHitActionAoeDamage(ability, 1.5f, false),
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Demolish",
            HasChannelAnimation = true,
            Cooldown = 0.5f,
            DamageMultiplier = 1.5f,
            StaminaFractionChange = -.18f,
            WrathFractionChange = -0.25f
        };

        public static AoeAbilityThatExecutesOnNextFireButtonDown Captivate 
            = new(DelayedAbilityExecutionOptions.DelayAndEndChannelOnExecute)//threshold AoE stun
        {
            Description = "Call on the winter spirits to cast an icy freeze over your opponents.",
            AbilityTags = new()
            {
                AbilityTag.AoeDamage,
                AbilityTag.Stun
            },
            ObeyGCD = true,
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Captivate",
            GetHitEffect = () => 
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.ReleaseObject("Captivate Hit Effect"),
            Cooldown = 12,
            DamageMultiplier = 1.5f,
            StaminaFractionChange = 0.18f,//gain stamina, because you can catch your breath while your opponents are stunned
            WrathFractionChange = -.15f,
            StunDuration = 2.5f,
            GetData = (controller) => controller.GetAimPosition(),
            AoeRadius = 1,
            FreezeAnimationDuringStun = true
        };

        //hits only slightly higher than max power Demolish, but fires instantly and has better AoE, hence the steeper Stamina and Wrath costs
        //on the other hand, only targets close range (no projectile)
        public static AoeAbilityThatExecutesImmediately Desecrate 
            = new(DelayedAbilityExecutionOptions.DelayAndEndChannelOnExecute)//threshold AoE
        {
            Description = "Desecrate the earth by slamming down your staff, " +
            "dealing heavy damage to any nearby enemies.",
            AbilityTags = new()
            {
                AbilityTag.AoeDamage,
                AbilityTag.AutoCastable
            },
            CanBeIncludedInAutoCastCycle = true,
            ObeyGCD = true,
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Desecrate",
            GetCombatantExecuteEffect = () => 
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.ReleaseObject("Desecrate Execute Effect"),
            Cooldown = 12,
            DamageMultiplier = 3.8f,
            StaminaFractionChange = -.25f,
            WrathFractionChange = -.25f,
            GetAoeCenter = (controller) => controller.Combatant.transform.position,
            AoeRadius = 1.5f,
        };

        public static AutoTargetedPowerUpBleed Invoke 
            = new(DelayedAbilityExecutionOptions.DelayAndEndChannelOnExecute)//ultimate bleed
        {
            Description = "Invoke your opponent's inner malice and use it against them.",
            AbilityTags = new()
            {
                AbilityTag.PowerUpAbility,
                AbilityTag.Bleed
            },
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Invoke",
            HasPowerUpAnimation = true,
            GetCombatantPowerUpEffect = () => 
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.ReleaseObject("Invoke PowerUp Effect"),
            GetHitEffect = () =>
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.ReleaseObject("Invoke Hit Effect"),
            Cooldown = 15,
            DamageMultiplier = 1,
            StaminaFractionChange = 0.25f,
            //Gain stamina! (You are stealing it from your enemy while channeling)
            //idea is that you will use basics to build up to this so will have lots of wrath but low stamina
            //when you cast this, hence will need the stamina
            WrathFractionChange = -0.75f,
            GetDataOnPowerUpStarted = true,
            TicksToAchieveMaxPower = 16,//16 ticks = 0.96 seconds
            PowerGainRate = 12,//maximum power multiplier of 2.5x
            AutoTarget = TargetAimPosition,
            BleedCount = 6,
            BleedRate = 1.2f,
            DamagePerBleedIteration = (iteration, baseDamage) =>
            {
                return iteration switch
                {
                    0 => baseDamage * 2,
                    1 => baseDamage * 1,
                    _ => baseDamage * 0.6f,
                };
            }
        };

        public static IEnumerable<AttackAbility> AllAbilities = new List<AttackAbility>()
        {
            Blast, Afflict, Daze, Demolish, Captivate, Desecrate, Invoke
        };
    }
}