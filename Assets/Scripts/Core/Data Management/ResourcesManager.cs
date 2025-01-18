using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Skills;
using RPGPlatformer.Inventory;


namespace RPGPlatformer.Core
{
    //GlobalGameTools will hold an instance of this
    [Serializable]
    public class ResourcesManager
    {
        [SerializeField] AbilityResourceContainerSO abilityResources;

        public Dictionary<CharacterSkill, Sprite> CircularSkillIcons = new();

        public AbilityResourceContainerSO AbilityResources => abilityResources;

        public void InitializeResources()
        {
            UpdateInventoryItemSOLookup();
            LoadCircularSkillIcons();
        }

        public void UpdateInventoryItemSOLookup()
        {
            InventoryItemSO[] invItems = Resources.LoadAll<InventoryItemSO>("");
            foreach (InventoryItemSO invItem in invItems)
            {
                invItem.UpdateLookup();
            }
        }

        public void LoadCircularSkillIcons()
        {
            CircularSkillIcons = new()
            {
                [CharacterSkillBook.Fitness] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular health icon"),
                [CharacterSkillBook.Defense] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular defense icon"),
                [CharacterSkillBook.Magic] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular magic icon"),
                [CharacterSkillBook.Melee] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular melee icon"),
                [CharacterSkillBook.Ranged] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular range icon")
            };
        }
    }
}