using RPGPlatformer.Inventory;
using System;
using UnityEngine;

namespace RPGPlatformer.Loot
{
    [Serializable]
    public struct DropTableEntry
    {
        [SerializeField] bool useFallbackIfMainRollFails;
        [SerializeField] DropTableItem mainItem;
        [SerializeField] DropTableItem fallbackItem;
        //if the main item is very rare, you can use a common item as a fallback
        //to make sure a dropped item is produced

        public IInventorySlotDataContainer RollAndGenerateDropItem()
        {
            var mainRoll = mainItem.RollAndGenerateDropItem();

            if (useFallbackIfMainRollFails && mainRoll == null)
            {
                return fallbackItem.RollAndGenerateDropItem();
            }

            return mainRoll;
        }
    }
}