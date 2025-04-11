using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.UI;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Combat
{
    public class PlayerCombatController : CombatController
    {
        protected override void Awake()
        {
            base.Awake();

            SettingsManager.IAMConfigured += OnIAMConfigure;
            SettingsManager.NewAbilityBarSettings += UpdateAbilityBars;

            InteractableGameObject.IGOClicked += OnIGOClicked;
        }

        protected override void Start()
        {
            base.Start();

            stateDriver.OnInventoryOverflow += OnInventoryOverflow;

            var spaghetti = InventoryItemSO.FindByName("Spaghetti");
            var staff = InventoryItemSO.FindByName("Basic Staff (SH)");
            var bow = InventoryItemSO.FindByName("Basic Bow (SH)");
            var sword = InventoryItemSO.FindByName("Basic Sword (SH)");
            var gold = InventoryItemSO.FindByName("Gold Coins");
            var cookie = InventoryItemSO.FindByName("Cookie");
            var redBody = InventoryItemSO.FindByName("Red Body");
            var bwTop = InventoryItemSO.FindByName("Blue Wizard Top");
            var bwPants = InventoryItemSO.FindByName("Blue Wizard Pants");

            stateDriver.TakeLoot(spaghetti.CreateInstanceOfItem().ToSlotData(1));
            stateDriver.TakeLoot(spaghetti.CreateInstanceOfItem().ToSlotData(1));
            stateDriver.TakeLoot(staff.CreateInstanceOfItem().ToSlotData(1));
            stateDriver.TakeLoot(bow.CreateInstanceOfItem().ToSlotData(1));
            stateDriver.TakeLoot(sword.CreateInstanceOfItem().ToSlotData(1));
            stateDriver.TakeLoot(cookie.CreateInstanceOfItem().ToSlotData(8));
            stateDriver.TakeLoot(gold.CreateInstanceOfItem().ToSlotData(773));
            stateDriver.TakeLoot(redBody.CreateInstanceOfItem().ToSlotData(1));
            stateDriver.TakeLoot(bwTop.CreateInstanceOfItem().ToSlotData(1));
            stateDriver.TakeLoot(bwPants.CreateInstanceOfItem().ToSlotData(1));
        }

        void OnIAMConfigure()
        {
            InputActionsManager iam = SettingsManager.Instance.IAM;

            iam.InputAction(InputActionsManager.leftClickActionName).started += (context) =>
            {
                if(InputSource.IsInputDisabled || iam.MouseOverUI || InteractableGameObject.MouseOverAnyIGO) 
                    return;
                FireButtonDown();
            };
            iam.InputAction(InputActionsManager.leftClickActionName).canceled += (context) =>
            {
                //if (inputDisabled) return;
                FireButtonUp();
            };
            iam.InputAction(InputActionsManager.rightClickActionName).started += (context) =>
            {
                //if (inputDisabled) return;
                CancelAbilityInProgress(true);
            };

            foreach (var entry in iam.AbilityBarActions)
            {
                iam.AbilityBarActions[entry.Key].started += (context) =>
                {
                    if (InputSource.IsInputDisabled) return;
                    HandleAbilityInput(entry.Key);
                };
            }
        }

        private void Update()
        {
            if (FireButtonIsDown)
            {
                RunAutoAbilityCycle(false);
            }

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
            if (!stateDriver.Health.IsDead)
            {
                stateDriver.HandleHealthChange(Mathf.Infinity, null);
            }
            else
            {
                stateDriver.Revive();
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

        //protected override void DisableInput()
        //{
        //    CancelAbilityInProgress(false);
        //    //inputDisabled = true;
        //}

        //protected override void EnableInput()
        //{
        //    inputDisabled = false;
        //}

        protected override void OnStunned(bool frozen)
        {
            if (frozen)
            {
                GameLog.Log("You've been frozen!");
            }
            else
            {
                GameLog.Log("You've been stunned and can't move!");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SettingsManager.IAMConfigured -= OnIAMConfigure;
            SettingsManager.NewAbilityBarSettings -= UpdateAbilityBars;
            InteractableGameObject.IGOClicked -= OnIGOClicked;
        }
    }
}