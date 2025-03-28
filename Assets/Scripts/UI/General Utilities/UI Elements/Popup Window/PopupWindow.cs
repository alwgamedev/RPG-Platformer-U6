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

        public event Action Destroyed;

        protected override void Awake()
        {
            base.Awake();
            
            if(closeButton != null)//should we destroy instead of hide?
            {
                //closeButton.onClick.AddListener(Hide);
                closeButton.onClick.AddListener(() => Destroy(gameObject));
            }
        }

        public void Configure(string title, string content, bool clearButtonsContainer = true)
        {
            SetTitle(title);
            SetContent(content);
            if (clearButtonsContainer)
            {
                ClearButtons();
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

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Destroyed?.Invoke();
            Destroyed = null;
        }
    }
}