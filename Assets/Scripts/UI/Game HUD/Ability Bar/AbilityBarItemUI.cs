using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGPlatformer.Combat;

namespace RPGPlatformer.UI
{
    public class AbilityBarItemUI : MonoBehaviour, IAbilityBarSlot
    {
        public Image fillImage;
        public TextMeshProUGUI nameTMP;

        public AbilityBarItem AbilityBarItem { get; private set; }

        public void Configure(AbilityBarItem abilityBarItem, float initialCooldownTime)
        {
            if(abilityBarItem?.ability == null)
            {
                Destroy(gameObject);
                return;
            }
            AbilityBarItem = abilityBarItem;
            nameTMP.text = abilityBarItem.ability.GetAbilityName();
            fillImage.fillAmount = abilityBarItem.ability.Cooldown != 0 ? initialCooldownTime / abilityBarItem.ability.Cooldown : 1;
        }

        public void StartCooldown()
        {
            if (AbilityBarItem.ability.Cooldown == 0)
            {
                return;
            }
            fillImage.fillAmount = 1;
            StartCoroutine(PlayCooldownAnimation());
        }

        public IEnumerator PlayCooldownAnimation()
        {
            while (fillImage.fillAmount > 0)
            {
                yield return null;
                fillImage.fillAmount -= Time.deltaTime / AbilityBarItem.ability.Cooldown;
            }
        }
    }
}