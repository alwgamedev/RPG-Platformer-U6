using RPGPlatformer.UI;
using UnityEngine;

namespace RPGPlatformer.Loot
{
    [RequireComponent(typeof(ILootDrop))]
    public class LootDropRightClickMenuSpawner : InteractableGameObjectRightClickMenuSpawner
    {
        //[SerializeField] Button menuButtonPrefab;

        //ILootDrop lootDrop;

        //protected override void Awake()
        //{
        //    base.Awake();

        //    lootDrop = GetComponent<ILootDrop>();
        //}

        ////public override bool CanCreateMenu()
        ////{
        ////    return menuPrefab && menuButtonPrefab;
        ////}

        //public override void ConfigureMenu(GameObject menu)
        //{
        //    CreateAndConfigureButton(menu, menuButtonPrefab, 
        //        $"Search <b>{lootDrop.DisplayName}</b>", lootDrop.Search);
        //    CreateAndConfigureButton(menu, menuButtonPrefab, 
        //        $"Examine <b>{lootDrop.DisplayName}</b>", lootDrop.Examine);
        //    CreateAndConfigureButton(menu, menuButtonPrefab, "Cancel", ClearMenu);
        //}
    }
}