using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class AbilityBarItemUI : MonoBehaviour, IAbilityBarSlot
    {
        [SerializeField] Image abilityIcon;
        //[SerializeField] TextMeshProUGUI nameTMP;
        [SerializeField] Image cooldownFillImage;
        [SerializeField] TextMeshProUGUI keybindText;
        [SerializeField] GameObject autoCastCheckmark;

        int? abilityBarIndex = null;

        public AbilityBarItem AbilityBarItem { get; private set; }

        private void Awake()
        {
            SettingsManager.OnIAMConfigure += DisplayKeybind;
        }

        public void Configure(AbilityBarItem abilityBarItem, int abilityBarIndex, float initialCooldownTime)
        {
            AbilityBarItem = abilityBarItem;
            this.abilityBarIndex = abilityBarIndex;
            autoCastCheckmark.SetActive(abilityBarItem?.includeInAutoCastCycle ?? false);
            DisplayKeybind();

            if(abilityBarItem == null)
            {
                cooldownFillImage.enabled = false;
                autoCastCheckmark.SetActive(false);
                SetIcon(null);
                return;
            }
            else
            {
                cooldownFillImage.fillAmount = abilityBarItem.ability.Cooldown != 0 ?
                    initialCooldownTime / abilityBarItem.ability.Cooldown : 1;
                if (AbilityTools.TryGetResources(abilityBarItem.ability, out var resources))
                {
                    SetIcon(resources.AbilityIcon);
                }
                else
                {
                    SetIcon(null);
                }
            }
        }

        public void StartCooldown()
        {
            if (AbilityBarItem.ability.Cooldown == 0)
            {
                cooldownFillImage.fillAmount = 0;
                return;
            }
            cooldownFillImage.fillAmount = 1;
            StartCoroutine(PlayCooldownAnimation());
        }

        public IEnumerator PlayCooldownAnimation()
        {
            while (cooldownFillImage.fillAmount > 0)
            {
                yield return null;
                cooldownFillImage.fillAmount -= Time.deltaTime / AbilityBarItem.ability.Cooldown;
            }
        }

        private void SetIcon(Sprite sprite)
        {
            if(sprite == null)
            {
                abilityIcon.sprite = null;
                abilityIcon.color = Color.black;
            }
            else
            {
                abilityIcon.sprite = sprite;
                abilityIcon.color = Color.white;
            }
        }

        private void DisplayKeybind()
        {
            if (abilityBarIndex.HasValue)
            {
                keybindText.text = GetKeybind(abilityBarIndex.Value);
            }
        }

        private string GetKeybind(int abilityIndex)
        {
            if(SettingsManager.Instance != null && SettingsManager.Instance.CurrentBindings.abilityBarBindingPaths
                .TryGetValue(abilityIndex, out var bindingPath))
            {
                return InputTools.KeyName(bindingPath).ToUpper();
            }
            return "";
        }

        private void OnDestroy()
        {
            SettingsManager.OnIAMConfigure -= DisplayKeybind;
        }
    }
}