using TMPro;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class CombatantTooltipSpawner : TooltipSpawner
    {
        public override void ConfigureTooltip(GameObject tooltip)
        {
            TextMeshProUGUI text = tooltip.GetComponent<TextMeshProUGUI>();
        }
    }
}