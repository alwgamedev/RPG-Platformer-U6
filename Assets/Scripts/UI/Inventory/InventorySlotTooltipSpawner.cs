using RPGPlatformer.Inventory;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class InventorySlotTooltipSpawner : TooltipSpawner
    {
        protected IInventorySlotDataContainer slotComponent;

        protected override void Awake()
        {
            base.Awake();

            slotComponent = GetComponent<IInventorySlotDataContainer>();
        }

        public override bool CanCreateTooltip()
        {
            if (!base.CanCreateTooltip()) return false;
            return slotComponent.Item() != null && tooltipPrefab;
        }

        public override void ConfigureTooltip(GameObject tooltip)
        {
            if (tooltip.TryGetComponent<InventoryItemTooltip>(out var abilityTooltip))
            {
                abilityTooltip.Configure(slotComponent.Item());
            }
            else
            {
                Debug.Log($"{GetType().Name} attached to {name} does not have the correct " +
                    $"type of tooltip prefab installed.");
                ClearTooltip();
            }
        }
    }
}