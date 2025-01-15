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

        public Transform Transform => transform;
        public bool AllowReplacementIfCantSwap => false;

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
            return item?.ItemCopy().ToSlotData(quantity);
            //"return this" causes bigly issues
        }
        
        public InventoryItem Item()
        {
            return item;
        }

        public int Quantity()
        {
            return quantity;
        }

        public virtual bool CanPlace(IInventorySlotDataContainer data)
        {
            return true;
        }

        public void PlaceItem(IInventorySlotDataContainer data)
        {
            item = data?.Item();
            quantity = data?.Quantity() ?? 0;
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
                item.Use();
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
            quantityText.text = quantity.ToString();
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