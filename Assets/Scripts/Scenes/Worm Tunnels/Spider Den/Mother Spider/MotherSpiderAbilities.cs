using System.Collections.Generic;
using RPGPlatformer.Core;
using RPGPlatformer.Effects;
using RPGPlatformer.UI;

namespace RPGPlatformer.Combat
{
    public class MotherSpiderAbilities
    {
        public static AbilityBar MotherSpiderUnarmedAbilityBar(ICombatController cc)
        {
            var abilities = new List<AbilityBarItem>()
            {
                new(MotherSpiderBite, true),
                new(MotherSpiderSlap, true)
            };

            return new(cc, abilities);
        }

        public static AbilityBar MotherSpiderRangedAbilityBar(ICombatController cc)
        {
            var abilities = new List<AbilityBarItem>()
            {
                new(MotherSpiderSpit, true)
            };

            return new(cc, abilities);
        }

        public static CloseRangeAbility MotherSpiderSlap 
            = new(DelayedAbilityExecutionOptions.DelayAndManuallyEndChannel)
        {
            AbilityTags = new()
            {
                AbilityTag.AutoCastable
            },
            CanBeIncludedInAutoCastCycle = true,
            ObeyGCD = true,
            AllowExecuteWithoutTarget = true,
            CombatStyle = CombatStyle.Unarmed,
            AnimationState = "Slap",
            GetHitEffect = ()
                => (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Punch Hit Effect"),
            Cooldown = 0.5f,
            StaminaFractionChange = 0,
            WrathFractionChange = 0.05f
        };

        public static AutoTargetedBleed MotherSpiderBite 
            = new(DelayedAbilityExecutionOptions.DelayAndManuallyEndChannel)//thresh bleed
        {
            AbilityTags = new()
            {
                AbilityTag.Bleed,
                AbilityTag.AutoCastable
            },
            CanBeIncludedInAutoCastCycle = true,
            AllowExecuteWithoutTarget = true,
            ObeyGCD = true,
            AnimationState = "Bite",
            GetHitEffect = () =>
                (PoolableEffect)GlobalGameTools.Instance.EffectPooler.GetObject("Festering Wound Hit Effect"),
            DamageMultiplier = 1,
            DamagePerBleedIteration = (i, d) => i == 0 ? 1.75f * d : d,
            BleedCount = 6,
            BleedRate = 1.4f,
            AutoTarget = (controller) =>
            {
                var h = AutoTargetedAbility.TargetInFront(controller.Combatant);
                if (h == GlobalGameTools.Instance.Player.Combatant.Health && !h.IsDead)
                {
                    GameLog.Log("You've been poisoned by the spider's bite!");
                }
                return h;
            },
            CombatStyle = CombatStyle.Unarmed,
            Cooldown = 8,
            StaminaFractionChange = 0,
            WrathFractionChange = -0.3f
        };

        public static GrenadeLikeAbility MotherSpiderSpit = new()//ranged attack
        {
            AbilityTags = new()
            {
                AbilityTag.ProjectileAbility,
                AbilityTag.AutoCastable
            },
            CanBeIncludedInAutoCastCycle = true,
            DelayedReleaseOfChannel = false,
            ObeyGCD = true,
            GetProjectile = () =>
                {
                    //temporary just while testing mother spider in other scenes.
                    Projectile p = null;
                    if (SceneCombatResources.Instance && SceneCombatResources.Instance.ProjectilePooler)
                    {
                        p = (Projectile)SceneCombatResources.Instance.ProjectilePooler.GetObject("Mother Spider Spit Projectile");
                    }
                    if (!p)
                    {
                        p = (Projectile)GlobalGameTools.Instance.ProjectilePooler.GetObject("Basic Arrow");
                    }
                    return p;
                },
                //so that we don't spawn 50 mother spider projectiles in every scene where we don't need them,
                //let's have a (singleton but non-persistent) "SceneTools" which can hold references to 
                //effect and projectile pools that are specific to characters in that scene.
                //we can use the basic arrow as a fall back (just so we have a projectile to use if
                //testing in a different scene)
            CombatStyle = CombatStyle.Ranged,
            AnimationState = "Spit",
            Cooldown = 0.48f,
            DamageMultiplier = 1,
            StaminaFractionChange = 0,
            WrathFractionChange = 0.05f
        };
    }
}