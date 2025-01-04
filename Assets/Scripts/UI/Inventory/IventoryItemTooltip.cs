using TMPro;
using UnityEngine;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.UI
{
    public class InventoryItemTooltip : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI bodyText;

        public virtual void Configure(InventoryItem item)
        {
            titleText.text = item.BaseData.DisplayName;
            bodyText.text = item.TooltipText();
        }
    }
}