using System;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class InventorySlotDataContainer : IInventorySlotDataContainer
    {
        protected InventoryItem item;
        protected int quantity;

        public InventoryItem Item => item;
        public int Quantity => quantity;

        public InventorySlotDataContainer(InventoryItem item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }
}