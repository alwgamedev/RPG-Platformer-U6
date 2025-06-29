using System;
using UnityEngine;
using RPGPlatformer.Inventory;
using System.Collections.Generic;
using UnityEngine.U2D.Animation;

namespace RPGPlatformer.Core
{
    public enum EquipmentSlot
    {
        Head, Torso, Legs, Mainhand, Offhand
    }

    public class ItemSlot : MonoBehaviour //ISavable
    {
        [SerializeField] string sortingLayer;
        [SerializeField] int sortingOrder;
        [SerializeField] EquippableItemSO defaultItemSO;
        [SerializeField] List<SpriteResolver> dependentResolvers = new();

        OneSidedItem osi;
        EquippableItem equippedItem;
        Dictionary<string, SpriteResolver> SpriteResolverForCategory = new();
        Dictionary<string, string> DefaultLabelForCategory = new();

        public EquippableItem defaultItem;

        //public EquippableItem DefaultItem => defaultItem;
        public EquippableItem EquippedItem => equippedItem;
        public EquippableItemGO EquippedItemGO { get; protected set; }

        //public string SortingLayer => sortingLayer;
        //public int SortingOrder => sortingOrder;
        //public static int NumEquipmentSlots => Enum.GetNames(typeof(EquipmentSlot)).Length;

        public event Action OnItemEquipped;

        private void Awake()
        {
            BuildDictionaries();

            //if (defaultItemSO != null)
            //{
            //    defaultItem = (EquippableItem)defaultItemSO.CreateInstanceOfItem();
            //}
            //else
            //{
            //    defaultItem = null;
            //}
        }

        public void InitializeDefaultItemFromSO(bool overrideCurrentDefault = false)
        {
            if (defaultItem != null && !overrideCurrentDefault) return;
            if (defaultItemSO == null) return;

            defaultItem = (EquippableItem)defaultItemSO.CreateInstanceOfItem();
        }

        //private void Start()
        //{
        //    if (DefaultItem == null && defaultItemSO != null)
        //    {
        //        DefaultItem = (EquippableItem)defaultItemSO.CreateInstanceOfItem();
        //    }
        //}

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
                Destroy(EquippedItemGO.gameObject);
            }

            equippedItem = item;
            SwapSprites(equippedItem);
            EquippedItemGO = AttachPrefab(equippedItem);

            if (EquippedItemGO)
            {
                Renderer renderer = EquippedItemGO.GetComponentInChildren<Renderer>();
                if (renderer)
                {
                    renderer.sortingLayerName = sortingLayer;
                    renderer.sortingOrder = sortingOrder;

                    if (renderer.gameObject.TryGetComponent(out SortingLayerControl slc))
                    {
                        slc.UpdateSortingData();
                        //notiy all dependents of the renderer's new sorting data
                    }
                    else
                    {
                        var slds = renderer.gameObject.GetComponent<SortingLayerDataSource>();
                        slds?.UpdateSortingData();
                    }

                    if (!osi)
                    {
                        osi = GetComponent<OneSidedItem>();
                    }
                    if (osi)
                    {
                        osi.itemRenderer = renderer;
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

        protected virtual EquippableItemGO AttachPrefab(EquippableItem item)
        {
            if (item?.EquippableItemData?.ItemGO != null)
            {
                return Instantiate(item.EquippableItemData.ItemGO, transform);
            }
            return null;
        }

        private void OnDestroy()
        {
            OnItemEquipped = null;
        }

        //public JsonNode CaptureState()
        //{
        //    return JsonSerializer.SerializeToNode(equippedItem?.ConvertToSerializable());
        //}

        //public void RestoreState(JsonNode jNode)
        //{
        //    defaultItem = (EquippableItem)(jNode.Deserialize<SerializableInventoryItem>()?.CreateItem());
        //}
    }
}
