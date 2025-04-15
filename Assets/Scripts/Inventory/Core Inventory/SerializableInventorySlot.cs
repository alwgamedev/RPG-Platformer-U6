using System;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class SerializableInventorySlot
    {
        public SerializableInventoryItem Item { get; set; }
        public int Quantity { get; set; }

        //NOTE: inventory manager needs to do some additional configuration when an item is placed in slot
        //so it's best to have the inventory manager create a new InventorySlot()
        //and place the item itself
    }
}