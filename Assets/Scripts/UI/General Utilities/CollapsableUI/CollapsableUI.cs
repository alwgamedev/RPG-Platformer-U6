using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class CollapsableUI : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] Button collapseButton;
        [SerializeField] bool openOnStart;

        Animation highlightAnimation;

        public bool IsOpen { get; private set; }
        public Button CollapseButton => collapseButton;

        private void Awake()
        {
            highlightAnimation = collapseButton.GetComponentInChildren<Animation>();
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

        public void HighlightFlash()
        {
            highlightAnimation.Play();
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