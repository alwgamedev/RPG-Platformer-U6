using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class PauseManager : MonoBehaviour
    {
        public bool GamePaused { get; private set; }

        public event Action OnPause;
        public event Action OnUnpause;

        private void OnEnable()
        {
            SettingsManager.OnIBMConfigure += OnIBMConfigured;
        }

        private void OnIBMConfigured()
        {
            SettingsManager.Instance.IBM.EscAction.started += (context) => TogglePause();

            SettingsManager.OnIBMConfigure -= OnIBMConfigured;
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
            if(GamePaused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }

        private void OnDestroy()
        {
            OnPause = null;
            OnUnpause = null;
        }
    }
}
