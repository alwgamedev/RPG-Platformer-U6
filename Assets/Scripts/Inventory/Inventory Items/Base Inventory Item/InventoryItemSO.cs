using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Inventory
{
    [CreateAssetMenu(menuName = "Inventory Items/New Inventory Item", fileName = "New Inventory Item")]
    public class InventoryItemSO : ScriptableObject
    {
        [SerializeField] protected InventoryItemData baseData;

        public InventoryItemData BaseData => baseData;

        public static Dictionary<string, InventoryItemSO> FindItemSO = new();

        public virtual InventoryItem CreateInstanceOfItem()
        {
            return new(baseData);
        }

        //public static InventoryItemSO FindByName(string name)
        //{
        //    foreach (var entry in InventoryItemLookup)
        //    {
        //        if (entry.Value == name)
        //        {
        //            return entry.Key;
        //        }
        //    }
        //    return null;
        //}

        public void UpdateLookup()
        {
            FindItemSO[baseData.LookupName] = this;
        }

        private void OnValidate()
        {
            baseData.ClampMaxStackable();
        }
    }
}
