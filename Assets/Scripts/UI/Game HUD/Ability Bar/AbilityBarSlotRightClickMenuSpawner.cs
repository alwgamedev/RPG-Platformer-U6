using RPGPlatformer.Combat;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class AbilityBarSlotRightClickMenuSpawner : RightClickMenuSpawner
    {
        [SerializeField] Button menuButtonPrefab;

        AbilityBarSlot slotComponent;

        protected override void Awake()
        {
            base.Awake();

            slotComponent = GetComponent<AbilityBarSlot>();
        }

        public override bool CanCreateMenu()
        {
            return slotComponent.AbilityBarItem?.Ability != null && menuPrefab & menuButtonPrefab;
        }

        public override void ConfigureMenu(GameObject menu)
        {
            var item = slotComponent.AbilityBarItem;

            var toggleAction = item.IncludeInAutoCastCycle ?
                "Remove from auto-cast cycle" : "Include in auto-cast cycle";
            if (!item.Ability.IsAsyncAbility)
            {
                CreateAndConfigureButton(menu, menuButtonPrefab, toggleAction,
                slotComponent.ToggleAbilityIncludedInAutoCastCycle);
            }
            CreateAndConfigureButton(menu, menuButtonPrefab, "Remove ability",
                slotComponent.RemoveItemWithNotification);
            CreateAndConfigureButton(menu, menuButtonPrefab, "Cancel", ClearMenu);
        }
        
    }
}
