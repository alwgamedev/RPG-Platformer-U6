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
            punchingArm.BeginTrackingGuide();
        }

        public void EndPunch()
        {
            punchingArm.EndTracking();
        }

        public override void OutOfRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            if (combatController.ChannelingAbility) return;

            if (!charging && !awaitingBeginCharge && UnarmedWeaponEquipped && unarmedAttacksCounter > 1)
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

        private void OnAutoAttack()
        {
            if (!UnarmedWeaponEquipped)
            {
                rangedAttacksCounter++;
                if (rangedAttacksCounter > 1)
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
            }
            ((AICombatant)combatController.Combatant).EquipWeaponSwap("Unarmed (Mother Spider)");
        }

        private void EquipRangedWeapon()
        {
            if (UnarmedWeaponEquipped)
            {
                rangedAttacksCounter = 0;
            }
            ((AICombatant)combatController.Combatant).EquipWeaponSwap("Ranged Weapon (Mother Spider)");
        }
    }
}