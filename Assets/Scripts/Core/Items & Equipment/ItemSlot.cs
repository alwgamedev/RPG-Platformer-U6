using System;
using UnityEngine;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Core
{
    public class ItemSlot : MonoBehaviour
    {
        public enum EquipmentSlots
        {
            Head, Torso, Mainhand, Offhand
        }

        [SerializeField] string sortingLayer;
        [SerializeField] int sortingOrder;
        [SerializeField] bool equipOnStart;
        [SerializeField] EquippableItemSO defaultItem;

        EquippableItem equippedItem;

        public EquippableItem EquipppedItem => equippedItem;
        public GameObject EquippedItemGO { get; protected set; }

        public string SortingLayer => sortingLayer;
        public int SortingOrder => sortingOrder;
        public static int NumEquipmentSlots => Enum.GetNames(typeof(EquipmentSlots)).Length;

        public event Action OnItemEquipped;

        public static EquipmentSlots EquipmentSlot(int slot)
        {
            return (EquipmentSlots)slot;
        }

        public static int SlotIndex(EquipmentSlots slot)
        {
            return (int)slot;
        }

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
            if(item == null)
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
