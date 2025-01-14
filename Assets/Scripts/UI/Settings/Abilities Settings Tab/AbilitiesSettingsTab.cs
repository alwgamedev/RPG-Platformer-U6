using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Combat;
using System.Collections.Generic;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class AbilitiesSettingsTab : SettingsTab
    {
        [SerializeField] Button mageButton;
        [SerializeField] Button meleeButton;
        [SerializeField] Button rangedButton;
        [SerializeField] Button unarmedButton;
        [SerializeField] AbilityBarUI displayBar;

        CombatStyle? currentlySelectedCombatStyle;

        Dictionary<CombatStyle, AbilityBar> CurrentBars = new()
        {
            [CombatStyle.Mage] = null,
            [CombatStyle.Melee] = null,
            [CombatStyle.Ranged] = null,
            [CombatStyle.Unarmed] = null
        };

        protected override void Awake()
        {
            base.Awake();

            mageButton.onClick.AddListener(() => SelectCombatStyle(CombatStyle.Mage));
            meleeButton.onClick.AddListener(() => SelectCombatStyle(CombatStyle.Melee));
            rangedButton.onClick.AddListener(() => SelectCombatStyle(CombatStyle.Ranged));
            unarmedButton.onClick.AddListener(() => SelectCombatStyle(CombatStyle.Unarmed));
        }

        public override void LoadDefaultSettings()
        {
            if (!currentlySelectedCombatStyle.HasValue) return;

            List<AbilityBarItem> items = currentlySelectedCombatStyle.HasValue ?
                AbilityTools.DefaultAbilityBarItems(currentlySelectedCombatStyle.Value) : new();
            displayBar.DisplayAbilityBar(new(null, items), false);
        }

        public override void Redraw()
        {
            currentlySelectedCombatStyle = null;
            RebuildDictionary();
            RedrawAbilityBar();
        }

        public override bool TrySaveTab(out string resultMessage)
        {
            resultMessage = "";
            return true;
        }

        private void SelectCombatStyle(CombatStyle combatStyle)
        {
            UpdateStoredAbilityBars();
            currentlySelectedCombatStyle = combatStyle;
            RedrawAbilityBar();
        }

        private void UpdateStoredAbilityBars()
        {
            if (currentlySelectedCombatStyle.HasValue)
            {
                CurrentBars[currentlySelectedCombatStyle.Value] = displayBar.AbilityBar;
            }
        }

        private void RedrawAbilityBar()
        {
            if (currentlySelectedCombatStyle.HasValue)
            {
                foreach (var entry in CurrentBars)
                {
                    if (entry.Key == currentlySelectedCombatStyle.Value)
                    {
                        displayBar.DisplayAbilityBar(CurrentBars[currentlySelectedCombatStyle.Value], false);
                        return;
                    }
                }
            }

            displayBar.DisplayAbilityBar(null, false);
        }

        private void RebuildDictionary()
        {
            if (SettingsManager.Instance == null) return;

            CurrentBars = SettingsManager.Instance.PlayerAbilityBars.BuildAllAbilityBars();
        }
    }
}
