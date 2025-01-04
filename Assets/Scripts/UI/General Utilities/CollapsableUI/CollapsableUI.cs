using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class CollapsableUI : MonoBehaviour
    {
        [SerializeField] GameObject content;
        [SerializeField] Button collapseButton;
        [SerializeField] bool openOnStart;

        private void Start()
        {
            SetOpen(openOnStart);
            collapseButton.onClick.AddListener(ToggleOpen);
        }

        public void SetOpen(bool val)
        {
            content.SetActive(val);
        }

        public float ContentWidth()
        {
            return ((RectTransform)content.transform).rect.width;
        }

        private void ToggleOpen()
        {
            SetOpen(!content.activeSelf);
        }

        private void OnDestroy()
        {
            collapseButton.onClick.RemoveAllListeners();
        }
    }
}