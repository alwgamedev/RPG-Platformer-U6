using RPGPlatformer.Inventory;
using System.Collections.Generic;

namespace RPGPlatformer.Core
{
    public interface IEquippableCharacter
    {
        public Dictionary<EquipmentSlot, ItemSlot> EquipSlots { get; }

        public bool CanEquip(EquippableItem item);

        public void EquipItem(EquippableItem item, /*EquipmentSlot slot,*/ bool handleUnequippedItem = true);

        public void UnequipItem(EquipmentSlot slot, bool handleUnequippedItem = true);

        public void HandleUnequippedItem(EquippableItem item);
    }
}