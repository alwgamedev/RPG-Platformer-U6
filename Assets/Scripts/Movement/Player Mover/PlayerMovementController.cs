using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class PlayerMovementController : ClimberMovementController/*, IPausable*/
    {
        //bool inputDisabled;

        bool moveLeftHeldDown;
        bool moveRightHeldDown;

        protected override void Awake()
        {
            base.Awake();

            SettingsManager.IAMConfigured += OnIAMConfigure;
        }

        void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;

            iam.InputAction(InputActionsManager.toggleRunActionName).started += (context) =>
            {
                //if (InputSource.IsInputDisabled) return;
                ToggleRunning();

            };
            iam.InputAction(InputActionsManager.moveRightActionName).started += (context) =>
            {
                //if (InputSource.IsInputDisabled) return;
                moveRightHeldDown = true;
                if (!InputSource.IsInputDisabled)
                {
                    ComputeMoveInput();
                }
            };
            iam.InputAction(InputActionsManager.moveRightActionName).canceled += (context) =>
            {
                //if (!InputSource.IsInputEnabled) return;
                moveRightHeldDown = false;
                if (!InputSource.IsInputDisabled)
                {
                    ComputeMoveInput();
                }
            };
            iam.InputAction(InputActionsManager.moveLeftActionName).started += (context) =>
            {
                moveLeftHeldDown = true;
                if (!InputSource.IsInputDisabled)
                {
                    ComputeMoveInput();
                }
            };
            iam.InputAction(InputActionsManager.moveLeftActionName).canceled += (context) =>
            {
                //if (inputDisabled) return;
                moveLeftHeldDown = false;
                if (!InputSource.IsInputDisabled)
                {
                    ComputeMoveInput();
                }
            };
            iam.InputAction(InputActionsManager.jumpActionName).started += (context) =>
            {
                if (InputSource.IsInputDisabled) return;
                stateDriver.Jump();
            };
            iam.InputAction(InputActionsManager.climbActionName).started += (context) =>
            {
                if (InputSource.IsInputDisabled) return;
                TryGrabOntoClimbableObject();
            };
        }

        public override void OnInputEnabled()
        {
            base.OnInputEnabled();

            ComputeMoveInput();
            //so that we can continue moving as soon as input is re-enabled
            //(e.g. when you are grabbed by evil root, you want move input to come back in immediately
            //when you are released)
        }

        private void ComputeMoveInput()
        {
            MoveInput = new Vector2((moveLeftHeldDown ? -1 : 0) + (moveRightHeldDown ? 1 : 0), 0);
        }

        protected override bool InSwingMode()
        {
            return SettingsManager.Instance.IAM.HeldDown(InputActionsManager.climbActionName);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SettingsManager.IAMConfigured -= OnIAMConfigure;
        }
    }
}