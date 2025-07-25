﻿using System;
using UnityEngine;
using RPGPlatformer.Effects;
using UnityEngine.InputSystem.XR;
using static UnityEngine.Rendering.GPUSort;

namespace RPGPlatformer.Combat
{
    //Instance needs to fill in:
    //(*) AutoTarget method (Func<ICombatController, Health>)
    //(*) base AttackAbility stats only
    public class AutoTargetedAbility : AttackAbility
    {
        public Func<ICombatController, IHealth> AutoTarget;
        public new Action<ICombatController, IHealth> OnExecute;

        public bool AllowExecuteWithoutTarget { get; init; }

        public AutoTargetedAbility(DelayedAbilityExecutionOptions delayOptions) : base(delayOptions)
        {
            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller, target) => controller.StoreAction(() =>
                    DealDamage(controller.Combatant, target,
                        ComputeDamage(controller.Combatant), StunDuration, FreezeAnimationDuringStun, GetHitEffect),
                        delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = (controller, target) => DealDamage(controller.Combatant,
                    target, ComputeDamage(controller.Combatant), StunDuration, FreezeAnimationDuringStun, GetHitEffect);
            }
        }

        public override void Execute(ICombatController controller)
        {
            if (!CanBeExecuted(controller)) return;

            IHealth target = AutoTarget(controller);

            if (target == null && !AllowExecuteWithoutTarget)
            {
                return;
            }

            controller.Combatant.TriggerCombat();
            OnExecute?.Invoke(controller, target);
            controller.OnAbilityExecute(this);

            PoolableEffect effect = GetCombatantExecuteEffect?.Invoke();
            if (effect)
            {
                effect.PlayAtPosition(controller.Combatant.transform);
            }

            UpdateCombatantStats(controller.Combatant);
        }

        //dont worry this is not being used for std auto-target abilities where range check is already done
        //in execute (actually this method isn't being used at all rn)
        public static void DealDamageWithRangeCheck(ICombatant combatant, IHealth target, float damage,
            float? stunDuration = null, bool freezeAnimationDuringStun = true,
            Func<PoolableEffect> getHitEffect = null)
        {
            if (combatant.TargetInRange(target))
            {
                DealDamage(combatant, target, damage, stunDuration, freezeAnimationDuringStun, getHitEffect);
            }
        }


        //TARGETING METHODS

        public static IHealth CustomTarget(ICombatant combatant, Vector2 center, float radius)
        {
            return combatant.FindTarget(center, radius);
        }

        public static IHealth TargetInFront(ICombatant combatant)//almost all melee abilities will use this
        {
            float radius = combatant.AttackRange / 2 + combatant.Health.TargetingTolerance;
            Vector2 center = combatant.transform.position 
                + radius * Mathf.Sign(combatant.transform.localScale.x) * combatant.transform.right;
            return combatant.FindTarget(center, radius);
        }

        public static IHealth TargetCentered(ICombatant combatant)
        {
            float radius = combatant.AttackRange;
            return combatant.FindTarget(combatant.transform.position, radius);
        }

