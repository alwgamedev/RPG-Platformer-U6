using UnityEngine;

namespace RPGPlatformer.UI
{
    public class PauseUI : HidableUI
    {
        [SerializeField] PauseMenu pauseMenu;
        [SerializeField] SettingsMenu settingsMenu;

        protected override void Awake()
        {
            base.Awake();

            pauseMenu.SettingsButton.onClick.AddListener(settingsMenu.Show);
            settingsMenu.OnHide += pauseMenu.Show;
        }

        public override void Pause()
        {
            base.Pause();

            settingsMenu.Hide();
            //(this calls pauseMenu.Show automatically; see Awake)
            //(silly but trying to avoid any repeat calls)
        }
    }
}