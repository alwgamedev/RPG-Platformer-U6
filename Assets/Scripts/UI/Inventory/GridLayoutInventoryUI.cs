using RPGPlatformer.Inventory;
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
                int lastRowCount = slots.Count() % gridLayoutGroup.constraintCount;
                int remainder = gridLayoutGroup.constraintCount - lastRowCount;
                for(int i = 0; i < remainder; i++)
                {
                    if (owner.Inventory.TryAddNewSlot())
                    {
                        var slot = Instantiate(inventorySlotPrefab, gridLayoutGroup.transform);
                        slot.PlaceItem(null);
                        slot.DisplayItem();
                        slot.OnDragResolved += () => owner.Inventory.MatchItems(slots);
                        slots = slots.Append(slot).ToArray();
                    }
                }
            }
        }
    }
}