        public static IHealth TargetAimPosition(ICombatController controller)
        {
            Vector2 aimPos = controller.GetAimPosition();
            return CustomTarget(controller.Combatant, aimPos, 0.5f);
        }
    }

    //Instance needs to fill in:
    //(*) base AttackAbility stats
    public class CloseRangeAbility : AutoTargetedAbility
    {
        public CloseRangeAbility(DelayedAbilityExecutionOptions delayOptions) : base(delayOptions)
        {
            AutoTarget = (controller) => TargetInFront(controller.Combatant);
        }
    }

    //Instance needs to fill in:
    //(*) BleedCount and BleedRate
    //(*) AutoTarget method (Func<ICombatController, Health>)
    //(*) base AttackAbility stats only
    public class AutoTargetedBleed : AutoTargetedAbility
    {
        public AutoTargetedBleed(DelayedAbilityExecutionOptions delayOptions) : base(delayOptions)
        {
            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller, target)
                    => controller.StoreAction(async () =>
                        await Bleed(controller.Combatant, target, ComputeDamage(controller.Combatant),
                        BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect),
                        delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = async (controller, target) =>
                    await Bleed(controller.Combatant, target, ComputeDamage(controller.Combatant),
                        BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);
            }
        }
    }

    //Instance needs to fill in:
    //(*) AutoTarget (Func<ICombatController,Health>)
    //(*) base AttackAbility stats
    //(*) OnExecute = Action<(Health, int)>
    //(*) whether it HasChannelAnimation (bool)
    public abstract class AutoTargetOnNextFireButtonDown 
        : AbilityThatGetsDataOnNextFireButtonDownAndExecutesImmediately<IHealth>
    {
        public Func<ICombatController, IHealth> AutoTarget;

        public AutoTargetOnNextFireButtonDown(DelayedAbilityExecutionOptions delayOptions) : base(delayOptions)
        {
            GetData = (controller) =>
            {
                IHealth target = AutoTarget(controller);
                EndChannelIfTargetNotInRange(controller, target, DelayedReleaseOfChannel);
                return target;
            };
        }
    }

    //Instance needs to fill in:
    //(*) AutoTarget (Func<ICombatController,Health>)
    //(*) base AttackAbility stats
    //(*) whether it HasChannelAnimation (bool)
    public class AutoTargetOnNextFireButtonDownSingleDamage : AutoTargetOnNextFireButtonDown
    {
        public AutoTargetOnNextFireButtonDownSingleDamage(DelayedAbilityExecutionOptions delayOptions) 
            : base(delayOptions)
        {
            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller, target)
                    => controller.StoreAction(() => 
                        DealDamage(controller.Combatant, target, ComputeDamage(controller.Combatant),
                        StunDuration, FreezeAnimationDuringStun, GetHitEffect), 
                        delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = (controller, target) =>
                DealDamage(controller.Combatant, target, ComputeDamage(controller.Combatant),
                StunDuration, FreezeAnimationDuringStun, GetHitEffect);
            }
        }
    }

    public class AutoTargetOnNextFireButtonDownBleed : AutoTargetOnNextFireButtonDown
    {
        public AutoTargetOnNextFireButtonDownBleed(DelayedAbilityExecutionOptions delayOptions) 
            : base(delayOptions)
        {
            //OnExecute = async (controller, target) =>
            //    await Bleed(controller.Combatant, target, ComputeDamage(controller.Combatant),
            //    BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);

            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller, target)
                    => controller.StoreAction(async () =>
                        await Bleed(controller.Combatant, target, ComputeDamage(controller.Combatant),
                        BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect),
                        delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = async (controller, target) =>
                    await Bleed(controller.Combatant, target, ComputeDamage(controller.Combatant),
                        BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);
            }
        }
    }

    //Instance needs to fill in:
    //(*) TicksToAchieveMaxPower and PowerGainRate
    //(*) AutoTarget (Func<ICombatController,Health>)
    //(*) base AttackAbility stats
    //(*) OnExecute = Action<(Health, int)>
    //(*) whether it HasChannelAnimation and HasPowerUpAnimation (bools)
    public abstract class AutoTargetedPowerUpAbility : PowerUpAbility<IHealth>
    {
        public Func<ICombatController, IHealth> AutoTarget { get; init; }

        public AutoTargetedPowerUpAbility(DelayedAbilityExecutionOptions delayOptions) : base(delayOptions)
        {
            GetData = (controller) =>
            {
                IHealth target = AutoTarget(controller);
                EndChannelIfTargetNotInRange(controller, target, DelayedReleaseOfChannel);
                return target;
            };
        }
    }

    //Instance needs to fill in:
    //(*) TicksToAchieveMaxPower and PowerGainRate
    //(*) AutoTarget (Func<ICombatController,Health>)
    //(*) base AttackAbility stats
    //(*) whether it HasChannelAnimation and HasPowerUpAnimation (bools)
    public class AutoTargetedPowerUpWithSingleDamageHit : AutoTargetedPowerUpAbility
    {
        public AutoTargetedPowerUpWithSingleDamageHit(DelayedAbilityExecutionOptions delayOptions) 
            : base(delayOptions)
        {
             OnExecute = (controller, args) =>
                AutoTargetedAbility.DealDamageWithRangeCheck(controller.Combatant, args.Item1,
                ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                StunDuration, FreezeAnimationDuringStun, GetHitEffect);

            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller, args)
                    => controller.StoreAction(() =>
                        AutoTargetedAbility.DealDamageWithRangeCheck(controller.Combatant, args.Item1,
                        ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                        StunDuration, FreezeAnimationDuringStun, GetHitEffect),
                        delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = (controller, args) =>
                    AutoTargetedAbility.DealDamageWithRangeCheck(controller.Combatant, args.Item1,
                    ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                    StunDuration, FreezeAnimationDuringStun, GetHitEffect);
            }
        }
    }

    //Instance needs to fill in: 
    //(*) BleedCount and BleedRate
    //(*) TicksToAchieveMaxPower and PowerGainRate
    //(*) AutoTarget (Func<ICombatController,Health>)
    //(*) base AttackAbility stats
    //(*) whether it HasChannelAnimation and HasPowerUpAnimation (bools)
    public class AutoTargetedPowerUpBleed : AutoTargetedPowerUpAbility
    {
        public AutoTargetedPowerUpBleed(DelayedAbilityExecutionOptions delayOptions) 
            : base(delayOptions)
        {
            OnExecute = async (controller, args) =>
                await Bleed(controller.Combatant, args.Item1, 
                    ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2), 
                    BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);
            //NOTE: if no target or target out of range,
            //it will have already cancelled the ability in GetData (see AutoTargetedPowerUpAbility)

            if (DelayOptions.delayExecute)
            {
                OnExecute = (controller, args)
                    => controller.StoreAction(async () =>
                        await Bleed(controller.Combatant, args.Item1,
                        ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                        BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect),
                        delayOptions.channelDuringDelay, delayOptions.endChannelOnExecute);
            }
            else
            {
                OnExecute = async (controller, args) =>
                    await Bleed(controller.Combatant, args.Item1,
                    ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                    BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);
            }
        }

    }
}