using System;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class EquippableItemData
    {
        [SerializeField] protected GameObject prefab;
        [SerializeField] protected EquipmentSlot slot;

        public GameObject Prefab => prefab;
        public EquipmentSlot Slot => slot;
    }
}