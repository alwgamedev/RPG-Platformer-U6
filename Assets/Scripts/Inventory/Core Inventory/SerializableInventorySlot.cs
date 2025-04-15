using System;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class SerializableInventorySlot
    {
        public SerializableInventoryItem Item { get; set; }
        public int Quantity { get; set; }

        //public InventorySlot CreateSlot()
        //{
        //    return new(Item?.CreateItem(), Quantity);
        //}
        //^bad bc the inventory manager needs to do some additional configuration when the item is place in slot
        //so have the inventory manager place the items
    }
}