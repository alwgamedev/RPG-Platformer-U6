using System;
using System.Collections.Generic;
using RPGPlatformer.UI;
using RPGPlatformer.Core;

namespace RPGPlatformer.Inventory
{
    public class InventoryItem : IExaminable
    {
        protected InventoryItemData baseData;
        protected Action OnUse;
        protected Action<int> OnRelease;

        public List<(string, Action)> RightClickActions { get; protected set; } = new();

        public InventoryItemData BaseData { get => baseData; set => baseData = value; }

        public InventoryItem(InventoryItemData data)
        {
            baseData = data;
            InitializeRightClickActions();
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

        public InventorySlotDataContainer ToSlotData(int quantity = 1)
        {
            return new(this, quantity);
        }

        public virtual void OnPlacedInInventorySlot(IInventoryOwner owner, int slotIndex)
        {
            OnRelease = (k) =>
            {
                owner.ReleaseFromSlot(slotIndex, k);
            };
        }

        public virtual void OnRemovedFromInventorySlot()
        {
            OnUse = null;
            OnRelease = null;
        }

        public virtual void Use()
        {
            OnUse?.Invoke();
        }

        public virtual void Release(int k = 0)
        {
            OnRelease?.Invoke(k);
        }

        public virtual void Examine()
        {
            GameLog.Log(baseData.Description);
        }

        public virtual string TooltipText()
        {
            return "";
        }

        protected virtual void InitializeRightClickActions()
        {
            RightClickActions = new()
            {
                ($"Use {baseData.DisplayName}", Use),
            };
        }


    }
}