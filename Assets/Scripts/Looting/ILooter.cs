using RPGPlatformer.Inventory;

namespace RPGPlatformer.Loot
{
    public interface ILooter
    {
        //usually if not handleOverflow, that means that excess items will just be ignored
        //(rather than say dropped in a loot bag)
        public void TakeLoot(IInventorySlotDataContainer loot, bool handleOverflow = true);

        public void TakeLoot(IInventorySlotDataContainer[] loot, bool handleOverflow = true);
    }
}