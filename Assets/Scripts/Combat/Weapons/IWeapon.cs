namespace RPGPlatformer.Combat
{
    using static CombatStyles;

    public interface IWeapon
    {
        public WeaponStats WeaponStats { get; }
        public CombatStyle CombatStyle { get; }
    }
}
