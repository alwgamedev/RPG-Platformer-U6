using RPGPlatformer.Inventory;

namespace RPGPlatformer.Loot
{
    public interface ILootDropper
    {
        public void DropLoot();

        public void DropLoot(IInventorySlotDataContainer loot);

        public void DropLoot(IInventorySlotDataContainer[] loot);
    }
}