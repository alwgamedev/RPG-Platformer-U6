using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Inventory;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    using static ItemSlot;

    public class EquipmentInspectorUI : HideableUI
    {
        [SerializeField] EquipmentInspectorSlotUI headSlot;
        [SerializeField] EquipmentInspectorSlotUI torsoSlot;
        [SerializeField] EquipmentInspectorSlotUI mainhandSlot;
        [SerializeField] EquipmentInspectorSlotUI offhandSlot;

        Dictionary<EquipmentSlots, EquipmentInspectorSlotUI> slots = new();

        IEquippableCharacter player;

        protected override void Awake()
        {
            base.Awake();

            player = GameObject.FindWithTag("Player").GetComponent<IEquippableCharacter>();

            slots = new()
            {
                [EquipmentSlots.Head] = headSlot,
                [EquipmentSlots.Torso] = torsoSlot,
                [EquipmentSlots.Mainhand] = mainhandSlot,
                [EquipmentSlots.Offhand] = offhandSlot
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
                var displayedItem = entry.Value.Item();

                if (charSlot.EquipppedItem != displayedItem)
                {
                    character.EquipItem((EquippableItem)displayedItem, entry.Key, false);
                }
            }
        }

        public void UpdateUI(IEquippableCharacter character, EquipmentSlots equipSlot)
        {
            EquippableItem equippedItem = character.EquipSlots[equipSlot].EquipppedItem;
            slots[equipSlot].PlaceItem(equippedItem?.ToSlotData(1));
            slots[equipSlot].DisplayItem();
        }
    }
}