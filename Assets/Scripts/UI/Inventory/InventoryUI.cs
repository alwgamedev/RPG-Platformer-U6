using UnityEngine;
using RPGPlatformer.Inventory;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public abstract class InventoryUI : HidableUI
    {
        [SerializeField] protected InventorySlotUI inventorySlotPrefab;

        protected IInventoryOwner owner;
        protected InventorySlotUI[] slots;

        //protected override void Awake()
        //{
        //    base.Awake();
        //}

        protected virtual void Start()
        {
            FindOwner();

            if (owner != null && owner.Inventory != null)
            {
                ConnectOwner(owner);
                UpdateInventoryUI();
            }
        }

        protected virtual void FindOwner() { }

        public abstract void InitializeSlots(IInventoryOwner owner);

        public abstract void Configure(IInventoryOwner owner);

        public virtual void UpdateInventoryUI()
        {
            for(int i = 0; i < owner.Inventory.NumSlots; i++)
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
                    if (slot && slot.gameObject)
                    {
                        Destroy(slot.gameObject);
                    }
                }
            }

            owner = null;
            slots = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (owner != null && owner.Inventory != null)
            {
                owner.Inventory.OnInventoryChanged -= UpdateInventoryUI;
            }
        }
    }
}