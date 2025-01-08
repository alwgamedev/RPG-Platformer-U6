namespace RPGPlatformer.Inventory
{
    public interface IInventoryOwner
    {
        public bool IsPlayer { get; }

        public InventoryManager Inventory { get; }

        public void ReleaseFromSlot(int slotIndex, int quantity = 1);

        public void HandleInventoryOverflow(IInventorySlotDataContainer data);

        public void HandleInventoryOverflow(IInventorySlotDataContainer[] data);
    }
}