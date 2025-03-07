using RPGPlatformer.Core;
using RPGPlatformer.Inventory;
using System;
using UnityEngine.EventSystems;

namespace RPGPlatformer.UI
{
    public class EquipmentInspectorSlotUI : InventorySlotUI
    {
        EquipmentSlot slot;
        IEquippableCharacter player;

        Action unequipAction;

        public EquippableItem EquippedItem => item as EquippableItem;

        public override bool CanPlace(IInventorySlotDataContainer data, 
            IDragSource<IInventorySlotDataContainer> origin = null)
        {
            return data?.Item == null
                || (data.Quantity == 1
                && data.Item is EquippableItem equippableItem
                && equippableItem.EquippableItemData.Slot == slot
                && player != null
                && player.CanEquip(equippableItem));
        }

        public void Configure(IEquippableCharacter player, EquipmentSlot slot)
        {
            this.slot = slot;
            this.player = player;
            unequipAction = () => player.UnequipItem(slot);
        }

        public void UnequipFromSlot()
        {
            unequipAction?.Invoke();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.IsLeftMouseButtonEvent())
            {
                UnequipFromSlot();
            }
        }

        private void OnDestroy()
        {
            unequipAction = null;
        }
    }
}