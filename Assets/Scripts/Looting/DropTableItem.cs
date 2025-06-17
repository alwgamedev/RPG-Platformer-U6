using RPGPlatformer.Inventory;
using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.Loot
{
    [Serializable]
    public struct DropTableItem
    {
        [SerializeField] InventoryItemSO item;
        [SerializeField] RandomizableInt quantity;
        [Range(0, 1)][SerializeField] float dropChance;

        public IInventorySlotDataContainer RollAndGenerateDropItem()
        {
            return GenerateDropItem(MiscTools.RandomFloat(0, 1));
        }

        public IInventorySlotDataContainer GenerateDropItem(float roll)
        {
            if (roll > dropChance)
            {
                return null;
            }

            return item.CreateInstanceOfItem().ToInventorySlotData(quantity.Value);
        }

        public void OnAfterDeserialize() { }
    }
}