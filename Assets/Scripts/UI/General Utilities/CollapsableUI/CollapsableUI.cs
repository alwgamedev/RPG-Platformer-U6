using RPGPlatformer.Effects;
using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class CollapsableUI : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] Button collapseButton;
        [SerializeField] bool openOnStart;

        //Animation highlightAnimation;
        TextHighlighter highlighter;

        public bool IsOpen { get; private set; }
        public Button CollapseButton => collapseButton;

        private void Awake()
        {
            //highlightAnimation = collapseButton.GetComponentInChildren<Animation>();
            highlighter = collapseButton.GetComponentInChildren<TextHighlighter>();
        }

        private void Start()
        {
            collapseButton.onClick.AddListener(ToggleOpen);
            SetOpen(openOnStart);
        }

        public void SetOpen(bool val)
        {
            content.SetActive(val);
            IsOpen = val;
        }

        public async void HighlightFlash()
        {
            await highlighter.HighlightFlash(GlobalGameTools.Instance.TokenSource.Token);
        }

        //public float ContentWidth()
        //{
        //    return ((RectTransform)content.transform).rect.width;
        //}

        private void ToggleOpen()
        {
            SetOpen(!IsOpen);
        }

        private void OnDestroy()
        {
            collapseButton.onClick.RemoveAllListeners();
        }
    }
}