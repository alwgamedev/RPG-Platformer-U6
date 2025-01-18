using RPGPlatformer.Core;

namespace RPGPlatformer.Inventory
{
    public class EquippableItem : InventoryItem
    {
        protected EquippableItemData equippableItemData;

        public EquippableItemData EquippableItemData => equippableItemData;

        public EquippableItem(InventoryItemData baseData,  EquippableItemData equippableItemData) : base(baseData)
        {
            this.equippableItemData = equippableItemData;
        }

        public override InventoryItem ItemCopy()
        {
            return new EquippableItem(baseData, equippableItemData);
        }

        public override void OnPlacedInInventorySlot(IInventoryOwner owner, int slotIndex)
        {
            base.OnPlacedInInventorySlot(owner, slotIndex);

            if (owner is not IEquippableCharacter character) return;
            OnUse = () =>
            {
                if (character.CanEquip(this))
                {
                    owner.Inventory.RemoveFromSlot(slotIndex);
                    character.EquipItem(this/*, equippableItemData.Slot*/);
                }
            };
        }
        //base class sets OnUsed = null and OnRelease = null when remove from inventory

        public override string TooltipText()
        {
            return $"{equippableItemData.Slot} Slot Item";
        }

        protected override void InitializeRightClickActions()
        {
            RightClickActions = new()
            {
                ($"Equip {baseData.DisplayName}", Use)
            };
        }
    }
}