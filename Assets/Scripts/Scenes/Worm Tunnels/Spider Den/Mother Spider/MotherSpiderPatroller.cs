using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class MotherSpiderPatroller : CombatPatroller
    {
        float unarmedAttackRange;
        int rangedAttacksCounter;
        bool charging;

        //and if false we will assume the ranged weapon is equipped
        bool UnarmedWeaponEquipped => combatController.Combatant.CurrentCombatStyle == CombatStyle.Unarmed;

        //public override float TargetingTolerance => base.TargetingTolerance / 2;
        //or just make it's collider less wide (but keep in mind we still need it's collider to bump into walls)

        private void Start()
        {
            unarmedAttackRange = combatController.Combatant.UnarmedWeapon.WeaponStats.AttackRange;
            combatController.CombatExited += EquipRangedWeapon;
            //let this be the default weapon
            //basically so that if you leave den and come back he has the ranged weapon initially,
            //like he would on the first encounter
            //(I could put these in the patroller controller's state change handlers,
            //but I'd rather not create a new script rn)

            combatController.AutoAttacked += OnAutoAttack;
        }

        public override void OutOfRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            if (combatController.ChannelingAbility) return;

            if (!charging && UnarmedWeaponEquipped)
            {
                EquipRangedWeapon();
            }
            else
            {
                //EquipUnarmedWeapon();
                //^so that he pursues you more closely without stuttering at the ranged weapon's attack range
                Trigger(typeof(Pursuit).Name);

            }
        }

        public override void InRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            if (!UnarmedWeaponEquipped && !combatController.ChannelingAbility
                && combatController.Combatant.CanAttackAtDistSqrd(distanceSqrd, tolerance, unarmedAttackRange))
            {
                //doing attRange / 2 so that if player is running away you get close enough to do an
                //unarmed attack before having to pursue again
                EquipUnarmedWeapon();
            }

            //do this whether you swapped or not
            base.InRangeAttackBehavior(distanceSqrd, tolerance);
        }

        protected override bool ShouldBreakPursuitToAttack(float distSquared, float tolerance)
        {
            if (UnarmedWeaponEquipped)
            {
                //so that she gets close enough to fire an unarmed attack before having to pursue again
                return base.ShouldBreakPursuitToAttack(distSquared, tolerance / 4);
            }
            else
            {
                return base.ShouldBreakPursuitToAttack(distSquared, tolerance);
            }
        }

        private void OnAutoAttack()
        {
            if (!UnarmedWeaponEquipped)
            {
                rangedAttacksCounter++;
                if (rangedAttacksCounter > 1)
                {
                    combatController.OnChannelEnded += ChannelEndedHandler;

                    void ChannelEndedHandler()
                    {
                        if (!UnarmedWeaponEquipped && combatController.IsInCombat)
                        {
                            BeginCharge();
                        }

                        combatController.OnChannelEnded -= ChannelEndedHandler;
                    }
                }
            }
            else if (charging)//don't end charge until you successfully land an unarmed hit
            {
                EndCharge();
            }
        }

        private void BeginCharge()
        {
            if (UnarmedWeaponEquipped || !combatController.IsInCombat)
            Debug.Log($"beginning charge (ranged attacks count {rangedAttacksCounter})");
            charging = true;
            EquipUnarmedWeapon();
        }

        private void EndCharge()
        {
            Debug.Log("ending charge");
            charging = false;
        }

        private void EquipUnarmedWeapon()
        {
            Debug.Log("equipping unarmed weapon");
            ((AICombatant)combatController.Combatant).EquipWeaponSwap("Unarmed (Mother Spider)");
        }

        private void EquipRangedWeapon()
        {
            Debug.Log("equipping ranged weapon");
            EndCharge();
            rangedAttacksCounter = 0;
            ((AICombatant)combatController.Combatant).EquipWeaponSwap("Ranged Weapon (Mother Spider)");
        }
    }
}