using RPGPlatformer.Inventory;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class EquipmentInspectorSlotRightClickMenuSpawner : InventorySlotRightClickMenuSpawner
    {
        public override void ConfigureMenu(GameObject menu)
        {
            EquippableItem item = slotComponent.Item as EquippableItem;
            EquipmentInspectorSlotUI slot = slotComponent as EquipmentInspectorSlotUI;

            CreateAndConfigureButton(menu, menuButtonPrefab, $"Unequip {item.BaseData.DisplayName}", slot.UnequipFromSlot);
            CreateAndConfigureButton(menu, menuButtonPrefab, $"Examine {item.BaseData.DisplayName}", item.Examine);
            CreateAndConfigureButton(menu, menuButtonPrefab, "Cancel", ClearMenu);
        }
    }
}