using System;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class SerializableInventorySlot
    {
        public SerializableInventoryItem Item { get; set; }
        public int Quantity { get; set; }

        public InventorySlot CreateSlot()
        {
            return new(Item?.CreateItem(), Quantity);
        }
    }
}