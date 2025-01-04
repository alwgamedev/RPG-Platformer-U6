using RPGPlatformer.Inventory;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class GridLayoutInventoryUI : InventoryUI
    {
        protected GridLayoutGroup gridLayoutGroup;

        protected override void Awake()
        {
            base.Awake();

            gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
        }

        protected override void Start()
        {
            if (owner != null && owner.Inventory != null)
            {
                ConnectOwner(owner);
            }
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
                //slots[i].SetIconSize(0.8f * gridLayoutGroup.cellSize);
                slots[i].DisplayItem();
                slots[i].OnDragResolved += () => owner.Inventory.MatchItems(slots);
            }
        }
    }
}