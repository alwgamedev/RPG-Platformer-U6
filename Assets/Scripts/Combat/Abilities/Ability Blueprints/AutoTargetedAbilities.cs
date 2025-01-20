using System;
using UnityEngine;
using RPGPlatformer.Effects;

namespace RPGPlatformer.Combat
{
    //Instance needs to fill in:
    //(*) AutoTarget method (Func<ICombatController, Health>)
    //(*) base AttackAbility stats only
    public class AutoTargetedAbility : AttackAbility
    {
        public Func<ICombatController, IHealth> AutoTarget;
        public new Action<ICombatController, IHealth> OnExecute;

        public AutoTargetedAbility(bool executeTriggeredInAnimation = false, bool channelWhileStored = true)
        {
            if (executeTriggeredInAnimation)
            {
                OnExecute = (controller, target) => controller.StoreAction(() =>
                    DealDamage(controller.Combatant, target,
                        ComputeDamage(controller.Combatant), StunDuration, FreezeAnimationDuringStun, GetHitEffect),
                        channelWhileStored);
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
            controller.Combatant.CheckIfTargetInRange(target, out var result);
            if (!result) return;

            controller.Combatant.Attack();
            OnExecute?.Invoke(controller, target);
            controller.OnAbilityExecute(this);

            PoolableEffect effect = GetCombatantExecuteEffect?.Invoke();
            if (effect)
            {
                effect.PlayAtPosition(controller.Combatant.Transform);
            }

            UpdateCombatantStats(controller.Combatant);
        }

        public static void DealDamageWithRangeCheck(ICombatant combatant, IHealth target, float damage,
            float? stunDuration = null, bool freezeAnimationDuringStun = true, 
            Func<PoolableEffect> getHitEffect = null)
        {
            combatant.CheckIfTargetInRange(target, out bool result);
            if (result)
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
            Transform transform = combatant.Transform;
            float radius = combatant.EquippedWeapon.WeaponStats.AttackRange / 2;
            Vector2 center = transform.position + radius * Mathf.Sign(transform.localScale.x) * transform.right;
            return combatant.FindTarget(center, radius);
        }

        public static IHealth TargetCentered(ICombatant combatant)
        {
            Transform transform = combatant.Transform;
            float radius = combatant.EquippedWeapon.WeaponStats.AttackRange;
            return combatant.FindTarget(transform.position, radius);
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
        public CloseRangeAbility(bool executeTriggeredInAnimation = false, bool channelWhileStored = true) 
            : base(executeTriggeredInAnimation, channelWhileStored)
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
        public AutoTargetedBleed() : base()
        {
            OnExecute = async (controller, target) =>
            await Bleed(controller.Combatant, target, ComputeDamage(controller.Combatant),
                        BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);
            //{
            //    //IHealth target = AutoTarget?.Invoke(controller);
            //    controller.Combatant.CheckIfTargetInRange(target, out bool result);
            //    if (result)
            //    {
            //        await Bleed(controller.Combatant, target, ComputeDamage(controller.Combatant), 
            //            BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);
            //    }
            //};
        }
    }

    //Instance needs to fill in:
    //(*) AutoTarget (Func<ICombatController,Health>)
    //(*) base AttackAbility stats
    //(*) OnExecute = Action<(Health, int)>
    //(*) whether it HasChannelAnimation (bool)
    public class AutoTargetOnNextFireButtonDown 
        : AbilityThatGetsDataOnNextFireButtonDownAndExecutesImmediately<IHealth>
    {
        public Func<ICombatController, IHealth> AutoTarget;

        public AutoTargetOnNextFireButtonDown() : base()
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
        public AutoTargetOnNextFireButtonDownSingleDamage() : base()
        {
            OnExecute = (controller, target) => 
                DealDamage(controller.Combatant, target, ComputeDamage(controller.Combatant),
                StunDuration, FreezeAnimationDuringStun, GetHitEffect);
        }
    }

    public class AutoTargetOnNextFireButtonDownBleed : AutoTargetOnNextFireButtonDown
    {
        public AutoTargetOnNextFireButtonDownBleed() : base()
        {
            OnExecute = async (controller, target) =>
                await Bleed(controller.Combatant, target, ComputeDamage(controller.Combatant),
                BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);
        }
    }

    //Instance needs to fill in:
    //(*) TicksToAchieveMaxPower and PowerGainRate
    //(*) AutoTarget (Func<ICombatController,Health>)
    //(*) base AttackAbility stats
    //(*) OnExecute = Action<(Health, int)>
    //(*) whether it HasChannelAnimation and HasPowerUpAnimation (bools)
    public class AutoTargetedPowerUpAbility : PowerUpAbility<IHealth>
    {
        public Func<ICombatController, IHealth> AutoTarget { get; init; }

        public AutoTargetedPowerUpAbility() : base()
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
        public AutoTargetedPowerUpWithSingleDamageHit() : base()
        {
             OnExecute = (controller, args) =>
                AutoTargetedAbility.DealDamageWithRangeCheck(controller.Combatant, args.Item1,
                ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2),
                StunDuration, FreezeAnimationDuringStun, GetHitEffect);
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
        public AutoTargetedPowerUpBleed() : base()
        {
            OnExecute = async (controller, args) =>
                await Bleed(controller.Combatant, args.Item1, 
                    ComputeDamage(controller.Combatant) * ComputePowerMultiplier(args.Item2), 
                    BleedCount, BleedRate, DamagePerBleedIteration, GetHitEffect);
            //NOTE: if no target or target out of range,
            //it will have already cancelled the ability in GetData (see AutoTargetedPowerUpAbility)
        }

    }
}