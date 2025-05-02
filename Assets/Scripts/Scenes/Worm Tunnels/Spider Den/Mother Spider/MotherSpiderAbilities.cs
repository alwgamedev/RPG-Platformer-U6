using System.Collections.Generic;
using RPGPlatformer.Core;
using RPGPlatformer.Effects;

namespace RPGPlatformer.Combat
{
    public class MotherSpiderAbilities
    {
        public static AbilityBar MotherSpiderUnarmedAbilityBar(ICombatController cc)
        {
            var abilities = new List<AbilityBarItem>()
            {
                new(MotherSpiderSlap, true)
            };

            return new(cc, abilities);
        }

        public static AbilityBar MotherSpiderRangedAbilityBar(ICombatController cc)
        {
            var abilities = new List<AbilityBarItem>()
            {
                new(RangedAbilities.Pierce, true)
            };

            return new(cc, abilities);
        }

        public static CloseRangeAbility MotherSpiderSlap 
            = new(DelayedAbilityExecutionOptions.DelayAndManuallyEndChannel)
        {
            //Description = "A real fighter uses their fists.",
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
    }
}