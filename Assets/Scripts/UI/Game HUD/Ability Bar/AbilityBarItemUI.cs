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
        [SerializeField] protected Image abilityIcon;
        [SerializeField] protected Image cooldownFillImage;
        [SerializeField] protected TextMeshProUGUI keybindText;
        [SerializeField] protected GameObject autoCastCheckmark;

        protected int? abilityBarIndex = null;

        public AbilityBarItem AbilityBarItem { get; private set; }

        protected virtual void Awake()
        {
            SettingsManager.OnIAMConfigure += DisplayKeybind;
        }

        public virtual void Configure(AbilityBarItem abilityBarItem, int abilityBarIndex, float initialCooldownTime)
        {
            AbilityBarItem = abilityBarItem;
            this.abilityBarIndex = abilityBarIndex;
            DisplayAutoCastCheckmark(abilityBarItem?.includeInAutoCastCycle ?? false);
            DisplayKeybind();

            if(abilityBarItem == null)
            {
                SetCooldownFill(0);
                DisplayAutoCastCheckmark(false);
                SetIcon(null);
                return;
            }
            else
            {
                SetCooldownFill(abilityBarItem.ability.Cooldown != 0 ?
                    initialCooldownTime / abilityBarItem.ability.Cooldown : 1);
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
            if (!cooldownFillImage) return;

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
            if (!cooldownFillImage) yield break;
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

        private void DisplayAutoCastCheckmark(bool val)
        {
            if (autoCastCheckmark)
            {
                autoCastCheckmark.SetActive(val);
            }
        }

        private void SetCooldownFill(float fillAmount)
        {
            if (cooldownFillImage)
            {
                cooldownFillImage.fillAmount = fillAmount;
            }
        }

        private void DisplayKeybind()
        {
            if (!keybindText) return;

            if (abilityBarIndex.HasValue)
            {
                keybindText.text = GetKeybind(abilityBarIndex.Value);
            }
        }

        private string GetKeybind(int abilityIndex)
        {
            if(SettingsManager.Instance != null && SettingsManager.Instance.InputSettings.abilityBarBindingPaths
                .TryGetValue(abilityIndex, out var bindingPath))
            {
                return InputTools.KeyName(bindingPath).ToUpper();
            }
            return "";
        }

        protected virtual void OnDestroy()
        {
            SettingsManager.OnIAMConfigure -= DisplayKeybind;
        }
    }
}