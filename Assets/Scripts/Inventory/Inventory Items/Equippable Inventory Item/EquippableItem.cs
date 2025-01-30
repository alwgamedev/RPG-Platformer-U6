using NUnit.Framework;
using RPGPlatformer.Core;
using RPGPlatformer.UI;
using Unity.VisualScripting;

namespace RPGPlatformer.Inventory
{
    public class EquippableItem : InventoryItem
    {
        protected EquippableItemData equippableItemData = new();

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
            var result = $"{equippableItemData.Slot} Slot Item"
                + $"\nDamage Bonus: {equippableItemData.DamageBonus: 0.#}"
                + $"\nDefense Bonus: {equippableItemData.DefenseBonus: 0.#}";

            return result + LevelReqsText();
        }

        public string LevelReqsText()
        {
            string result = "";

            var levelReqs = equippableItemData.LevelReqs;
            if (levelReqs != null)
            {
                for (int i = 0; i < levelReqs.Count; i++)
                {
                    result += $"\n<b>Requires</b> level {levelReqs[i].Level} {levelReqs[i].Skill}";
                }
            }
            return result;
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