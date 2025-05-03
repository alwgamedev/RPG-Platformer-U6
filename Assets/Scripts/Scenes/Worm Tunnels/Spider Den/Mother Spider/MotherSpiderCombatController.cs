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
                return b + 0.015f * d * Mathf.Sign(c.x) * c.CCWPerp();
            }

            return base.GetAimPosition();
        }
    }
}