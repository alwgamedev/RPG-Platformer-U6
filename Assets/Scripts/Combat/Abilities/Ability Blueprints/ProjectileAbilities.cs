using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static IProjectileAbility;
    using static AttackAbility;
    using static IAoeAbility;

    public interface IProjectileAbility
    {
        public int MaxHits { get; }
        public Func<IProjectile> GetProjectile { get; }
        public Func<AttackAbility, GetHitActionDelegate> GetHitAction { get; }

        public delegate Action<Collider2D> GetHitActionDelegate(ICombatController controller, IProjectile projectile);

        public static GetHitActionDelegate GetHitActionSingleDamage(AttackAbility ability)
        {
            return (controller, projectile) => (collider) =>
            {
                if(collider.gameObject.TryGetComponent(out IHealth colliderHealth))
                {
                    DealDamage(controller.Combatant, colliderHealth, 
                        ability.ComputeDamage(controller.Combatant) * projectile.PowerMultiplier, 
                        ability.StunDuration, ability.FreezeAnimationDuringStun, ability.GetHitEffect);
                }
                //NOTE: power multiplier is passed on to the projectile, because it needs a place to be stored between end of PowerUp and
                //time that animation triggers projectile.Shoot
            };
        }

        public static GetHitActionDelegate GetHitActionAoeDamage(AttackAbility ability, float aoeRadius, 
            bool excludeInstigator)
        {
            return (controller, projectile) => (collider) =>
            {
                ExecuteAoeAbility(null, c => projectile.transform.position, aoeRadius,
                    ability.ComputeDamage(controller.Combatant) * projectile.PowerMultiplier, 
                    excludeInstigator, controller.Combatant,
                    ability.StunDuration, ability.FreezeAnimationDuringStun, ability.GetHitEffect);
                //NOTE: power multiplier is passed on to the projectile, because it needs a place to be stored between end of PowerUp and
                //time that animation triggers projectile.Shoot
            };
        }

        public static GetHitActionDelegate GetHitActionBleedDamage(AttackAbility ability, 
            bool useHitEffectOnlyOnFirstHit = false)
        {
            return (controller, projectile) => async (collider) =>
            {
                if (collider.gameObject.TryGetComponent(out Health colliderHealth))
                {
                    await Bleed(controller.Combatant, colliderHealth, 
                        ability.ComputeDamage(controller.Combatant) * projectile.PowerMultiplier,
                        ability.BleedCount, ability.BleedRate, ability.DamagePerBleedIteration, 
                        ability.GetHitEffect, useHitEffectOnlyOnFirstHit);
                }
            };
        }
    }

    //Description: shoot projectile immediately
    //Instance needs to fill in:
    //(*) ProjectilePrefab
    //(*) base AttackAbility stats
    public class GunLikeAbility : AttackAbility, IProjectileAbility//shoot projectile instantly
    {
        public int MaxHits { get; init; } = 1;
        public Func<IProjectile> GetProjectile { get; init; }
        public Func<AttackAbility, GetHitActionDelegate> GetHitAction { get; set; } = GetHitActionSingleDamage;

        //making this settable because I want to provide a non-static default value in the constructor,
        //but I also may want to change it per instance without having to create a new class

        public GunLikeAbility()
        {
            OnExecute = (controller) => 
                PrepareProjectileWithStandardAiming(controller, GetProjectile?.Invoke(), 1, GetHitAction(this), MaxHits);
        }
    }

    //Description: shoot projectile on next fire button down (with cancellation detection, like all the async abilities)
    //Instance needs to fill in:
    //(*) ProjectilePrefab
    //(*) base AttackAbility stats
    public class GrenadeLikeAbility : AbilityThatGetsDataOnNextFireButtonDownAndExecutesImmediately<object>, IProjectileAbility
    {
        public int MaxHits { get; init; } = 1;
        public Func<IProjectile> GetProjectile { get; init; }
        public Func<AttackAbility, GetHitActionDelegate> GetHitAction { get; set; } = GetHitActionSingleDamage;

        public GrenadeLikeAbility() : base()
        {
            GetData = (controller) => null;//(controller) => controller.GetAimPosition();
            OnExecute = (controller, args) => 
                PrepareProjectileWithStandardAiming(controller, GetProjectile?.Invoke(), 1, GetHitAction(this), MaxHits);
        }
    }

    //Description: PowerUpAbility that powers up while fire button is held down and gets aim position on release (what it does OnExecute is left unspecified)
    //Instance needs to fill in:
    //(*) TicksToAchieveMaxPower and PowerGainRate
    //(*) base AttackAbility stats
    //(*) OnExecute = Action<(Vector2, int)>
    public class AimedPowerUpAbility : PowerUpAbility<object>
    {
        public AimedPowerUpAbility() : base()
        {
            GetData = controller => null;
        }
    }

    //Description: hold fire button down to build power then shoot a projectile on release
    //Instance needs to fill in:
    //(*) ProjectilePrefab
    //(*) base AttackAbility stats
    public class GrenadeLikeAbilityWithPowerUp : AimedPowerUpAbility, IProjectileAbility
    {
        public int MaxHits { get; init; } = 1;
        public Func<IProjectile> GetProjectile { get; init; }
        public Func<AttackAbility, GetHitActionDelegate> GetHitAction { get; set; } = GetHitActionSingleDamage;

        public GrenadeLikeAbilityWithPowerUp() : base()
        {
            TicksToAchieveMaxPower = 15;//15 ticks = 0.9 seconds
            PowerGainRate = 20;//maximum power multiplier 1.75x
            OnExecute = (controller, args) =>
            {
                float powerMultiplier = ComputePowerMultiplier(args.Item2);
                PrepareProjectileWithStandardAiming(controller, GetProjectile?.Invoke(), powerMultiplier, GetHitAction(this), MaxHits);
            };
        }
    }
}