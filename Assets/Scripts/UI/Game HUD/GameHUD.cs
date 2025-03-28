using UnityEngine;

namespace RPGPlatformer.UI
{
    //includes only the in-game hud; does not include pause ui
    public class GameHUD : HidableUI
    {
        AbilityBarUI abilityBar;
        DialogueUI dialogueUI;
        EquipmentInspectorUI equipmentInspector;
        PlayerInventoryUI playerInventory;
        XPAlertBar xpAlertBar;

        public static AbilityBarUI AbilityBar => Instance.abilityBar;
        public static DialogueUI DialogueUI => Instance.dialogueUI;
        public static EquipmentInspectorUI EquipmentInspector => Instance.equipmentInspector;
        public static GameLog GameLog => GameLog.Instance;
        public static PlayerInventoryUI PlayerInventory => Instance.playerInventory;
        public static XPAlertBar XPAlertBar => Instance.xpAlertBar;
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