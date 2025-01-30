using System;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class InventorySlotDataContainer : IInventorySlotDataContainer
    {
        protected InventoryItem item;
        protected int quantity;

        public InventoryItem Item { get => item; set => item = value; }
        public int Quantity { get => quantity; set => quantity = value; }

        //public InventoryItem Item()
        //{
        //    return Item;
        //}

        //public int Quantity()
        //{
        //    return Quantity;
        //}

        public InventorySlotDataContainer(InventoryItem item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }
}