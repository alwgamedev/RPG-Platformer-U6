namespace RPGPlatformer.Inventory
{
    public class SerializableConsumableInventoryItem : SerializableInventoryItem
    {
        public int DosesRemaining { get; set; }

        public override InventoryItem CreateItem()
        {
            var itemSO = InventoryItemSO.FindItemSO[LookupName] as ConsumableInventoryItemSO;

            if (itemSO != null)
            {
                return itemSO.CreateInstanceOfItem(DosesRemaining);
            }

            return null;
        }
    }
}