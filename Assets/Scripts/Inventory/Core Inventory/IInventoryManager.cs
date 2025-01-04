namespace RPGPlatformer.Inventory
{
    public interface IInventoryManager
    {
        public int NumSlots { get; }

        public void PlaceInSlot(int i, IInventorySlotDataContainer data);

        public IInventorySlotDataContainer RemoveFromSlot(int i, int quantity);

        public IInventorySlotDataContainer RemoveAllFromSlot(int i);
    }
}