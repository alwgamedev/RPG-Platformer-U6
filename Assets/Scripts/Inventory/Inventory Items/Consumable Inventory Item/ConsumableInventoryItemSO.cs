using UnityEngine;
namespace RPGPlatformer.Inventory
{
    [CreateAssetMenu(menuName = "Inventory Items/Consumable Inventory Item", fileName = "New Consumable Inventory Item")]
    public class ConsumableInventoryItemSO : InventoryItemSO
    {
        [SerializeField] ConsumableStats stats;

        public override InventoryItem CreateInstanceOfItem()
        {
            return new ConsumableInventoryItem(baseData, stats);
        }

        public ConsumableInventoryItem CreateInstanceOfItem(int dosesRemaining)
        {
            return new ConsumableInventoryItem(baseData, stats, dosesRemaining);
        }
    }
}