using RPGPlatformer.Core;

namespace RPGPlatformer.Inventory
{
    public interface IInventoryOwner
    {
        public bool IsPlayer { get; }

        public InventoryManager Inventory { get; }

        public bool HasItem(InventoryItem item)
        {
            if (item == null) return false;

            if (item is EquippableItem && this is IEquippableCharacter ec)
            {
                foreach (var slot in ec.EquipSlots)
                {
                    if (item.Equals(slot.Value.EquippedItem))
                    {
                        return true;
                    }
                }
            }

            return Inventory != null && Inventory.ContainsItem(item);
        }

        public void ReleaseFromSlot(int slotIndex, int quantity = 1);

        public void HandleInventoryOverflow(IInventorySlotDataContainer data);

        public void HandleInventoryOverflow(IInventorySlotDataContainer[] data);
    }
}