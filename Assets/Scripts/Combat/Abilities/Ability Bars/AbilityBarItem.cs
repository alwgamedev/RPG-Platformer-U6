namespace RPGPlatformer.Combat
{
    public class AbilityBarItem
    {
        public readonly AttackAbility ability;
        public readonly bool includeInAutoCastCycle;

        public AbilityBarItem() { }

        public AbilityBarItem(AttackAbility ability, bool includeInAutoCastCycle)
        {
            this.ability = ability;
            this.includeInAutoCastCycle = includeInAutoCastCycle;
        }
    }
}