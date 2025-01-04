using System;
using UnityEngine;

namespace RPGPlatformer.Inventory
{
    using static RPGPlatformer.Core.ItemSlot;

    [Serializable]
    public class EquippableItemData
    {
        [SerializeField] protected GameObject prefab;
        [SerializeField] protected EquipmentSlots slot;

        public GameObject Prefab => prefab;
        public EquipmentSlots Slot => slot;
    }
}