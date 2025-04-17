using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Inventory
{
    public interface IInventoryOwner
    {
        public bool IsPlayer { get; }

        public InventoryManager Inventory { get; }

        public bool HasItem(InventoryItem item)
        {
            if (item == null) return false;

            if (item is EquippableItem ei && this is IEquippableCharacter ec)
            {
                if (ec.EquipSlots.TryGetValue(ei.EquippableItemData.Slot, out var slot) && slot
                    && ei.Equals(slot.EquippedItem))
                {
                    return true;
                }
            }

            return Inventory != null && Inventory.ContainsItem(item);
        }

        public void ReleaseFromSlot(int slotIndex, int quantity = 1);

        public void HandleInventoryOverflow(IInventorySlotDataContainer data);

        public void HandleInventoryOverflow(IInventorySlotDataContainer[] data);
    }
}