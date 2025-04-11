using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.Movement
{
    public class PlayerMovementController : AdvancedMovementController/*, IPausable*/
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
                if (InputSource.IsInputDisabled) return;
                ToggleRunning();

            };
            iam.InputAction(InputActionsManager.moveRightActionName).started += (context) =>
            {
                if (InputSource.IsInputDisabled) return;
                moveRightHeldDown = true;
                UpdateMoveInput();
            };
            iam.InputAction(InputActionsManager.moveRightActionName).canceled += (context) =>
            {
                //if (!InputSource.IsInputEnabled) return;
                moveRightHeldDown = false;
                UpdateMoveInput();
            };
            iam.InputAction(InputActionsManager.moveLeftActionName).started += (context) =>
            {
                if (InputSource.IsInputDisabled) return;
                moveLeftHeldDown = true;
                UpdateMoveInput();
            };
            iam.InputAction(InputActionsManager.moveLeftActionName).canceled += (context) =>
            {
                //if (inputDisabled) return;
                moveLeftHeldDown = false;
                UpdateMoveInput();
            };

            iam.InputAction(InputActionsManager.jumpActionName).started += (context) =>
            {
                if (InputSource.IsInputDisabled) return;
                stateDriver.Jump();
            };
        }

        //protected override void HandleMoveInput()
        //{
        //    if (CurrentMount == null)
        //    {
        //        base.HandleMoveInput();
        //    }
        //    else
        //    {
        //        HandleMoveInput(MoveInput, MoveWithoutAcceleration);
        //        //mover.Rigidbody.linearVelocity += CurrentMount.Velocity;
        //    }
        //}

        private void UpdateMoveInput()
        {
            MoveInput = new Vector2((moveLeftHeldDown ? -1 : 0) + (moveRightHeldDown ? 1 : 0), 0);
        }

        //public void Pause()
        //{
        //    DisableInput();
        //}

        //public void Unpause()
        //{
        //    EnableInput();
        //}

        //private void DisableInput()
        //{
        //    SoftStop();
        //    inputDisabled = true;
        //}

        //private void EnableInput()
        //{
        //    inputDisabled = false;
        //}

        //public override void OnDeath()
        //{
        //    DisableInput();
        //    base.OnDeath();
        //}

        //public override void OnRevival()
        //{
        //    base.OnRevival();
        //    EnableInput();
        //}

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SettingsManager.IAMConfigured -= OnIAMConfigure;
        }
    }
}