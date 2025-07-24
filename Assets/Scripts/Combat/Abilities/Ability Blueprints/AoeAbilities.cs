using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using RPGPlatformer.Effects;

namespace RPGPlatformer.Combat
{
    using static IAoeAbility;
    using static AttackAbility;
    using static Health;

    public interface IAoeAbility
    {
        public float AoeRadius { get; }
        public bool ExcludeInstigator { get; }

        public Func<ICombatController, Vector2> GetAoeCenter { get; }

        public static IEnumerable FindTargetsAtPosition(Vector2 position, float radius)
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(position, 2 * radius * Vector2.right + 2 * radius * Vector2.up, 0);
            return hitColliders.Select(x => GetHealthComponent(x)).Distinct();
        }

        //NOTE: the base DealDamage method already checks for null
        public static void DealAoeDamage(IDamageDealer damageDealer, IEnumerable targets, float damage, bool excludeInstigator,
            float? stunDuration = null, bool freezeAnimationDuringStun = true, Func<PoolableEffect> getHitEffect = null)
        {
            foreach (IHealth health in targets)
            {
                if (health == null || (excludeInstigator && health.transform == damageDealer.transform))
                {
                    continue;
                }
                DealDamage(damageDealer, health, damage, stunDuration, freezeAnimationDuringStun, getHitEffect);
            }
        }

        public static void ExecuteAoeAbility(ICombatController controller, 
            Func<ICombatController, Vector2> getAoeCenter, float aoeRadius, float damage, 
            bool excludeInstigator, IDamageDealer damageDealer, float? stunDuration = null, 
            bool freezeAnimationDuringStun = true, Func<PoolableEffect> getHitEffect = null)
        {
            if (getAoeCenter == null) return;
            IEnumerable targets = FindTargetsAtPosition(getAoeCenter(controller), aoeRadius);
            DealAoeDamage(damageDealer, targets, damage, excludeInstigator, stunDuration, 
                freezeAnimationDuringStun, getHitEffect);
        }
    }

    //Instance needs to fill in: 
    //(*) GetAoeCenter (Func<ICombatController, Vector2>) -- where the aoe hit should be centered (e.g. aim position or combatant's position)
    //(*) AoeRadius (float)
    //(*) ExcludeInstigator (bool -- whether the attacker should be hit by the AoE)
    //(*) base AttackAbility stats
    public class AoeAbilityThatExecutesImmediately : AttackAbility, IAoeAbility
    {
        public float AoeRadius { get; init; } = 2;
        public bool ExcludeInstigator { get; init; } = true;

        public Func<ICombatController, Vector2> GetAoeCenter { get; init; }

        public AoeAbilityThatExecutesImmediately(DelayedAbilityExecutionOptions delayOptions) : base(delayOptions)
        {
            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller) => controller.StoreAction(() =>
                {
                    //Vector2 aoeCenter = GetAoeCenter(controller);
                    ExecuteAoeAbility(controller, GetAoeCenter, AoeRadius,
                        ComputeDamage(controller.Combatant), ExcludeInstigator, controller.Combatant,
                        StunDuration, FreezeAnimationDuringStun, GetHitEffect);
                }, delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = (controller) => ExecuteAoeAbility(controller, GetAoeCenter, AoeRadius,
                    ComputeDamage(controller.Combatant), ExcludeInstigator, controller.Combatant,
                    StunDuration, FreezeAnimationDuringStun, GetHitEffect);
            }
        }
    }

    //Instance needs to fill in: 
    //(*) GetData (Func<ICombatController, Vector2>) -- where the aoe hit should be centered (e.g. aim position or combatant's position)
    //(*) AoeRadius (float)
    //(*) ExcludeInstigator (bool)
    //(*) base AttackAbility stats
    //(*) whether it HasChannelAnimation (bool)
    public class AoeAbilityThatExecutesOnNextFireButtonDown 
        : AbilityThatGetsDataOnNextFireButtonDownAndExecutesImmediately<Vector2>, IAoeAbility
    {
        public float AoeRadius { get; init; } = 2;
        public bool ExcludeInstigator { get; init; } = true;

        public Func<ICombatController, Vector2> GetAoeCenter
        {
            get => GetData;
            init
            {
                GetData = value;
            }
        }

        public AoeAbilityThatExecutesOnNextFireButtonDown(DelayedAbilityExecutionOptions delayOptions) 
            : base(delayOptions)
        {
            //OnExecute = (controller, position) => ExecuteAoeAbility(null, c => position, AoeRadius,
            //    ComputeDamage(controller.Combatant), ExcludeInstigator, controller.Combatant,
            //    StunDuration, FreezeAnimationDuringStun, GetHitEffect);

            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller, position) => controller.StoreAction(() =>
                {
                    ExecuteAoeAbility(null, c => position, AoeRadius,
                    ComputeDamage(controller.Combatant), ExcludeInstigator, controller.Combatant,
                    StunDuration, FreezeAnimationDuringStun, GetHitEffect);
                }, delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = (controller, position) => ExecuteAoeAbility(null, c => position, AoeRadius,
                    ComputeDamage(controller.Combatant), ExcludeInstigator, controller.Combatant,
                    StunDuration, FreezeAnimationDuringStun, GetHitEffect);
            }
        }
    }

    //Instance needs to fill in:
    //(*) TicksToAchieveMaxPower and PowerGainRate
    //(*) GetData/GetAoeCenter (either one) (Func<ICombatController,Vector2>)
    //(*) AoeRadius (float)
    //(*) ExcludeInstigator (bool)
    //(*) base AttackAbility stats
    //(*) whether it HasChannelAnimation and HasPowerUpAnimation (bools)
    public class AoePowerUpAbility : PowerUpAbility<Vector2>, IAoeAbility
    {
        protected Collider2D[] hitColliders;

        public float AoeRadius { get; init; } = 2;
        public bool ExcludeInstigator { get; init; } = true;

        public Func<ICombatController, Vector2> GetAoeCenter
        {
            get => GetData;
            init
            {
                GetData = value;
            }
        }

        public AoePowerUpAbility(DelayedAbilityExecutionOptions delayOptions) : base(delayOptions)
        {
            OnExecute = (controller, args) => 
                ExecuteAoeAbility(null, c => args.Item1, AoeRadius,
                ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                ExcludeInstigator, controller.Combatant,
                StunDuration, FreezeAnimationDuringStun, GetHitEffect);


            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller, args) => controller.StoreAction(() =>
                {
                    ExecuteAoeAbility(null, c => args.Item1, AoeRadius,
                    ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                    ExcludeInstigator, controller.Combatant,
                    StunDuration, FreezeAnimationDuringStun, GetHitEffect);
                }, delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = (controller, args) =>
                    ExecuteAoeAbility(null, c => args.Item1, AoeRadius,
                    ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                    ExcludeInstigator, controller.Combatant,
                    StunDuration, FreezeAnimationDuringStun, GetHitEffect);
            }
        }
    }
}