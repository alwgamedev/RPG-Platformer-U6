using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public abstract class SettingsTab : MonoBehaviour
    {
        [SerializeField] protected Button loadDefaultSettingsButton;

        public bool HasUnsavedChanges { get; protected set; }

        protected virtual void Awake()
        {
            if(loadDefaultSettingsButton)
            {
                loadDefaultSettingsButton.onClick.AddListener(LoadDefaultSettings);
            }
        }

        public abstract void Redraw();

        public abstract void LoadDefaultSettings();

        public abstract bool TrySaveTab(out string resultMessage);

        protected virtual void OnDestroy()
        {
            loadDefaultSettingsButton.onClick.RemoveAllListeners();
        }
    }
}