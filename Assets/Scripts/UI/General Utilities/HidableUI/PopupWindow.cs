using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class PopupWindow : HidableUI
    {
        [SerializeField] Button closeButton;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI content;
        [SerializeField] Button buttonPrefab;
        [SerializeField] Transform buttonContainer;

        public Button CloseButton => closeButton;

        protected override void Awake()
        {
            base.Awake();
            
            if(closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void SetTitle(string text)
        {
            if(title != null)
            {
                title.text = text ?? "";
            }
        }

        public void SetContent(string text)
        {
            if(content != null)
            {
                content.text = text ?? "";
            }
        }

        public Button AddButton(string text)
        {
            var b = Instantiate(buttonPrefab, buttonContainer.transform);
            b.GetComponentInChildren<TextMeshProUGUI>().text = text;
            return b;
        }

        public void ClearButtons()
        {
            if (buttonContainer == null) return;
            foreach (var b in buttonContainer.GetComponentsInChildren<Button>())
            {
                b.onClick.RemoveAllListeners();
                Destroy(b.gameObject);
            }
        }
    }
}