using UnityEngine;

namespace RPGPlatformer.Inventory
{
    [CreateAssetMenu(fileName = "New Equippable Item", menuName = "Equippable Items/New Equippable Item")]
    public class EquippableItemSO : InventoryItemSO
    {
        [SerializeField] protected EquippableItemData equippableItemData;

        public override InventoryItem CreateInstanceOfItem()
        {
            return new EquippableItem(baseData, equippableItemData);
        }
    }

}
