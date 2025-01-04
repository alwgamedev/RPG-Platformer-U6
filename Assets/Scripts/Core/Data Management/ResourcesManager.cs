using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Skills;
using RPGPlatformer.Inventory;


namespace RPGPlatformer.Core
{
    //to be attached to Global Game Tools singleton (so we don't need any singleton/destroy stuff here)
    public class ResourcesManager
    {
        public Dictionary<CharacterSkill, Sprite> CircularSkillIcons = new();

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
                [CharacterSkillBook.Health] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular health icon"),
                [CharacterSkillBook.Defense] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular defense icon"),
                [CharacterSkillBook.Magic] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular magic icon"),
                [CharacterSkillBook.Melee] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular melee icon"),
                [CharacterSkillBook.Range] = Resources.Load<Sprite>("UI Resources/Skill Icons/Circular/circular range icon")
            };
        }
    }
}