using RPGPlatformer.Inventory;

namespace RPGPlatformer.Loot
{
    public interface ILooter
    {
        public void TakeLoot(IInventorySlotDataContainer loot);

        public void TakeLoot(IInventorySlotDataContainer[] loot);
    }
}