namespace RPGPlatformer.Combat
{
    public interface IAbilityBarItem
    {
        public AttackAbility Ability { get; }
        public bool IncludeInAutoCastCycle { get; }
    }
}