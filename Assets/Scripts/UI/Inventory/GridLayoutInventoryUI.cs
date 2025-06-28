using RPGPlatformer.Inventory;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class GridLayoutInventoryUI : InventoryUI
    {
        [SerializeField] bool fillLastRowWithEmptySlots;

        protected GridLayoutGroup gridLayoutGroup;

        protected override void Awake()
        {
            base.Awake();

            gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
        }

        public override void InitializeSlots(IInventoryOwner owner)
        {
            slots = new InventorySlotUI[owner.Inventory.NumSlots];
        }

        public override void Configure(IInventoryOwner owner)
        {
            InitializeSlots(owner);

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = Instantiate(inventorySlotPrefab, gridLayoutGroup.transform);
                slots[i].PlaceItem(owner.Inventory.GetDataForSlot(i));
                slots[i].DisplayItem();
                slots[i].OnDragResolved += () => owner.Inventory.MatchItems(slots);
            }

            if(fillLastRowWithEmptySlots)
            {
                int n = slots.Length;
                int lastRowCount = n % gridLayoutGroup.constraintCount;
                if (lastRowCount == 0) return;//last row is already full

                int remainder = gridLayoutGroup.constraintCount - lastRowCount;
                var newSlots = new InventorySlotUI[n + remainder];
                Array.Copy(slots, newSlots, n);
                for(int i = 0; i < remainder; i++)
                {
                    if (owner.Inventory.TryAddNewSlot())
                    {
                        var slot = Instantiate(inventorySlotPrefab, gridLayoutGroup.transform);
                        slot.PlaceItem(null);
                        slot.DisplayItem();
                        slot.OnDragResolved += () => owner.Inventory.MatchItems(slots);
                        newSlots[n + i] = slot;
                    }
                }

                slots = newSlots;
            }
        }
    }
}