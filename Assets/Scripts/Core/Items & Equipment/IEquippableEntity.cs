using RPGPlatformer.Inventory;

namespace RPGPlatformer.Core
{
    public interface IEquippableEntity
    {
        public bool CanEquip(EquippableItem item);

        public void EquipItem(EquippableItem item, bool handleUnequippedItem = true);

        public void UnequipItem(EquipmentSlot slot, bool handleUnequippedItem = true);

        public void HandleUnequippedItem(EquippableItem item);
    }
}