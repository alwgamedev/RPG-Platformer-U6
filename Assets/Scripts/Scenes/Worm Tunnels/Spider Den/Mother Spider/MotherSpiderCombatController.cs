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

        //public override void OnAbilityExecute(AttackAbility ability)
        //{
        //    base.OnAbilityExecute(ability);

        //    if (ability == MotherSpiderAbilities.MotherSpiderBite)
        //    {
        //        GameLog.Log("You've been infected by the spider's poisonous bite!");
        //    }
        //}
    }
}