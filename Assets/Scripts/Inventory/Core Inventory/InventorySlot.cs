using System;

namespace RPGPlatformer.Inventory
{
    public class InventorySlot : InventorySlotDataContainer
    {
        public InventorySlot(InventoryItem item = null, int quantity = 0) : base(item, quantity) { }

        public bool HasSpaceForMore()
        {
            return Item == null || Quantity < Item.BaseData.MaxStack;
        }

        public int MaxAddition()
        {
            if (Item == null)
            {
                return 0;
            }
            return Math.Max(Item.BaseData.MaxStack - Quantity, 0);
        }

        public IInventorySlotDataContainer PlaceItem(IInventorySlotDataContainer data)
        {
            if(data == null || data.Item == null || data.Quantity == 0)
            {
                item = null;
                quantity = 0;
                return null;
            }

            if(InventoryItem.ItemsAreOfSameType(Item, data.Item))//false if either is null
            {
                int addable = Math.Min(MaxAddition(), data.Quantity);
                quantity += addable;
                return data.Item.ToSlotData(data.Quantity - addable);
            }

            item = data.Item;
            quantity = Math.Min(data.Quantity, data.Item.BaseData.MaxStack);
            return data.Item.ItemCopy().ToSlotData(data.Quantity - Quantity);
        }

        public IInventorySlotDataContainer Remove(int quantity = 1)//NOTE: needs to be paired with EmptySlot() if Quantity == 0 after removing items
        {
            var copy = Item?.ItemCopy();
            int available = Math.Max(0, Math.Min(quantity, this.quantity));
            this.quantity -= available;
            return copy?.ToSlotData(available);
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