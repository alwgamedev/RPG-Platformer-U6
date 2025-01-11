using RPGPlatformer.Inventory;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class Weapon : EquippableItem, IWeapon
    {
        protected WeaponStats weaponStats;
        protected CombatStyle combatStyle;
        protected AnimatorOverrideController animatorOverrideController;

        public WeaponStats WeaponStats => weaponStats;
        public CombatStyle CombatStyle => combatStyle;
        public AnimatorOverrideController AnimatorOverrideController => animatorOverrideController;

        //TO-DO: equippableItemData.Slot should be limited to mainhand or offhand (never head or torso)

        public Weapon(InventoryItemData baseData, EquippableItemData equippableItemData, 
            WeaponStats weaponStats, CombatStyle combatStyle, AnimatorOverrideController animatorOverrideController) : base(baseData, equippableItemData)
        {
            this.weaponStats = weaponStats;
            this.combatStyle = combatStyle;
            this.animatorOverrideController = animatorOverrideController;
        }

        public override InventoryItem ItemCopy()
        {
            return new Weapon(baseData, equippableItemData, weaponStats, combatStyle, animatorOverrideController);
        }
    }
}