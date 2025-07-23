using RPGPlatformer.Core;
using RPGPlatformer.Inventory;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.UI
{
    //includes only the in-game hud; does not include pause ui
    public class GameHUD : HidableUI
    {
        [SerializeField] HidableUI sceneFader;
        [SerializeField] float sceneFadeTime = 0.25f;

        AbilityBarUI abilityBar;
        DialogueUI dialogueUI;
        EquipmentInspectorUI equipmentInspector;
        PlayerInventoryUI playerInventory;
        XPAlertBar xpAlertBar;
        SkillsUI skillsUI;

        public static AbilityBarUI AbilityBar => Instance.abilityBar;
        public static DialogueUI DialogueUI => Instance.dialogueUI;
        public static EquipmentInspectorUI EquipmentInspector => Instance.equipmentInspector;
        public static GameLog GameLog => GameLog.Instance;
        public static PlayerInventoryUI PlayerInventory => Instance.playerInventory;
        public static SkillsUI SkillsUI => Instance.skillsUI;
        public static XPAlertBar XPAlertBar => Instance.xpAlertBar;
        public static HidableUI SceneFader => Instance.sceneFader;
        public static GameHUD Instance { get; private set; }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                base.Awake();
                Configure();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Configure()
        {
            abilityBar = GetComponentInChildren<AbilityBarUI>(true);
            dialogueUI = GetComponentInChildren<DialogueUI>(true);
            equipmentInspector = GetComponentInChildren<EquipmentInspectorUI>(true);
            playerInventory = GetComponentInChildren<PlayerInventoryUI>(true);
            xpAlertBar = GetComponentInChildren<XPAlertBar>(true);
            skillsUI = GetComponentInChildren<SkillsUI>(true);
        }

        public static void GiftPlayerLoot(IInventorySlotDataContainer loot, string message = null, 
            bool forceOpenInventory = false, bool handleOverflow = true)
        {
            if (loot?.Item == null || loot.Quantity <= 0)
            {
                Debug.LogWarning("Unable to gift player loot -- loot missing." +
                    $" Was loot null? {loot == null}. Was Item null? {loot?.Item == null}.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                GameLog.Log(message);
            }
            if (!string.IsNullOrWhiteSpace(loot.Item.BaseData.DisplayName))
            {
                GameLog.Log($"{loot.Item.BaseData.DisplayName} has been placed in your inventory.");
            }

            if (forceOpenInventory && !PlayerInventory.CollapsableUI.IsOpen)
            {
                PlayerInventory.CollapsableUI.SetOpen(true);
            }
            PlayerInventory.CollapsableUI.HighlightFlash();
            GlobalGameTools.Instance.PlayerLooter.TakeLoot(loot, handleOverflow);
        }

        public static async Task FadeSceneOut()
        {
            if (!Instance || !SceneFader)
            {
                Debug.Log("no scene fader");
                return;
            }
            await SceneFader.FadeShow(Instance.sceneFadeTime, GlobalGameTools.Instance.TokenSource.Token);
        }

        public static async Task FadeSceneIn()
        {
            if (!Instance || !SceneFader)
            {
                Debug.Log("no scene fader");
                return;
            }
            await SceneFader.FadeHide(Instance.sceneFadeTime, GlobalGameTools.Instance.TokenSource.Token);
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}