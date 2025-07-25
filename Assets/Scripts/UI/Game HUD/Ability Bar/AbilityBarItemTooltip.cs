﻿using RPGPlatformer.Combat;
using System;
using TMPro;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class AbilityBarItemTooltip : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] TextMeshProUGUI abilityTagsText;
        [SerializeField] TextMeshProUGUI statsText;
        [SerializeField] Color plusStatColor = Color.blue;
        [SerializeField] Color minusStatColor = Color.red;

        public void Configure(AttackAbility ability)
        {
            Clear();

            if (ability == null) return;

            titleText.text = ability.GetAbilityName();

            if (ability.Description != null)
            {
                descriptionText.text += ability.Description;
            }

            if(ability.AbilityTags != null)
            {
                abilityTagsText.text += "<b>Tags:</b> ";
                for (int i = 0; i < ability.AbilityTags.Count; i++)
                {
                    abilityTagsText.text += ability.AbilityTags[i];
                    if(i < ability.AbilityTags.Count - 1)
                    {
                        abilityTagsText.text += ", ";
                    }
                }
            }

            statsText.text += $"<b>Cooldown:</b> {ability.Cooldown:0.##}s";
            statsText.text += "\n" + DamageStatText(ability);
            if(ability.StunDuration != null)
            {
                statsText.text += $"\n<b>Stun Duration:</b> {ability.StunDuration:0.##}s";
            }
            statsText.text += $"\n<b>Stamina:</b> {FormattedStatChangeText(ability.StaminaFractionChange)}";
            statsText.text += $"\n<b>Wrath:</b> {FormattedStatChangeText(ability.WrathFractionChange)}";
        }

        public string DamageStatText(AttackAbility ability)
        {
            string text = "";
            if (ability.BleedCount == 0)
            {
                text += $"<b>Damage Multiplier:</b> {ability.DamageMultiplier:0.##}x";
            }
            if (ability.BleedCount > 0)
            {
                float totalDamageMultiplier = 0;
                for (int i = 0; i < ability.BleedCount; i++)
                {
                    totalDamageMultiplier += ability.DamagePerBleedIteration(i, ability.DamageMultiplier);
                }
                text += $"<b>Damage Multiplier:</b> {totalDamageMultiplier:0.##}x total over {ability.BleedCount} hits";
            }
            if (ability is IPowerUpAbility powerUp)
            {
                text += $"\n<b>Max. Power-Up:</b> {powerUp.MaxPowerUpMultiplier:0.##}x";
            }
            return text;
        }

        public string FormattedStatChangeText(float fractionChange)//for wrath and stamina texts
        {
            float percent = fractionChange * 100;
            //silly but (int)(-0.01 * 100) was coming out as 0
            if(fractionChange >= 0)
            {
                return $"<color=#{ColorUtility.ToHtmlStringRGB(plusStatColor)}>+{percent:0.#}%</color>";
            }
            return $"<color=#{ColorUtility.ToHtmlStringRGB(minusStatColor)}>{percent:0.#}%</color>";
            //to-do: the minus sign is not showing up correctly (even when use unicode symbol \u2212).
            //possibly the problem is fixed in Unity versions with LTS
        }



        public void Clear()
        {
            foreach (var text in GetComponentsInChildren<TextMeshProUGUI>())
            {
                text.text = "";
            }
        }
    }
}