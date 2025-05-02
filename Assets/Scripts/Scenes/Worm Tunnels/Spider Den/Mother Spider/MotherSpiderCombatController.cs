using RPGPlatformer.AIControl;

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
    }
}