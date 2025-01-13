using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class SettingsMenu : TabMenu
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button saveTabButton;
        [SerializeField] Button saveAllButton;
        //[SerializeField] Button openTabButton;
        [SerializeField] HidableUI popupPanel;
        [SerializeField] PopupWindow popupWindowPrefab;
        //[SerializeField] PopupWindow saveResultPopup;

        Dictionary<string, SettingsTab> SettingsTabLookup = new();

        //GameObject videoPanel;
        //GameObject audioPanel;
        //InputSettingsUI inputPanel;
        //To-do: maybe each tab is just an "ISettingsTab" (that you can get
        //via TabLookup[name].content.GetComp<ISettingsTab>) and ISettingsTab has a "CanSave" method
        //that either returns success or a failure message to display in the popup
        //PopupWindow saveResultPopup;

        //public Button CloseButton => closeButton;
        //public Button SaveButton => saveButton;

        protected override void Awake()
        {
            base.Awake();

            foreach (var entry in TabLookup)
            {
                var st = entry.Value.Content.GetComponentInChildren<SettingsTab>();
                if (st != null)
                { 
                    SettingsTabLookup[entry.Key] = st; 
                }
            }

            saveTabButton.onClick.AddListener(SaveOpenTab);
            saveAllButton.onClick.AddListener(SaveAllTabs);

            OnShow += ClosePopupPanel;
            OnShow += Redraw;

            OnHide += ClosePopupPanel;
            closeButton.onClick.AddListener(CloseMenu);
        }

        private void Redraw()
        {
            foreach(var entry in SettingsTabLookup)
            {
                if(entry.Value != null)
                {
                    entry.Value.Redraw();
                }
            }
        }

        private void SaveOpenTab()
        {
            if(openTab != null && SettingsTabLookup.TryGetValue(openTab, out var settingsTab)
                && settingsTab != null)
            {
                if(settingsTab.TrySaveTab(out string resultMessage))
                {
                    SpawnSaveSuccessPopup();
                }
                else
                {
                    SpawnSaveFailurePopup(resultMessage);
                }
            }
        }

        private void SaveAllTabs()
        {
            foreach(var entry in SettingsTabLookup)
            {
                if(!entry.Value.TrySaveTab(out var failureMessage))
                {
                    string msg = $"<b>{entry.Key}:</b> {failureMessage}";
                    SpawnSaveFailurePopup(msg);
                    return;
                }
            }

            SpawnSaveSuccessPopup();
        }

        private void SpawnSaveSuccessPopup()
        {
            SpawnPopup("Success!", "Your settings have been saved.");
        }

        private void SpawnSaveFailurePopup(string failureMessage)
        {
            SpawnPopup("Settings not saved", failureMessage);
        }

        private void SpawnUnsavedChangesPopup()
        {
            var popup = SpawnPopup("You have unsaved changes", "Are you sure you want to exit?");
            var yesButton = popup.AddButton("Yes");
            var noButton = popup.AddButton("No");
            yesButton.onClick.AddListener(Hide);
            noButton.onClick.AddListener(ClosePopupPanel);
        }

        private PopupWindow SpawnPopup(string title, string message)
            //return the popup in case e.g. we want to add yes/no buttons to it
        {
            var popup = Instantiate(popupWindowPrefab, popupPanel.transform);
            popup.ClearButtons();
            popup.SetTitle(title);
            popup.SetContent(message);
            popup.CloseButton.onClick.AddListener(ClosePopupPanel);
            popupPanel.Show();
            return popup;
        }

        private void CloseMenu()
        {
            foreach(var entry in SettingsTabLookup)
            {
                if(entry.Value != null & entry.Value.HasUnsavedChanges)
                {
                    SpawnUnsavedChangesPopup();
                    return;
                }
            }

            Hide();
        }

        private void ClosePopupPanel()
        {
            foreach(var popup in popupPanel.GetComponentsInChildren<PopupWindow>(true))
            {
                Destroy(popup.gameObject);
            }
            popupPanel.Hide();
        }
    }
}