using UnityEngine;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.UI
{
    public abstract class InventoryUI : HideableUI
    {
        [SerializeField] protected InventorySlotUI inventorySlotPrefab;

        protected IInventoryOwner owner;
        protected InventorySlotUI[] slots;

        protected override void Awake()
        {
            base.Awake();
        }

        protected virtual void Start()
        {
            if (owner != null && owner.Inventory != null)
            {
                ConnectOwner(owner);
            }
        }

        public abstract void InitializeSlots(IInventoryOwner owner);

        public abstract void Configure(IInventoryOwner owner);

        public virtual void UpdateInventoryUI()
        {
            for(int i = 0; i < slots.Length; i++)
            {
                slots[i].PlaceItem(owner.Inventory.GetDataForSlot(i));
                slots[i].DisplayItem();
            }
        }

        protected virtual void ConnectOwner(IInventoryOwner owner)
        {
            this.owner = owner;
            Configure(this.owner);
            owner.Inventory.OnInventoryChanged += UpdateInventoryUI;
        }

        protected virtual void DisconnectOwner()
        {
            if (owner != null && owner.Inventory != null)
            {
                owner.Inventory.OnInventoryChanged -= UpdateInventoryUI;
            }

            if (slots != null)
            {
                foreach (var slot in slots)
                {
                    if (slot)
                    {
                        Destroy(slot.gameObject);
                    }
                }
            }

            owner = null;
            slots = null;
        }
    }
}