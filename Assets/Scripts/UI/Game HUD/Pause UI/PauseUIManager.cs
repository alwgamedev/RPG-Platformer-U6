using RPGPlatformer.SceneManagement;
using UnityEngine;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class PauseUIManager : MonoBehaviour, IPausable
    {
        [SerializeField] GameObject pauseBackgroundPanel;
        [SerializeField] PauseMenu pauseMenu;
        [SerializeField] SettingsMenu settingsMenu;

        private void Awake()
        {
            //base.Awake();

            pauseMenu.SettingsButton.onClick.AddListener(settingsMenu.Show);
            settingsMenu.OnHide += pauseMenu.Show;

            Unpause();
        }

        public void Pause()
        {
            //base.Pause();
            pauseBackgroundPanel.SetActive(true);
            settingsMenu.Hide();
            //(this calls pauseMenu.Show automatically; see Awake)
            //(silly but trying to avoid any repeat calls)
        }

        public void Unpause()
        {
            pauseBackgroundPanel.SetActive(false);
        }
    }
}