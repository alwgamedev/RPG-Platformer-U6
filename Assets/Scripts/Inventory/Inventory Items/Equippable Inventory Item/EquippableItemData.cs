using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class EquippableItemData
    {
        [SerializeField] protected GameObject prefab;
        [SerializeField] protected List<SpriteSwapData> spriteSwapData = new();
        [SerializeField] protected EquipmentSlot slot;

        public GameObject Prefab => prefab;
        public List<SpriteSwapData> SpriteSwapData => spriteSwapData;
        public EquipmentSlot Slot => slot;
    }
}