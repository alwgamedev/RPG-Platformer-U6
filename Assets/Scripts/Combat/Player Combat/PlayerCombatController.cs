using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.UI;
using RPGPlatformer.Inventory;
using UnityEngine.EventSystems;

namespace RPGPlatformer.Combat
{
    public class PlayerCombatController : CombatController
    {
        bool inputDisabled;

        public override IInputSource InputSource => SettingsManager.Instance.IAM;

        protected override void Awake()
        {
            base.Awake();

            SettingsManager.OnIAMConfigure += OnIAMConfigure;
            SettingsManager.NewAbilityBarSettings += UpdateAbilityBars;

            InteractableGameObject.IGOClicked += OnIGOClicked;
        }

        protected override void Start()
        {
            base.Start();

            combatant.OnInventoryOverflow += OnInventoryOverflow;

            var spaghetti = InventoryItemSO.FindByName("Spaghetti");
            var staff = InventoryItemSO.FindByName("Basic Staff (SH)");
            var bow = InventoryItemSO.FindByName("Basic Bow (SH)");
            var sword = InventoryItemSO.FindByName("Basic Sword (SH)");
            var gold = InventoryItemSO.FindByName("Gold Coins");
            var cookie = InventoryItemSO.FindByName("Cookie");
            var redBody = InventoryItemSO.FindByName("Red Body");
            var bwTop = InventoryItemSO.FindByName("Blue Wizard Top");
            var bwPants = InventoryItemSO.FindByName("Blue Wizard Pants");

            combatant.TakeLoot(spaghetti.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(spaghetti.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(staff.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(bow.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(sword.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(cookie.CreateInstanceOfItem().ToSlotData(8));
            combatant.TakeLoot(gold.CreateInstanceOfItem().ToSlotData(773));
            combatant.TakeLoot(redBody.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(bwTop.CreateInstanceOfItem().ToSlotData(1));
            combatant.TakeLoot(bwPants.CreateInstanceOfItem().ToSlotData(1));
        }

        void OnIAMConfigure()
        {
            InputActionsManager iam = SettingsManager.Instance.IAM;

            iam.LeftClickAction.started += (context) =>
            {
                if(inputDisabled || iam.MouseOverUI || InteractableGameObject.MouseOverAnyIGO) return;
                
                FireButtonDown();
            };
            iam.LeftClickAction.canceled += (context) =>
            {
                if (inputDisabled) return;
                FireButtonUp();
            };
            iam.RightClickAction.started += (context) =>
            {
                if (inputDisabled) return;
                CancelAbilityInProgress(true);
            };

            foreach (var entry in iam.AbilityBarActions)
            {
                iam.AbilityBarActions[entry.Key].started += (context) =>
                {
                    if (inputDisabled) return;
                    HandleAbilityInput(entry.Key);
                };
            }
        }

        protected override void Update()
        {
            base.Update();

            //FOR TESTING PURPOSES
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                TogglePlayerAlive();
            }
        }

        private void OnIGOClicked()
        {
            CancelAbilityInProgress();
        }

        protected override void AttemptedToExecuteAbilityOnCooldown()
        {
            GameLog.Log("That ability is on cooldown.");
        }

        private void TogglePlayerAlive()//FOR TESTING PURPOSES
        {
            if (!combatant.Health.IsDead)
            {
                combatant.HandleHealthChange(Mathf.Infinity, null);
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

        protected override void DisableInput()
        {
            CancelAbilityInProgress(false);
            inputDisabled = true;
        }

        protected override void EnableInput()
        {
            inputDisabled = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SettingsManager.OnIAMConfigure -= OnIAMConfigure;
            SettingsManager.NewAbilityBarSettings -= UpdateAbilityBars;
            InteractableGameObject.IGOClicked -= OnIGOClicked;
        }
    }
}