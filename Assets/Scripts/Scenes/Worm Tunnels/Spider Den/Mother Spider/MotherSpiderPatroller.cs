using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class MotherSpiderPatroller : CombatPatroller
    {
        [SerializeField] IKLimbAnimator punchingArm;

        float unarmedAttackRange;
        int unarmedAttacksCounter;
        int rangedAttacksCounter;
        bool charging;
        bool awaitingBeginCharge;

        //System.Random rng = new();

        //if false, the ranged weapon is equipped
        bool UnarmedWeaponEquipped => combatController.Combatant.CurrentCombatStyle == CombatStyle.Unarmed;

        private void Start()
        {
            unarmedAttackRange = combatController.Combatant.UnarmedWeapon.WeaponStats.AttackRange;
            //combatController.CombatExited += EquipRangedWeapon;
            //^so that this is default weapon anytime you re-encounter spider (e.g. die and return to den)
            //^equipping ranged weapon also ends charge (so EndCharge is called on combat exit)
            combatController.AutoAttacked += OnAutoAttack;
        }

        //call from anim event and gradually increase tracking strength in anim
        public void BeginPunch()
        {
            punchingArm.BeginTrackingGuide(MiscTools.RandomFloat(0.4f, 1)/*(float)rng.NextDouble() * .6f + .4f*/);
        }

        public void EndPunch()
        {
            punchingArm.EndTracking();
        }

        public override void OutOfRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            if (combatController.ChannelingAbility) return;

            if (!charging && !awaitingBeginCharge && UnarmedWeaponEquipped && 
                (unarmedAttacksCounter > 1 || !CanPursue(distanceSqrd, tolerance)))
            {
                EquipRangedWeapon();
            }
            else
            {
                Trigger(typeof(Pursuit).Name);

            }
        }

        public override void InRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            if (!UnarmedWeaponEquipped && !combatController.ChannelingAbility
                && combatController.Combatant.CanAttackAtDistSqrd(distanceSqrd, tolerance, unarmedAttackRange))
            {
                EquipUnarmedWeapon();
            }

            //do this whether you swapped or not
            base.InRangeAttackBehavior(distanceSqrd, tolerance);
        }

        protected override bool ShouldBreakPursuitToAttack(float distSquared, float tolerance)
        {
            var b = base.ShouldBreakPursuitToAttack(distSquared, tolerance);
            var d = (CurrentTarget.transform.position.x - transform.position.x) * Vector3.right;
            if (!b && !MovementController.CanMove(d))
            {
                EquipRangedWeapon();
            }

            return b;
        }

        private void OnAutoAttack()
        {
            if (!UnarmedWeaponEquipped)
            {
                rangedAttacksCounter++;
                if (ShouldBeginCharge())
                {
                    awaitingBeginCharge = true;
                    combatController.OnChannelEnded += ChannelEndedHandler;

                    void ChannelEndedHandler()
                    {
                        combatController.OnChannelEnded -= ChannelEndedHandler;
                        awaitingBeginCharge = false;
                        if (combatController.IsInCombat)
                        {
                            BeginCharge();
                        }
                    }
                }
            }
            else
            {
                if (charging)
                {
                    EndCharge();
                }
                unarmedAttacksCounter++;
            }
        }

        private bool ShouldBeginCharge()
        {
            return rangedAttacksCounter > 2
                && TryGetDistanceSqrd(CurrentTarget, out var d2, out var t)
                && (combatController.Combatant.CanAttackAtDistSqrd(d2, t, unarmedAttackRange)
                || CanPursue(d2, t));
        }

        private void BeginCharge()
        {
            if (UnarmedWeaponEquipped || !combatController.IsInCombat)
            charging = true;
            EquipUnarmedWeapon();
        }

        private void EndCharge()
        {
            charging = false;
        }

        private void EquipUnarmedWeapon()
        {
            if (!UnarmedWeaponEquipped)
            {
                unarmedAttacksCounter = 0;
                rangedAttacksCounter = 0;
            }
            ((AICombatant)combatController.Combatant).EquipWeaponSwap("Unarmed (Mother Spider)");

            //combatController.StartAttacking();
        }

        private void EquipRangedWeapon()
        {
            if (UnarmedWeaponEquipped)
            {
                unarmedAttacksCounter = 0;
                rangedAttacksCounter = 0;
            }
            ((AICombatant)combatController.Combatant).EquipWeaponSwap("Ranged Weapon (Mother Spider)");

            //combatController.StartAttacking();
        }
    }
}