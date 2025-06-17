using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.UI;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Combat
{
    public class PlayerCombatController : CombatController
    {
        bool hasSentAutoCastStaminaWarning;
        //gets reset next time you cast an auto ability
        //or release fire button

        protected override void Awake()
        {
            base.Awake();

            if (SettingsManager.Instance && SettingsManager.Instance.IAMIsConfigured)
            {
                OnIAMConfigure();
            }
            SettingsManager.IAMConfigured += OnIAMConfigure;//still subscribe to get settings updates
            SettingsManager.NewAbilityBarSettings += UpdateAbilityBars;

            InteractableGameObject.IGOClicked += OnIGOClicked;
        }

        private void Update()
        {
            if (FireButtonIsDown)
            {
                if (RunAutoAbilityCycle(false) && hasSentAutoCastStaminaWarning)
                {
                    hasSentAutoCastStaminaWarning = false;
                }
            }

            //JUST FOR TESTING
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                TogglePlayerAlive();
            }
        }

        private void OnIAMConfigure()
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

        protected override void InitializeInventoryItems()
        {
            //JUST FOR TESTING
            var spaghetti = InventoryItemSO.FindItemSO["Spaghetti"];
            var staff = InventoryItemSO.FindItemSO["Basic Staff (SH)"];
            var bow = InventoryItemSO.FindItemSO["Basic Bow (SH)"];
            var sword = InventoryItemSO.FindItemSO["Basic Sword (SH)"];
            var ssbow = InventoryItemSO.FindItemSO["Spider Silk Bow (SH)"];
            var gold = InventoryItemSO.FindItemSO["Gold Coins"];
            var cookie = InventoryItemSO.FindItemSO["Cookie"];
            var redBody = InventoryItemSO.FindItemSO["Red Body"];
            var bwHat = InventoryItemSO.FindItemSO["Blue Wizard Hat"];
            var bwTop = InventoryItemSO.FindItemSO["Blue Wizard Top"];
            var bwPants = InventoryItemSO.FindItemSO["Blue Wizard Pants"];

            stateDriver.TakeLoot(spaghetti.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(spaghetti.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(staff.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(bow.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(sword.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(ssbow.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(cookie.CreateInstanceOfItem().ToInventorySlotData(8));
            stateDriver.TakeLoot(gold.CreateInstanceOfItem().ToInventorySlotData(773));
            stateDriver.TakeLoot(redBody.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(bwHat.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(bwTop.CreateInstanceOfItem().ToInventorySlotData(1));
            stateDriver.TakeLoot(bwPants.CreateInstanceOfItem().ToInventorySlotData(1));
        }

        private void OnIGOClicked()
        {
            CancelAbilityInProgress();
        }

        protected override void AttemptedToExecuteAbilityOnCooldown()
        {
            GameLog.Log("That ability is on cooldown.");
        }

        protected override void OnNoAutoCastAbility()
        {
            //bool insuffStamina = abilityBarManager.CurrentAbilityBar.HasInsufficientStaminaToAutoCast();
            if (abilityBarManager.CurrentAbilityBar.HasInsufficientStaminaToAutoCast()
                && !hasSentAutoCastStaminaWarning)
            {
                GameLog.Log("You don't have enough stamina to execute any of your auto-cast abilities.");
                hasSentAutoCastStaminaWarning = true;
            }
            //else if (!insuffStamina && hasSentAutoCastStaminaWarning)
            //{
            //    hasSentAutoCastStaminaWarning = false;
            //}
        }
                

        protected override void BaseOnFireButtonUp()
        {
            base.BaseOnFireButtonUp();
            if (hasSentAutoCastStaminaWarning)
            {
                hasSentAutoCastStaminaWarning = false;
            }
        }

        private void TogglePlayerAlive()//FOR TESTING PURPOSES
        {
            if (stateDriver.Health.IsDead)
            {
                stateDriver.Revive();
            }
            else
            {
                stateDriver.Instakill();
            }
        }

        protected override void OnInventoryOverflow()
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