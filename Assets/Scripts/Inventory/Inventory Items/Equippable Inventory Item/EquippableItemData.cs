using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Skills;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public class EquippableItemData
    {
        [SerializeField] protected EquipmentSlot slot;
        [SerializeField] protected float damageBonus;
        [SerializeField] protected float defenseBonus;
        [SerializeField] protected List<LevelRequirement> levelReqs = new();
        //[SerializeField] protected GameObject prefab;
        [SerializeField] protected EquippableItemGO itemGO;
        [SerializeField] protected List<SpriteSwapData> spriteSwapData = new();

        //public GameObject Prefab => prefab;
        public EquippableItemGO ItemGO => itemGO;
        public float DamageBonus => damageBonus;
        public float DefenseBonus => defenseBonus;
        public List<LevelRequirement> LevelReqs => levelReqs;
        public List<SpriteSwapData> SpriteSwapData => spriteSwapData;
        public EquipmentSlot Slot => slot;
    }
}