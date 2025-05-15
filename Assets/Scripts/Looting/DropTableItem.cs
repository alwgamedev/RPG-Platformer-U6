using RPGPlatformer.Inventory;
using System;
using UnityEngine;

namespace RPGPlatformer.Loot
{
    [Serializable]
    public struct DropTableItem : ISerializationCallbackReceiver
    {
        [SerializeField] InventoryItemSO item;
        [SerializeField] int minQuantity;
        [SerializeField] int maxQuantity;
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

            var q = MiscTools.rng.Next(minQuantity, maxQuantity + 1);
            return item.CreateInstanceOfItem().ToInventorySlotData(q);
        }

        public void OnBeforeSerialize()
        {
            minQuantity = Math.Max(minQuantity, 0);
            maxQuantity = Math.Max(maxQuantity, 0);
            minQuantity = Math.Min(minQuantity, maxQuantity);
        }

        public void OnAfterDeserialize() { }
    }
}