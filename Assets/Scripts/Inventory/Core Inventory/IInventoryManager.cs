namespace RPGPlatformer.Inventory
{
    public interface IInventoryManager
    {
        public int NumSlots { get; }

        public bool ContainsItem(string lookupName);

        public bool ContainsItem(InventoryItem item);

        public void PlaceInSlotOrDistributeToFirstAvailable(int i, IInventorySlotDataContainer data);

        public IInventorySlotDataContainer[] DistributeToFirstAvailableSlots(IInventorySlotDataContainer[] data);

        public IInventorySlotDataContainer DistributeToFirstAvailableSlots(IInventorySlotDataContainer data);

        //returns quantity removed
        public int RemoveItem(InventoryItem item, int quantity = 1);

        public int RemoveItem(string itemLookup, int quantity = 1);

        public IInventorySlotDataContainer RemoveFromSlot(int i, int quantity);

        public IInventorySlotDataContainer RemoveAllFromSlot(int i);

        public IInventorySlotDataContainer[] RemoveAllItems();
    }
}