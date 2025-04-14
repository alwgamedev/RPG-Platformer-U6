namespace RPGPlatformer.UI
{
    public class LootInspectorSlotUI : InventorySlotUI
    {
        public override void UseItem()
        {
            item?.ReleaseFromInventory(quantity);
        }
    }
}