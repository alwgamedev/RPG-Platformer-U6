﻿using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class AbilityBarItemTooltipSpawner : TooltipSpawner
    {
        IAbilityBarSlot abilityBarSlot;

        protected override void Awake()
        {
            base.Awake();

            abilityBarSlot = GetComponent<IAbilityBarSlot>();
        }

        public override bool CanCreateTooltip()
        {
            return abilityBarSlot.AbilityBarItem?.Ability != null && tooltipPrefab;
            //later we may want to allow these to be disabled in Settings
        }

        public override void ConfigureTooltip(GameObject tooltip)
        {
            if(tooltip.TryGetComponent<AbilityBarItemTooltip>(out var abilityTooltip))
            {
                abilityTooltip.Configure(abilityBarSlot.AbilityBarItem?.Ability);
            }
            else
            {
                Debug.Log($"{GetType().Name} attached to {name} does not have the correct " +
                    $"type of tooltip prefab installed.");
                ClearTooltipImmediate();
            }
        }

        protected override Vector3 GetPosition()
        {
            return transform.position + .55f * GetComponent<RectTransform>().WorldHeight() * Vector3.up;
            //buffer upwards, because if mouse is over tooltip when it spawns
            //we get unwanted PointerExit events causing tooltip to flicker on/off
        }
    }
}