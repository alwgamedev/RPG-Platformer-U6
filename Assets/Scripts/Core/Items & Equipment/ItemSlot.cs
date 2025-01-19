using System;
using UnityEngine;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Core
{
    public enum EquipmentSlot
    {
        Head, Torso, Mainhand, Offhand
    }

    public class ItemSlot : MonoBehaviour
    {
        [SerializeField] string sortingLayer;
        [SerializeField] int sortingOrder;
        [SerializeField] bool equipOnStart;
        [SerializeField] EquippableItemSO defaultItem;

        EquippableItem equippedItem;

        public EquippableItem EquipppedItem => equippedItem;
        public GameObject EquippedItemGO { get; protected set; }

        public string SortingLayer => sortingLayer;
        public int SortingOrder => sortingOrder;
        public static int NumEquipmentSlots => Enum.GetNames(typeof(EquipmentSlot)).Length;

        public event Action OnItemEquipped;

        //public static bool IsWeaponSlot(EquipmentSlot slot)
        //{
        //    return slot == EquipmentSlot.Mainhand || equip
        //}

        //public static EquipmentSlots EquipmentSlot(int slot)
        //{
        //    return (EquipmentSlots)slot;
        //}

        //public static int SlotIndex(EquipmentSlots slot)
        //{
        //    return (int)slot;
        //}

        private void Start()
        {
            if (defaultItem && equipOnStart)
            {
                EquipItem((EquippableItem)defaultItem.CreateInstanceOfItem());
            }
        }

        public void EquipItem(EquippableItem item)
        {
            equippedItem = item;
            if (EquippedItemGO)
            {
                Destroy(EquippedItemGO);
            }
            EquippedItemGO = AttachPrefab(equippedItem);
            if (EquippedItemGO)
            {
                SpriteRenderer sprite = EquippedItemGO.GetComponentInChildren<SpriteRenderer>();
                if (sprite)
                {
                    sprite.sortingLayerName = sortingLayer;
                    sprite.sortingOrder = sortingOrder;

                    OneSidedItem osi = GetComponent<OneSidedItem>();
                    if (osi)
                    {
                        osi.itemSprite = sprite;
                    }
                }
            }

            OnItemEquipped?.Invoke();
        }

        protected virtual GameObject AttachPrefab(EquippableItem item)
        {
            if(item == null || item.EquippableItemData.Prefab == null)
            {
                return null;
            }
            return Instantiate(item.EquippableItemData.Prefab, transform);
        }
        //armour slots may have a different protocol for doing this (they may need to set a sprite in sprite library)

        private void OnDestroy()
        {
            OnItemEquipped = null;
        }
    }
}
