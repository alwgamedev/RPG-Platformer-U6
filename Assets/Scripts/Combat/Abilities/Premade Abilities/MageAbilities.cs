using System.Collections.Generic;
using RPGPlatformer.Core;
using RPGPlatformer.Effects;

namespace RPGPlatformer.Combat
{
    using static AttackAbility;
    using static AutoTargetedAbility;
    using static CombatStyles;
    using static IProjectileAbility;

    public static class MageAbilities
    {
        public enum MageAbilitiesEnum
        {
            Blast, Afflict, Daze, Demolish, Captivate, Desecrate, Invoke
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

        public static List<AbilityBarItem> DefaultAbilityBarData()
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
                AbilityTag.Projectile_Ability
            },
            ObeyGCD = true,
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Green Trail Projectile"),
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Blast",
            Cooldown = 0.3f,
            DamageMultiplier = 0.45f,
            StaminaFractionChange = -0.01f,
            WrathFractionChange = 0.04f
        };

        public static AutoTargetOnNextFireButtonDownBleed Afflict = new()//basic bleed
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
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Afflict Hit Effect"),
            AutoTarget = TargetAimPosition,
            DamageMultiplier = 0.8f,
            Cooldown = 8,
            StaminaFractionChange = -0.08f,
            WrathFractionChange = 0.08f,
            BleedCount = 5,
            BleedRate = 1200
        };

        public static AutoTargetOnNextFireButtonDownSingleDamage Daze = new()//basic stun
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
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Rope Coil Effect"),
            AutoTarget = TargetAimPosition,
            DamageMultiplier = 1.35f,
            Cooldown = 8,
            StunDuration = 1.2f,
            StaminaFractionChange = .18f,//gain stamina, because you can catch your breath while opponent is stunned
            WrathFractionChange = 0.05f
        };

        public static GrenadeLikeAbilityWithPowerUp Demolish = new()//threshold AoE
        {
            //NOTE: this class has default power-up stats (see class)
            Description = "Demolish your opponents with a bigly large wrecking ball that explodes upon impact.",
            AbilityTags = new()
            {
                AbilityTag.AoE_Damage,
                AbilityTag.Power_Up_Ability,
                AbilityTag.Projectile_Ability
            },
            GetProjectile = () => (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Grenade Projectile"),
            GetHitAction = (ability) => GetHitActionAoeDamage(ability, 2.5f, false),
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Demolish",
            HasChannelAnimation = true,
            Cooldown = 0.5f,
            DamageMultiplier = 1.5f,
            StaminaFractionChange = -.18f,
            WrathFractionChange = -0.25f
        };

        public static AoeAbilityThatExecutesOnNextFireButtonDown Captivate = new()//threshold AoE stun
        {
            Description = "Summon the winter spirits to cast an icy freeze over your opponents.",
            AbilityTags = new()
            {
                AbilityTag.AoE_Damage,
                AbilityTag.Stun
            },
            ObeyGCD = true,
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Captivate",
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Captivate Hit Effect"),
            Cooldown = 12,
            DamageMultiplier = 1.5f,
            StaminaFractionChange = 0.18f,//gain stamina, because you can catch your breath while your opponents are stunned
            WrathFractionChange = -.15f,
            StunDuration = 2.5f,
            GetData = (controller) => controller.GetAimPosition(),
            AoeRadius = 1.5f
        };

        //hits only slightly higher than max power Demolish, but fires instantly and has better AoE, hence the steeper Stamina and Wrath costs
        //on the other hand, only targets close range (no projectile)
        public static AoeAbilityThatExecutesImmediately Desecrate = new(true)//threshold AoE
        {
            Description = "Desecrate the earth by slamming down your staff, " +
            "dealing heavy damage to any nearby enemies.",
            AbilityTags = new()
            {
                AbilityTag.AoE_Damage
            },
            //ExecuteTriggeredInAnimation = true,
            ObeyGCD = true,
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Desecrate",
            GetCombatantExecuteEffect = () => 
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Desecrate Execute Effect"),
            Cooldown = 12,
            DamageMultiplier = 3.8f,
            StaminaFractionChange = -.25f,
            WrathFractionChange = -.25f,
            GetAoeCenter = (controller) => controller.Combatant.Transform.position,
            AoeRadius = 2f,
        };

        public static AutoTargetedPowerUpBleed Invoke = new()//ultimate bleed
        {
            Description = "Invoke your opponent's inner malice and use it against them.",
            AbilityTags = new()
            {
                AbilityTag.Power_Up_Ability,
                AbilityTag.Bleed
            },
            CombatStyle = CombatStyle.Mage,
            AnimationState = "Invoke",
            HasPowerUpAnimation = true,
            GetCombatantPowerUpEffect = () => 
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Invoke PowerUp Effect"),
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
            BleedRate = 1200,
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
    }
}