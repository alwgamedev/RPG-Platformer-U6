using System;
using UnityEngine;

namespace RPGPlatformer.Inventory
{
    public class InventorySlot : InventorySlotDataContainer
    {
        public InventorySlot(InventoryItem item = null, int quantity = 0) : base(item, quantity) { }

        public bool HasSpaceForMore()
        {
            return Item == null || Quantity < Item.BaseData.MaxStack;
        }

        public IInventorySlotDataContainer PlaceItem(IInventorySlotDataContainer data)
        {
            if(data?.Item == null || data.Quantity == 0)
            {
                item = null;
                quantity = 0;
                return null;
            }

            if (data.Item.Equals(item))//in particular current item is not null
            {
                var addable = Math.Min(data.Quantity, item.BaseData.MaxStack - data.Quantity);
                quantity += addable;
                return data.Item.ToInventorySlotData(data.Quantity - addable);
            }

            item = data.Item;
            quantity = Math.Min(data.Quantity, data.Item.BaseData.MaxStack);
            return data.Item.ItemCopy().ToInventorySlotData(data.Quantity - quantity);
        }

        public IInventorySlotDataContainer Remove(int quantity = 1)
            //NOTE: needs to be paired with EmptySlot() if Quantity == 0 after removing items
        {
            var copy = Item?.ItemCopy();
            int available = Math.Max(0, Math.Min(quantity, this.quantity));
            this.quantity -= available;
            return copy?.ToInventorySlotData(available);
        }
                
        public void EmptySlot()
        {
            item = null;
            quantity = 0;
        }

        public SerializableInventorySlot ConvertToSerializable()
        {
            var ser = new SerializableInventorySlot()
            {
                Item = item?.ConvertToSerializable(),
                Quantity = quantity
            };

            return ser;
        }
    }
}