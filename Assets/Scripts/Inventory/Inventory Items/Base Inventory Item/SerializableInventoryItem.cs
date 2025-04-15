using System;
using System.Text.Json.Serialization;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    [JsonDerivedType(typeof(SerializableInventoryItem), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(SerializableConsumableInventoryItem), typeDiscriminator: "consumable")]
    public class SerializableInventoryItem
    {
        public string LookupName { get; set; }

        public virtual InventoryItem CreateItem()
        {
            var itemSO = InventoryItemSO.FindItemSO[LookupName];

            if (itemSO != null)
            {
                return itemSO.CreateInstanceOfItem();
            }

            return null;
        }
    }
}