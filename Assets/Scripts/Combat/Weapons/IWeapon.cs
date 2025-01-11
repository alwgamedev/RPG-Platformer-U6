namespace RPGPlatformer.Combat
{
    public interface IWeapon
    {
        public WeaponStats WeaponStats { get; }
        public CombatStyle CombatStyle { get; }
    }
}
