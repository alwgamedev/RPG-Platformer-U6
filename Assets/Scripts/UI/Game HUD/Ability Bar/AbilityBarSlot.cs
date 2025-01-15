using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class AbilityBarSlot : MonoBehaviour, IAbilityBarSlot, IDragDropSlot<AbilityBarItem>
    {
        [SerializeField] protected Image abilityIcon;
        [SerializeField] protected Image cooldownFillImage;
        [SerializeField] protected TextMeshProUGUI keybindText;
        [SerializeField] protected GameObject autoCastCheckmark;

        public Func<AttackAbility, bool> ValidAbility = a => true;

        protected int? abilityBarIndex = null;

        protected bool[] acceptedCombatStyles = new bool[Enum.GetValues(typeof(CombatStyle)).Length];

        public AbilityBarItem AbilityBarItem { get; protected set; }
        public Transform Transform => transform;
        public bool AllowReplacementIfCantSwap => true;

        public event Action OnDragResolved;

        protected virtual void Awake()
        {
            SettingsManager.OnIAMConfigure += DisplayKeybind;
        }


        //DRAG/DROP FUNCTIONS

        public bool CanPlace(AbilityBarItem item)
        {
            return item?.Ability == null 
                || (CanAcceptCombatStyle(item.Ability.CombatStyle) && ValidAbility(item.Ability));
        }

        public bool CanAcceptCombatStyle(CombatStyle combatStyle)
        {
            return acceptedCombatStyles[(int)combatStyle];
        }

        public AbilityBarItem Contents()
        {
            return AbilityBarItem;
        }

        public void DragComplete()
        {
            OnDragResolved?.Invoke();
        }

        public void DropComplete()
        {
            OnDragResolved?.Invoke();
        }

        public void PlaceItem(AbilityBarItem item)
        {
            StopAllCoroutines();
            AbilityBarItem = item;
        }

        public void RemoveItem()
        {
            StopAllCoroutines();
            AbilityBarItem = null;
        }


        //SET-UP

        public void Configure(int? abilityBarIndex, IEnumerable<CombatStyle> acceptedCombatStyles)
        {
            this.abilityBarIndex = abilityBarIndex;
            SetAcceptedCombatStyles(acceptedCombatStyles);
        }

        public void DisplayItem(float initialCooldownTime)
        {
            SetIcon(AbilityBarItem?.Ability);
            DisplayAutoCastCheckmark(AbilityBarItem?.IncludeInAutoCastCycle ?? false);
            DisplayKeybind();
            InitializeCooldown(initialCooldownTime);
        }

        public void SetAcceptedCombatStyles(IEnumerable<CombatStyle> combatStyles)
        {
            acceptedCombatStyles = new bool[Enum.GetValues(typeof(CombatStyle)).Length];

            if (combatStyles == null) return;

            foreach (var style in combatStyles)
            {
                acceptedCombatStyles[(int)style] = true;
            }
        }

        public void InitializeCooldown(float initialCooldownTime)
        {
            if (!cooldownFillImage) return;
            if (AbilityBarItem?.Ability == null)
            {
                SetCooldownFill(0); 
                return;
            }
            float fillAmount = AbilityBarItem.Ability.Cooldown != 0 ?
                    initialCooldownTime / AbilityBarItem.Ability.Cooldown : 0;
            SetCooldownFill(fillAmount);
            if(fillAmount > 0)
            {
                StartCoroutine(PlayCooldownAnimation());
            }
        }

        private Sprite GetIcon(AttackAbility ability)
        {
            if (ability != null && AbilityTools.TryGetResources(ability, out var resources))
            {
                return resources.AbilityIcon;
            }
            return null;
        }

        private void SetIcon(AttackAbility ability)
        {
            SetIcon(GetIcon(ability));
        }

        private void SetIcon(Sprite sprite)
        {
            abilityIcon.sprite = sprite;
            abilityIcon.color = sprite == null ? Color.black : Color.white;
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
            else
            {
                keybindText.text = "";
            }
        }

        private string GetKeybind(int abilityIndex)
        {
            if (SettingsManager.Instance != null && SettingsManager.Instance.InputSettings.abilityBarBindingPaths
                .TryGetValue(abilityIndex, out var bindingPath))
            {
                return InputTools.KeyName(bindingPath).ToUpper();
            }
            return "";
        }


        //COOLDOWN FUNCTIONS

        public void StartCooldown()
        {
            if (!cooldownFillImage) return;

            if (AbilityBarItem.Ability.Cooldown == 0)
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
                cooldownFillImage.fillAmount -= Time.deltaTime / AbilityBarItem.Ability.Cooldown;
            }
        }

        protected virtual void OnDestroy()
        {
            SettingsManager.OnIAMConfigure -= DisplayKeybind;
        }

    }
}