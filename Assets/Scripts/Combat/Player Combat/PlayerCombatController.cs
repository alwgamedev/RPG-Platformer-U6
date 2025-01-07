using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.UI;
using RPGPlatformer.Inventory;
using UnityEngine.EventSystems;

namespace RPGPlatformer.Combat
{
    public class PlayerCombatController : CombatController
    {
        public override IInputSource InputSource => SettingsManager.Instance.IBM;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            SettingsManager.OnIBMConfigure += OnIBMConfigure;
        }

        protected override void Start()
        {
            base.Start();

            combatant.OnInventoryOverflow += OnInventoryOverflow;

            var spaghetti = InventoryItemSO.FindByName("Spaghetti");
            var bow = InventoryItemSO.FindByName("Basic Bow (SH)");
            var sword = InventoryItemSO.FindByName("Basic Sword (SH)");
            var gold = InventoryItemSO.FindByName("Gold Coins");
            var cookie = InventoryItemSO.FindByName("Cookie");

            //combatant.Inventory.DistributeToFirstAvailableSlots(spaghetti);
            //combatant.Inventory.DistributeToFirstAvailableSlots(spaghetti);
            //combatant.Inventory.DistributeToFirstAvailableSlots(bow);
            //combatant.Inventory.DistributeToFirstAvailableSlots(sword);
            //combatant.Inventory.DistributeToFirstAvailableSlots(cookie, 8);
            //combatant.Inventory.DistributeToFirstAvailableSlots(gold, 773);

            combatant.TakeLoot(spaghetti.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(spaghetti.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(bow.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(sword.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(cookie.CreateInstanceOfItem().ToSlotData(8));
            combatant.TakeLoot(gold.CreateInstanceOfItem().ToSlotData(773));
        }

        void OnIBMConfigure()
        {
            InputBindingManager ibm = SettingsManager.Instance.IBM;

            ibm.LeftClickAction.started += (context) =>
            {
                if (!EventSystem.current.IsPointerOverGameObject() && !InteractableGameObject.MouseOverAnyIGO)
                {
                    FireButtonDown();
                }
            };
            ibm.LeftClickAction.canceled += (context) => FireButtonUp();
            ibm.RightClickAction.started += (context) => CancelAbilityInProgress(true);

            foreach (var entry in ibm.AbilityBarActions)
            {
                ibm.AbilityBarActions[entry.Key].started += (context) => HandleAbilityInput(entry.Key);
            }

            SettingsManager.OnIBMConfigure -= OnIBMConfigure;
        }

        protected override void Update()
        {
            base.Update();

            //FOR TESTING PURPOSES
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                TogglePlayerAlive();
            }
        }

        protected override void AttemptedToExecuteAbilityOnCooldown()
        {
            GameLog.Log("That ability is on cooldown.");
        }

        private void TogglePlayerAlive()//FOR TESTING PURPOSES
        {
            if (!combatant.Health.IsDead)
            {
                combatant.TakeDamage(Mathf.Infinity, null);
            }
            else
            {
                combatant.Revive();
            }
        }

        private void OnInventoryOverflow()
        {
            GameLog.Log("Your inventory is too full to hold any more items.");
        }

        public override void OnTargetingFailed()
        {
            GameLog.Log("No target selected (you may be out of range).");
        }

        public override void OnInsufficientStamina()
        {
            GameLog.Log("You don't have enough stamina to use that ability.");
        }

        public override void OnInsufficientWrath()
        {
            GameLog.Log("You don't have enough wrath to use that ability.");
        }

        public override Vector2 GetAimPosition()
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}