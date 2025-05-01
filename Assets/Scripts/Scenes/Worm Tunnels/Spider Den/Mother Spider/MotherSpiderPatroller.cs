using RPGPlatformer.Combat;
using System;

namespace RPGPlatformer.AIControl
{
    public class MotherSpiderPatroller : CombatPatroller
    {
        //[SerializeField] float targetingToleranceOverride;

        float unarmedAttackRange;

        bool UnarmedWeaponEquipped => combatController.Combatant.CurrentCombatStyle == CombatStyle.Unarmed;

        //public override float TargetingTolerance => targetingToleranceOverride;
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
        }

        public override void OutOfRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            if (UnarmedWeaponEquipped)
            {
                EquipRangedWeapon();
            }
            else
            {
                //^so that he pursues you more closely without stuttering at the ranged weapon's attack range
                if (!combatController.ChannelingAbility)
                {
                    EquipUnarmedWeapon();
                    Trigger(typeof(Pursuit).Name);
                }
            }
        }

        public override void InRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            if (!UnarmedWeaponEquipped && !combatController.ChannelingAbility)
            {
                if (combatController.Combatant.CanAttackAtDistSqrd(distanceSqrd, tolerance, unarmedAttackRange))
                {
                    EquipUnarmedWeapon();
                }
            }

            //do this whether you swapped or not
            base.InRangeAttackBehavior(distanceSqrd, tolerance);
        }

        //protected override void MaintainMinimumCombatDistance(float targetDistSqrd, float tolerance)
        //{
        //    base.MaintainMinimumCombatDistance(targetDistSqrd, 0.25f);
        //}

        private void EquipUnarmedWeapon()
        {
            ((AICombatant)combatController.Combatant).EquipWeaponSwap("Unarmed (Mother Spider)");
        }

        private void EquipRangedWeapon()
        {
            ((AICombatant)combatController.Combatant).EquipWeaponSwap("Ranged Weapon (Mother Spider)");
        }
    }
}