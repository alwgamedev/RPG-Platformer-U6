using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Inventory;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class EquipmentInspectorUI : HidableUI
    {
        [SerializeField] EquipmentInspectorSlotUI headSlot;
        [SerializeField] EquipmentInspectorSlotUI torsoSlot;
        [SerializeField] EquipmentInspectorSlotUI legsSlot;
        [SerializeField] EquipmentInspectorSlotUI mainhandSlot;
        [SerializeField] EquipmentInspectorSlotUI offhandSlot;

        Dictionary<EquipmentSlot, EquipmentInspectorSlotUI> slots = new();

        IEquippableCharacter player;

        protected override void Awake()
        {
            base.Awake();

            player = GameObject.FindWithTag("Player").GetComponent<IEquippableCharacter>();

            slots = new()
            {
                [EquipmentSlot.Head] = headSlot,
                [EquipmentSlot.Torso] = torsoSlot,
                [EquipmentSlot.Legs] = legsSlot,
                [EquipmentSlot.Mainhand] = mainhandSlot,
                [EquipmentSlot.Offhand] = offhandSlot
            };
        }

        private void Start()
        {
            Configure(player);
        }

        public void Configure(IEquippableCharacter character)
        {
            foreach(var entry in slots)
            {
                entry.Value.Configure(character, entry.Key);
                character.EquipSlots[entry.Key].OnItemEquipped += () => UpdateUI(character, entry.Key);
                entry.Value.OnDragResolved += () => UpdateCharacter(character);

                UpdateUI(character, entry.Key);
            }
        }

        public void UpdateCharacter(IEquippableCharacter character)
        {
            foreach(var entry in slots)
            {
                var charSlot = character.EquipSlots[entry.Key];
                var displayedItem = entry.Value.Item;

                if (charSlot.EquipppedItem != displayedItem)
                {
                    character.EquipItem((EquippableItem)displayedItem, false);
                }
            }
        }

        public void UpdateUI(IEquippableCharacter character, EquipmentSlot equipSlot)
        {
            EquippableItem equippedItem = character.EquipSlots[equipSlot].EquipppedItem;
            slots[equipSlot].PlaceItem(equippedItem?.ToSlotData(1));
            slots[equipSlot].DisplayItem();
        }
    }
}