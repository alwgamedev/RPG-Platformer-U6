using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPGPlatformer.UI
{
    public class StatBarItem : MonoBehaviour
    {
        [SerializeField] Image fillImage;
        [SerializeField] TextMeshProUGUI pointsText;

        public void SetFillAmount(float amount)
        {
            if (fillImage)
            {
                fillImage.fillAmount = amount;
            }
        }

        public void SetText(string text)
        {
            if (pointsText)
            {
                pointsText.text = text;
            }
        }
    }
}