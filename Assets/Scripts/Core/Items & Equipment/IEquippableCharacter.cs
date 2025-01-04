using RPGPlatformer.Inventory;
using System.Collections.Generic;

namespace RPGPlatformer.Core
{
    public interface IEquippableCharacter
    {
        public Dictionary<ItemSlot.EquipmentSlots, ItemSlot> EquipSlots { get; }

        public void EquipItem(EquippableItem item, ItemSlot.EquipmentSlots slot, bool handleUnequippedItem = true);

        public void HandleUnequippedItem(EquippableItem item);
    }
}