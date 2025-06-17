using RPGPlatformer.Core;
using RPGPlatformer.UI;
using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class PauseManager : MonoBehaviour
    {
        bool pauseCanBeToggled = true;

        public bool GamePaused { get; private set; }

        public event Action OnPause;
        public event Action OnUnpause;

        public static PauseManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (SettingsManager.Instance && SettingsManager.Instance.IAMIsConfigured)
                {
                    OnIAMConfigured();
                }
                SettingsManager.IAMConfigured += OnIAMConfigured;//still subscribe to get settings updates
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (Instance != this) return;

            SettingsMenu sm = FindAnyObjectByType<SettingsMenu>(FindObjectsInactive.Include);
            if (sm)
                //so you can't Esc to unpause once paused
                //(so you don't suddenly unpause and have any unexpected consequences with the ui)
            {
                sm.OnShow += DisableToggling;
                sm.OnHide += EnableToggling;
            }
        }

        private void OnIAMConfigured()
        {
            SettingsManager.Instance.IAM.InputAction(InputActionsManager.escActionName).started 
                += (context) => TogglePause();

            //SettingsManager.OnIAMConfigure -= OnIAMConfigured;
        }

        public void Pause()
        {
            OnPause?.Invoke();
            Time.timeScale = 0;
            GamePaused = true;
        }

        public void Unpause()
        {
            GamePaused = false;
            Time.timeScale = 1;
            OnUnpause?.Invoke();
        }

        public void TogglePause()
        {
            if (!pauseCanBeToggled)
            {
                return;
            }
            else if (GamePaused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }

        private void EnableToggling()
        {
            pauseCanBeToggled = true;
        }

        private void DisableToggling()
        {
            pauseCanBeToggled = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            OnPause = null;
            OnUnpause = null;

            SettingsManager.IAMConfigured -= OnIAMConfigured;
        }
    }
}
