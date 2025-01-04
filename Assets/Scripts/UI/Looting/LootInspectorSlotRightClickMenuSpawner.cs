using UnityEngine;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.UI
{
    public class LootInspectorSlotRightClickMenuSpawner : InventorySlotRightClickMenuSpawner
    {
        public override void ConfigureMenu(GameObject menu)
        {
            InventoryItem item = slotComponent.Item();
            CreateAndConfigureDropButtons(menu, menuButtonPrefab, slotComponent, "Take");
            CreateAndConfigureButton(menu, menuButtonPrefab, $"Examine {item.BaseData.DisplayName}", item.Examine);
            CreateAndConfigureButton(menu, menuButtonPrefab, "Cancel", ClearMenu);
        }
    }
}