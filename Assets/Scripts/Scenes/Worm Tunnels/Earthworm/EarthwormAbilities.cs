using System.Collections.Generic;
using RPGPlatformer.Combat;

namespace RPGPlatformer.AIControl
{
    public static class EarthwormAbilities
    {
        public static AbilityBar EarthwormAbilityBar(ICombatController cc)
        {
            var items = new List<AbilityBarItem>
            {
                new(EarthwormSlam, true),
                new(EarthwormStab, true)
            };
            return new(cc, items);
        }

        //worm's basic ability
        //override cc's find target to use nose position (or maybe that will make the combat range too OP)
        public static CloseRangeAbility EarthwormStab = new(DelayedAbilityExecutionOptions.DelayWithNoChannel)
        {
            Description = "Poke.",
            AbilityTags = new()
            {
                AbilityTag.AutoCastable
            },
            CanBeIncludedInAutoCastCycle = true,
            ObeyGCD = true,
            //AllowExecuteWithoutTarget = true,
            CombatStyle = CombatStyle.Unarmed,
            AnimationState = "Stab",
            Cooldown = 1.44f,
            StaminaFractionChange = 0,
            WrathFractionChange = 0.06f,//this way if slam costs .24 wrath, he'll be able to slam on every 5th hit
            DamageMultiplier = 1,
        }; 
        
        public static AoeAbilityThatExecutesImmediately EarthwormSlam 
            = new(DelayedAbilityExecutionOptions.DelayAndManuallyEndChannel)
        //thresh AoE Stun
        {
            Description = "SLAM!",
            AbilityTags = new()
            {
                AbilityTag.AoeDamage,
                AbilityTag.Stun,
                AbilityTag.AutoCastable
            },
            CanBeIncludedInAutoCastCycle = true,
            ObeyGCD = true,
            CombatStyle = CombatStyle.Unarmed,
            AnimationState = "Slam",
            //TO-DO: add hit effect (so you know if you've been hit and stunned)
            Cooldown = 6,
            DamageMultiplier = 6.5f,//TO-DO (balance the damage dealt so player is incentivized to dodge instead of tank)
            StaminaFractionChange = 0,
            WrathFractionChange = -.24f,
            AoeRadius = .75f,
            GetAoeCenter = (controller) =>
            {
                return ((EarthwormCombatController)controller).Nose.position;
            },
            ExcludeInstigator = true,
            StunDuration = 4
        };
    }
}