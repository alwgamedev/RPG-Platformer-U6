using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class CollapsableUI : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] Button collapseButton;
        [SerializeField] bool openOnStart;

        bool isOpen;

        private void Start()
        {
            collapseButton.onClick.AddListener(ToggleOpen);
            SetOpen(openOnStart);
        }

        public void SetOpen(bool val)
        {
            content.SetActive(val);
            isOpen = val;
        }

        public float ContentWidth()
        {
            return ((RectTransform)content.transform).rect.width;
        }

        private void ToggleOpen()
        {
            SetOpen(!isOpen);
        }

        private void OnDestroy()
        {
            collapseButton.onClick.RemoveAllListeners();
        }
    }
}