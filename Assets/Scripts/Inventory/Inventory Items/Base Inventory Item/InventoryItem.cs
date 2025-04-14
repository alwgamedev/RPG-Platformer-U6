using System;
using System.Collections.Generic;
using RPGPlatformer.UI;
using RPGPlatformer.Core;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace RPGPlatformer.Inventory
{
    public class InventoryItem : IExaminable
    {
        protected InventoryItemData baseData;
        protected Action Use;
        protected Action<int> ReleaseItem;

        //public List<(string, Action)> RightClickActions { get; protected set; } = new();

        public InventoryItemData BaseData { get => baseData; set => baseData = value; }

        public InventoryItem(InventoryItemData data)
        {
            baseData = data;
            //InitializeRightClickActions();
        }

        public SerializableInventoryItem ConvertToSerializable()
        {
            var ser = new SerializableInventoryItem()
            {
                LookupName = baseData.LookupName
            };

            return ser;
        }

        public static bool ItemsAreOfSameType(InventoryItem item1, InventoryItem item2)
        {
            if(item1 == null || item2 == null)
            {
                return false;
            }
            if(item1 is IDosedItem dosed1 && item2 is IDosedItem dosed2)
            {
                if(dosed1.Doses != dosed2.Doses || dosed1.DosesRemaining != dosed2.DosesRemaining)
                {
                    return false;
                }
            }
            return item1.BaseData.DisplayName == item2.BaseData.DisplayName;
        }

        public virtual InventoryItem ItemCopy()
        {
            return new(baseData);
        }

        public InventorySlotDataContainer ToInventorySlotData(int quantity = 1)
        {
            return new(this, quantity);
        }

        public virtual void OnPlacedInInventorySlot(IInventoryOwner owner, int slotIndex)
        {
            ReleaseItem = (k) =>
            {
                owner.ReleaseFromSlot(slotIndex, k);
            };
        }

        public virtual void OnRemovedFromInventorySlot()
        {
            Use = null;
            ReleaseItem = null;
        }

        public void UseItem()
        {
            Use?.Invoke();
        }

        public virtual void ReleaseFromInventory(int k = 0)
        {
            ReleaseItem?.Invoke(k);
        }

        public virtual void Examine()
        {
            GameLog.Log(baseData.Description);
        }

        public virtual string TooltipText()
        {
            return "";
        }

        public virtual IEnumerable<(string, Action)> RightClickActions()
        {
            if (Use != null)
            {
                yield return ($"Use {baseData.DisplayName}", Use);
            }
        }

        //protected virtual void InitializeRightClickActions()
        //{
        //    RightClickActions = new()
        //    {
        //        ($"Use {baseData.DisplayName}", Use),
        //    };
        //}
    }
}