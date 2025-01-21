using System;
using UnityEngine;
using RPGPlatformer.Inventory;
using System.Collections.Generic;
using UnityEngine.U2D.Animation;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEditor.Progress;

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
        [SerializeField] List<SpriteResolver> dependentResolvers = new();

        EquippableItem equippedItem;
        Dictionary<string, SpriteResolver> SpriteResolverForCategory = new();
        Dictionary<string, string> DefaultLabelForCategory = new();

        public EquippableItem EquipppedItem => equippedItem;
        public GameObject EquippedItemGO { get; protected set; }

        public string SortingLayer => sortingLayer;
        public int SortingOrder => sortingOrder;
        public static int NumEquipmentSlots => Enum.GetNames(typeof(EquipmentSlot)).Length;

        public event Action OnItemEquipped;

        private void Awake()
        {
            BuildDictionaries();
        }

        private void Start()
        {
            if (defaultItem && equipOnStart)
            {
                EquipItem((EquippableItem)defaultItem.CreateInstanceOfItem());
            }
        }

        private void BuildDictionaries()
        {
            foreach (var resolver in dependentResolvers)
            {
                string category = resolver.GetCategory();
                SpriteResolverForCategory[category] = resolver;
                DefaultLabelForCategory[category] = resolver.GetLabel();
            }
        }

        //NOTE: EquipItem(null) will work perfectly for unequipping
        public void EquipItem(EquippableItem item)
        {
            if (EquippedItemGO)
            {
                Destroy(EquippedItemGO);
            }

            equippedItem = item;
            SwapSprites(equippedItem);
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

        protected virtual void SwapSprites(EquippableItem item)
        {
            var swapData = item?.EquippableItemData?.SpriteSwapData;

            foreach (var entry in SpriteResolverForCategory)
            {
                entry.Value.SetCategoryAndLabel(entry.Key, GetLabel(entry.Key, swapData));
            }
        }

        string GetLabel(string category, List<SpriteSwapData> itemData)
        {
            if (itemData == null)
            {
                return DefaultLabelForCategory[category];
            }

            foreach (var datum in itemData)
            {
                if (datum.Category == category)
                {
                    return datum.Label;
                }
            }
            return DefaultLabelForCategory[category];
        }

        protected virtual GameObject AttachPrefab(EquippableItem item)
        {
            if (item?.EquippableItemData?.Prefab != null)
            {
                return Instantiate(item.EquippableItemData.Prefab, transform);
            }
            return null;
        }

        private void OnDestroy()
        {
            OnItemEquipped = null;
        }
    }
}
