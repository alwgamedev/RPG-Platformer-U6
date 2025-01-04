using System;
using UnityEngine;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public struct InventoryItemData
    {
        [SerializeField] string lookupName;
        [SerializeField] string displayName;
        [SerializeField] string description;
        [SerializeField] Sprite icon;
        [SerializeField][Min(1)] int maxStack;

        public string LookupName => lookupName;
        public string DisplayName => $"<b>{displayName}</b>";
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxStack => maxStack;

        public InventoryItemData(string lookupName, string displayName, string description, Sprite icon, int maxStackable = 1)
        {
            this.lookupName = lookupName;
            this.displayName = displayName;
            this.description = description;
            this.icon = icon;
            this.maxStack = maxStackable;
        }

        public void ClampMaxStackable()
        {
            if(maxStack <= 0)
            {
                maxStack = 1;
            }
        }
    }
}