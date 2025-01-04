using RPGPlatformer.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class PauseMenu : HideableUI
    {
        [SerializeField] Button settingsButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button quitButton;

        protected override void OnEnable()
        {
            base.OnEnable();

            resumeButton.onClick.AddListener(Resume);
        }

        protected virtual void Resume()
        {
            PauseManager pm = FindAnyObjectByType<PauseManager>();
            if(pm)
            {
                pm.Unpause();
            }
        }

        protected override void OnDestroy()
        {
            foreach(var b in GetComponentsInChildren<Button>())
            {
                b.onClick.RemoveAllListeners();
            }
        }
    }
}