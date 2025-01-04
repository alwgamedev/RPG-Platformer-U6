namespace RPGPlatformer.Inventory
{
    public class InventorySlotDataContainer : IInventorySlotDataContainer
    {
        protected InventoryItem item;
        protected int quantity;

        public InventoryItem Item()
        {
            return item;
        }

        public int Quantity()
        {
            return quantity;
        }

        public InventorySlotDataContainer(InventoryItem item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }
}