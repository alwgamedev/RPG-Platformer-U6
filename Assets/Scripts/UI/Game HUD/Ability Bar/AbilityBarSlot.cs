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
        [SerializeField] protected Transform draggableParentTransform;

        public Func<AttackAbility, IDragSource<AbilityBarItem>, bool> ValidAbility = (a, o) => false;

        protected int? abilityBarIndex = null;
        protected bool[] acceptedCombatStyles = new bool[Enum.GetValues(typeof(CombatStyle)).Length];

        public AbilityBarItem AbilityBarItem { get; protected set; }
        public Transform DraggableParentTransform => draggableParentTransform;
        public virtual bool AllowReplacementIfCantSwap => true;

        public event Action OnDragResolved;
        public event Action OnItemChanged;//only used when item has been changed via RightClickMenu

        protected virtual void Awake()
        {
            SettingsManager.IAMConfigured += DisplayKeybind;
        }


        //DRAG/DROP FUNCTIONS

        //only used when dragging/dropping
        public virtual bool CanPlace(AbilityBarItem item, IDragSource<AbilityBarItem> origin = null)
        {
            if (item?.Ability == null) return false;

            return CanAcceptCombatStyle(item.Ability.CombatStyle)
                && ValidAbility(item.Ability, origin);
        }

        public bool CanAcceptCombatStyle(CombatStyle combatStyle)
        {
            return acceptedCombatStyles[(int)combatStyle];
        }

        public bool ItemCanBeDragged()
        {
            return AbilityBarItem?.Ability != null;
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


        //EDITING ITEM

        public void RemoveItemWithNotification()
        {
            RemoveItem();
            OnItemChanged?.Invoke();
        }

        public void ToggleAbilityIncludedInAutoCastCycle()
        {
            if (AbilityBarItem?.Ability == null) return;

            AbilityBarItem.SetIncludeInAutoCastCycle(!AbilityBarItem.IncludeInAutoCastCycle);
            DisplayAutoCastCheckmark(AbilityBarItem.IncludeInAutoCastCycle);
            OnItemChanged?.Invoke();
        }

        //SET-UP

        public void Configure(int? abilityBarIndex, IEnumerable<CombatStyle> acceptedCombatStyles)
        {
            SetAbilityBarIndex(abilityBarIndex);
            SetAcceptedCombatStyles(acceptedCombatStyles);
        }

        public void DisplayItem(float initialCooldownTime)
        {
            SetIcon(AbilityBarItem?.Ability);
            DisplayAutoCastCheckmark(AbilityBarItem?.IncludeInAutoCastCycle ?? false);
            DisplayKeybind();
            InitializeCooldown(initialCooldownTime);
        }

        public void SetAbilityBarIndex(int? abilityBarIndex)
        {
            this.abilityBarIndex = abilityBarIndex;
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
            if (ability == null)
            {
                abilityIcon.sprite = null;
                abilityIcon.enabled = false;
                return;
            }
            abilityIcon.enabled = true;
            SetIcon(GetIcon(ability));
            abilityIcon.color = abilityIcon.sprite == null ? Color.blue : Color.white;
            //^color TEMPORARY until we finish the ability icons
        }

        private void SetIcon(Sprite sprite)
        {
            abilityIcon.sprite = sprite;
            //abilityIcon.enabled = sprite != null;//color = sprite != null ? Color.white : Color.clear;
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
            SettingsManager.IAMConfigured -= DisplayKeybind;
            OnDragResolved = null;
            OnItemChanged = null;
        }
    }
}