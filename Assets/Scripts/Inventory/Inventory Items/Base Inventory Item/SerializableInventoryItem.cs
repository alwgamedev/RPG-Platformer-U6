using RPGPlatformer.Core;
using System;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class SerializableInventoryItem
    {
        public string LookupName { get; set; }

        public InventoryItem CreateItem()
        {
            var itemSO = InventoryItemSO.FindByName(LookupName);
            if (itemSO != null)
            {
                return itemSO.CreateInstanceOfItem();
            }
            return null;
        }
    }
}