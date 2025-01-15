namespace RPGPlatformer.Combat
{
    public class AbilityBarItem : IAbilityBarItem
    {
        AttackAbility ability;
        bool includeInAutoCastCycle;

        public AttackAbility Ability => ability;
        public bool IncludeInAutoCastCycle => includeInAutoCastCycle;

        public AbilityBarItem() { }

        public AbilityBarItem(AttackAbility ability, bool includeInAutoCastCycle)
        {
            this.ability = ability;
            this.includeInAutoCastCycle = includeInAutoCastCycle;
        }

        public void SetIncludeInAutoCastCycle(bool val)
        {
            includeInAutoCastCycle = val;
        }

        public void ToggleIncludeInAutoCastCycle()
        {
            includeInAutoCastCycle = !includeInAutoCastCycle;
        }
    }
}