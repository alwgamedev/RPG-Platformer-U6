using UnityEngine;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Combat
{
    public class WeaponSO : EquippableItemSO, IWeapon //NOTE: if want to create a blank weapon in Unity, use EmptyWeaponSO.
    {
        [SerializeField] protected WeaponStats weaponStats;
        [SerializeField] protected AnimatorOverrideController animatorOverrideController;

        public WeaponStats WeaponStats => weaponStats;
        public virtual CombatStyle CombatStyle { get; }
        public AnimatorOverrideController AnimatorOverrideController => animatorOverrideController;

        public override InventoryItem CreateInstanceOfItem()
        {
            return new Weapon(baseData, equippableItemData, weaponStats, CombatStyle, animatorOverrideController);
        }
    }
}