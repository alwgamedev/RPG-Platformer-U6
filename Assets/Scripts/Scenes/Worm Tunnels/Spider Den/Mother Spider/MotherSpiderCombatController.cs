using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class MotherSpiderCombatController : AICombatController
    {
        protected override void InitializeAbilityBars()
        {
            abilityBarManager.SetAbilityBar(CombatStyle.Unarmed, 
                MotherSpiderAbilities.MotherSpiderUnarmedAbilityBar(this));
            abilityBarManager.SetAbilityBar(CombatStyle.Ranged,
                MotherSpiderAbilities.MotherSpiderRangedAbilityBar(this));
        }

        public override Vector2 GetAimPosition()
        {
            if (Combatant.CurrentCombatStyle == CombatStyle.Ranged)
            {
                var b = base.GetAimPosition();
                var c = b - (Vector2)Combatant.EquipSlots[Core.EquipmentSlot.Mainhand].transform.position;
                var d = c.magnitude;
                return b + 0.01f * d * Mathf.Sign(c.x) * c.CCWPerp();
                //if you really want to do this correctly just calculate how much projectile will fall 
                //due to gravity by the time it reaches base.aimPos's x coord (but then we have to
                //account for the projectile's linear damping, which we don't have on hand in this context,
                //so I say good enough as is)
            }

            return base.GetAimPosition();
        }
    }
}