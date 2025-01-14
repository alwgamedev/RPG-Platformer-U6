using RPGPlatformer.Core;
using RPGPlatformer.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;

namespace RPGPlatformer.Combat
{
    using static AutoTargetedAbility;

    public static class MeleeAbilities
    {

        public enum MeleeAbilitiesEnum
        {
            Jab, Slice, Swipe, Uppercut, Slash, Thrust, Slam, Ravage
        }

        public static AttackAbility GetAbility(string abilityName)
        {
            return GetAbility((MeleeAbilitiesEnum)Enum.Parse(typeof(AttackAbility), abilityName));
        }

        public static AttackAbility GetAbility(MeleeAbilitiesEnum ability)
        {
            return ability switch
            {
                MeleeAbilitiesEnum.Jab => Jab,
                MeleeAbilitiesEnum.Slice => Slice,
                MeleeAbilitiesEnum.Swipe => Swipe,
                MeleeAbilitiesEnum.Uppercut => Uppercut,
                MeleeAbilitiesEnum.Slash => Slash,
                MeleeAbilitiesEnum.Thrust => Thrust,
                MeleeAbilitiesEnum.Slam => Slam,
                MeleeAbilitiesEnum.Ravage => Ravage,
                _ => null
            };
        }

        public static bool TryGetAbility(string abilityName, out AttackAbility ability)
        {
            ability = null; 
            string formattedName = string.Concat(abilityName.Where(x => x != ' '));
            if (Enum.TryParse(typeof(MeleeAbilitiesEnum), formattedName, out var obj))
            {
                ability = GetAbility((MeleeAbilitiesEnum)obj);
                return true;
            }
            return false;
        }

        public static List<AbilityBarItem> DefaultAbilityBarItems()
        {
            return new()
            {
                new(Jab, false),
                new(Slice, false),
                new(Thrust, false),   
                new(Slam, false),
                new(Ravage, false),
                new(Swipe, true),
                new(Slash, true),
                new(Uppercut, true)

            };
        }

        public static AutoTargetedBleed Swipe = new()//basic bleed
        {
            ObeyGCD = true,
            AnimationState = "Swipe",
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Swipe Hit Effect"),
            DamageMultiplier = 0.8f,
            BleedCount = 5,
            BleedRate = 1200,
            AutoTarget = (controller) => TargetInFront(controller.Combatant),
            CombatStyle = CombatStyle.Melee,
            Cooldown = 8
        };

        public static CloseRangeAbility Slash = new()//basic
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Melee,
            AnimationState = "Slash",
            Cooldown = 1.44f,
            StaminaFractionChange = -.03f,
            WrathFractionChange = 0.05f,
            DamageMultiplier = 1.35f,
        };

        public static CloseRangeAbility Uppercut = new()//basic
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Melee,
            AnimationState = "Uppercut",
            Cooldown = 1.44f,
            StaminaFractionChange = -0.05f,
            WrathFractionChange = 0.06f,
            DamageMultiplier = 1.5f,

        };

        public static CloseRangeAbility Jab = new()//basic stun
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Melee,
            AnimationState = "Jab",
            GetHitEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Rope Coil Effect"),
            Cooldown = 8,
            StaminaFractionChange = .18f,
            WrathFractionChange = 0.05f,
            DamageMultiplier = 1.1f,
            StunDuration = 1.2f

        };

        public static AutoTargetedBleed Slice = new()//thresh bleed
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Melee,
            AnimationState = "Slice",
            //GetHitEffect = () => (PoolableEffect)GlobalGameTools.EffectPooler.GetObject("Slice Hit Effect"),
            //HIT EFFECT NOT IN USE (too much visual noise)
            Cooldown = 12,
            DamageMultiplier = 1.05f,
            BleedCount = 4,
            BleedRate = 1000,
            AutoTarget = (controller) => TargetInFront(controller.Combatant),
            StaminaFractionChange = -.18f,
            WrathFractionChange = -.25f
        };

        public static CloseRangeAbility Thrust = new()//thresh
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Melee,
            AnimationState = "Thrust",
            Cooldown = 8,
            DamageMultiplier = 4.5f,
            StaminaFractionChange = -.18f,
            WrathFractionChange = -.35f
        };

        public static AoeAbilityThatExecutesImmediately Slam = new(true)//thresh AoE Stun (should be comparable to Desecrate, with slightly higher damage + stun)
        {
            ObeyGCD = true,
            CombatStyle = CombatStyle.Melee,
            AnimationState = "Slam",
            GetCombatantExecuteEffect = () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Dust Cloud Effect"),
            Cooldown = 12,
            DamageMultiplier = 4.2f,
            StaminaFractionChange = -.25f,
            WrathFractionChange = -.25f,
            AoeRadius = 1.5f,
            GetAoeCenter = (controller) => controller.Combatant.Transform.position,
            ExcludeInstigator = true,
            StunDuration = 2.5f,
            FreezeAnimationDuringStun = false
        };

        public static AoePowerUpAbility Ravage = new()//ultimate AoE
        {
            CombatStyle = CombatStyle.Melee,
            AnimationState = "Ravage",
            HasPowerUpAnimation = true,
            GetCombatantExecuteEffect = 
                () => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Ravage Execute Effect"),
            TicksToAchieveMaxPower = 18,//18 ticks = 1.08s
            PowerGainRate = 12,//max power multiplier 2.5x
            Cooldown = 15,
            DamageMultiplier = 3,
            StaminaFractionChange = .25f,
            WrathFractionChange = -.75f,
            AoeRadius = 1.5f,
            GetAoeCenter = (controller) => controller.Combatant.Transform.position,
            ExcludeInstigator = true
        };


        public static IEnumerable<AttackAbility> AllAbilities = new List<AttackAbility>()
        {
            Jab, Slice, Swipe, Uppercut, Slash, Thrust, Slam, Ravage
        };
    }
}