using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Combat;
using System.Collections.Generic;
using RPGPlatformer.Core;
using System.Linq;

namespace RPGPlatformer.UI
{
    public class AbilitiesSettingsTab : SettingsTab
    {
        [SerializeField] ObscurableButton mageButton;
        [SerializeField] ObscurableButton meleeButton;
        [SerializeField] ObscurableButton rangedButton;
        [SerializeField] ObscurableButton unarmedButton;
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

            mageButton.Button.onClick.AddListener(() => SelectCombatStyle(CombatStyle.Mage));
            meleeButton.Button.onClick.AddListener(() => SelectCombatStyle(CombatStyle.Melee));
            rangedButton.Button.onClick.AddListener(() => SelectCombatStyle(CombatStyle.Ranged));
            unarmedButton.Button.onClick.AddListener(() => SelectCombatStyle(CombatStyle.Unarmed));

            displayBar.AbilityBarChanged += OnAbilityBarChanged;
        }

        private void Start()
        {
            UpdateAbilityBarUI();
        }

        public override void LoadDefaultSettings()
        {
            if (!currentlySelectedCombatStyle.HasValue) return;

            List<AbilityBarItem> items = currentlySelectedCombatStyle.HasValue ?
                AbilityTools.DefaultAbilityBarItems(currentlySelectedCombatStyle.Value) : new();
            displayBar.ConnectAbilityBar(new(null, items), 
                AbilityBarUI.DefaultAcceptedCombatStyles(currentlySelectedCombatStyle));
        }

        public override void Redraw()
        {
            DeselectCombatStyle();
            GetCurrentPlayerBars();
            UpdateAbilityBarUI();
        }

        public override bool TrySaveTab(out string resultMessage)
        {
            UpdateStoredAbilityBars();
            resultMessage = "";

            if(SettingsManager.Instance == null)
            {
                Debug.LogWarning("Can't save SettingsTab because SettingsManager doesn't have an instance.");
                return false;
            }

            Dictionary<CombatStyle, List<AbilityBarItem>> itemsLookup = new();
            foreach (var entry in CurrentBars)
            {
                itemsLookup[entry.Key] = entry.Value.AbilityBarItems;
            }

            SerializableCharacterAbilityBarData data = new(itemsLookup);
            SettingsManager.Instance.SetPlayerAbilityBars(data);
            return true;
        }

        private void SelectCombatStyle(CombatStyle combatStyle)
        {
            UpdateStoredAbilityBars();
            currentlySelectedCombatStyle = combatStyle;
            UpdateAbilityBarUI();
        }

        private void DeselectCombatStyle()
        {
            currentlySelectedCombatStyle = null;
            mageButton.OnClicked(false);
            meleeButton.OnClicked(false);
            rangedButton.OnClicked(false);
            unarmedButton.OnClicked(false);
        }

        private void UpdateStoredAbilityBars()
        {
            if (currentlySelectedCombatStyle.HasValue)
            {
                CurrentBars[currentlySelectedCombatStyle.Value] = displayBar.AbilityBar;
            }

            HasUnsavedChanges = false;
        }

        private void UpdateAbilityBarUI()
        {
            if (currentlySelectedCombatStyle.HasValue)
            {
                foreach (var entry in CurrentBars)
                {
                    if (entry.Key == currentlySelectedCombatStyle.Value)
                    {
                        displayBar.ConnectAbilityBar(CurrentBars[currentlySelectedCombatStyle.Value],
                            AbilityBarUI.DefaultAcceptedCombatStyles(currentlySelectedCombatStyle));
                        return;
                        //displayBar.DisplayAbilityBar(CurrentBars[currentlySelectedCombatStyle.Value], false);
                        //return;
                    }
                }
            }
            else
            {
                displayBar.ConnectAbilityBar(null, new List<CombatStyle>());
            }
        }

        private void GetCurrentPlayerBars()
        {
            if (SettingsManager.Instance == null) return;

            CurrentBars = SettingsManager.Instance.PlayerAbilityBars.BuildAllAbilityBars();
        }

        private void OnAbilityBarChanged()
        {
            HasUnsavedChanges = true;
        }
    }
}
