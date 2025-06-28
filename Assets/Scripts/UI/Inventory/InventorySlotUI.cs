using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.UI
{
    //[RequireComponent(typeof(InventorySlotRightClickMenuSpawner))]
    public class InventorySlotUI : MonoBehaviour, IInventorySlotDataContainer, 
        IDragDropSlot<IInventorySlotDataContainer>, IPointerClickHandler
    {
        [SerializeField] protected Image slotIcon;
        [SerializeField] protected TextMeshProUGUI quantityText;
        [SerializeField] protected TextMeshProUGUI dosesText;

        protected Transform draggableChild;
        protected InventoryItem item;
        protected int quantity;

        public Transform DraggableParentTransform => transform;
        public bool AllowReplacementIfCantSwap => false;
        public InventoryItem Item => item;
        public int Quantity => quantity;

        public event Action OnDragResolved;
        //public event Action OnItemChanged;

        private void Awake()
        {
            draggableChild = GetComponentInChildren<DraggableInventoryItem>().transform;
        }

        public void SetIconSize(Vector2 sizeDelta)
        {
            ((RectTransform)slotIcon.transform).sizeDelta = sizeDelta;
        }

        public IInventorySlotDataContainer Contents()
        {
            return item?.ItemCopy().ToInventorySlotData(quantity);
            //"return this" causes bigly issues
        }

        public virtual bool ItemCanBeDragged()
        {
            return item != null && quantity > 0;
        }

        public virtual bool CanPlace(IInventorySlotDataContainer data, 
            IDragSource<IInventorySlotDataContainer> origin = null)
        {
            return true;
        }

        public void PlaceItem(IInventorySlotDataContainer data)
        {
            if (data?.Item == null || data.Quantity <= 0)
            {
                item = null;
                quantity = 0;
            }
            else
            {
                item = data.Item;
                quantity = data.Quantity;
            }
            //OnItemChanged?.Invoke();
            //we don't need to control quantity here, because the data will be copied from the backing inventory, where
            //max stackable is already enforced
        }

        public void RemoveItem()
        {
            item = null;
            quantity = 0;
            //OnItemChanged?.Invoke();
        }

        public virtual void UseItem()
        {
            if(item != null)
            {
                item.UseItem();
            }
        }

        public void DisplayItem()
        {
            SetIcon(item);
            SetQuantityText();
            SetDosesText();
        }

        public void DragComplete()
        {
            OnDragResolved?.Invoke();
        }

        public void DropComplete()
        {
            OnDragResolved?.Invoke();
        }

        public bool TryTransfer(IDragDropSlot<IInventorySlotDataContainer> target)
        {
            if (target == null)
            {
                return false;
            }

            if (item == null || quantity <= 0)
            {
                return true;
            }

            var c = target.Contents();
            var m = item.BaseData.MaxStack;
            
            if (!item.Equals(c?.Item) || c.Quantity >= m)
            {
                return false;
            }

            var qT = Math.Min(m, c.Quantity + quantity);//how much target will have after transfer
            var qS = qT - c.Quantity;//how much was transferred
            target.PlaceItem(item.ToInventorySlotData(qT));
            PlaceItem(item.ToInventorySlotData(quantity - qS));
            return true;

        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.IsLeftMouseButtonEvent())
            {
                UseItem();
            }
        }

        private void SetQuantityText()
        {
            if (!quantityText) return;
            if (item == null || item.BaseData.MaxStack == 1)
            {
                quantityText.text = "";
                return;
            }
            quantityText.text = $"{quantity:N0}";
        }

        private void SetDosesText()
        {
            if(!dosesText) return;
            if(item != null && item is IDosedItem dosed && dosed.Doses > 1)
            {
                dosesText.text = $"({dosed.DosesRemaining}/{dosed.Doses})";
                return;
            }
            dosesText.text = "";
            
        }

        private void SetIcon(InventoryItem item)
        {
            if(!slotIcon)
            {
                return;
            }
            else if (item != null && item.BaseData.Icon != null)
            {
                slotIcon.sprite = item.BaseData.Icon;
                slotIcon.preserveAspect = true;
                slotIcon.enabled = true;
            }
            else
            {
                slotIcon.enabled = false;
                slotIcon.sprite = null;
            }
        }

        private void OnDestroy()
        {
            Destroy(draggableChild.gameObject);//in case the draggable is detached from the slot when it is destroyed
            OnDragResolved = null;
            //OnItemChanged = null;
        }
    }
}