using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Inventory;
using RPGPlatformer.Core;
using Unity.VisualScripting;
using UnityEngine.TextCore.Text;

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

        //CollapsableUI collapsableUI;
        //Animation highlightAnim;
        IEquippableCharacter player;

        protected override void Awake()
        {
            base.Awake();

            //collapsableUI = GetComponent<CollapsableUI>();
            //highlightAnim = collapsableUI.CollapseButton.GetComponent<Animation>();

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
            player = GlobalGameTools.Instance.Player.Combatant;
            Configure(player);
            //this does an initial display (see ** below), so we are okay if this Start executes after player
            //(and misses the first equip event)
        }

        public void Configure(IEquippableCharacter character)
        {
            foreach(var entry in slots)
            {
                entry.Value.Configure(character, entry.Key);
                character.EquipSlots[entry.Key].OnItemEquipped += UpdateUI;
                entry.Value.OnDragResolved += UpdateCharacter;

                UpdateUI(character, entry.Key);//**initial display
            }
        }

        public void UpdateCharacter()
        {
            UpdateCharacter(player);
        }

        public void UpdateCharacter(IEquippableCharacter character)
        {
            foreach(var entry in slots)
            {
                var charSlot = character.EquipSlots[entry.Key];
                var displayedItem = entry.Value.Item;

                if (charSlot.EquippedItem != displayedItem)
                {
                    if (displayedItem != null)
                    {
                        character.EquipItem((EquippableItem)displayedItem, false);
                    }
                    else
                    {
                        character.UnequipItem(entry.Key, false);
                    }
                }
            }
        }

        //update all slots at once (mainly so we can match an Action delegate, but also
        //in case equip event in one slot has affected other slots -- in that case we would get
        //an event for each slot, but oh well we have to match Action delegate...)
        public void UpdateUI()
        {
            //if (!collapsableUI.IsOpen)
            //{
            //    highlightAnim.Play();
            //}

            foreach (var entry in slots)
            {
                UpdateUI(player, entry.Key);
            }
        }

        public void UpdateUI(IEquippableCharacter character, EquipmentSlot equipSlot)
        {
            EquippableItem equippedItem = character.EquipSlots[equipSlot].EquippedItem;
            slots[equipSlot].PlaceItem(equippedItem?.ToInventorySlotData(1));
            slots[equipSlot].DisplayItem();
        }

        protected override void OnDestroy()
        {
            if (player != null)
            {
                foreach (var entry in slots)
                {
                    var key = player.EquipSlots[entry.Key];
                    var val = entry.Value;
                    if (key != null)
                    {
                        key.OnItemEquipped -= UpdateUI;
                    }
                    if (val != null)
                    {
                        val.OnDragResolved -= UpdateCharacter;
                    }
                }
            }
        }
    }
}