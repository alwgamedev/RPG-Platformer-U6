using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.Movement
{
    public class PlayerMovementController : AdvancedMovementController, IPausable
    {
        bool inputDisabled;

        protected override void Awake()
        {
            base.Awake();

            SettingsManager.OnIAMConfigure += OnIAMConfigure;
        }

        void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;

            iam.ToggleRunAction.started += (context) =>
            {
                if (inputDisabled) return;
                ToggleRunning();

            };

            iam.MoveRightAction.started += (context) =>
            {
                if (inputDisabled) return;
                UpdateMoveInput();
            };
            iam.MoveRightAction.canceled += (context) => 
            { 
                if (inputDisabled) return; 
                UpdateMoveInput(); 
            };
            iam.MoveLeftAction.started += (context) => 
            { 
                
                if (inputDisabled) return; 
                UpdateMoveInput(); 
            };
            iam.MoveLeftAction.canceled += (context) => 
            { 
                if (inputDisabled) return; 
                UpdateMoveInput(); 
            };

            iam.SpacebarAction.started += (context) => 
            { 
                if (inputDisabled) return; 
                mover.Jump(); 
            };
        }

        private void UpdateMoveInput()
        {
            MoveInput = new Vector2((SettingsManager.Instance.IAM.MoveRightHeldDown ? 1 : 0)
               - (SettingsManager.Instance.IAM.MoveLeftHeldDown ? 1 : 0), 0);
        }

        protected override void Move(Vector2 moveInput)
        {
            if (CurrentMount != null && Grounded)
            {
                mover.MoveWithoutAcceleration(GetMoveDirection(moveInput), currentMovementOptions);
            }
            else
            {
                base.Move(moveInput);
            }
        }

        public void Pause()
        {
            DisableInput();
        }

        public void Unpause()
        {
            EnableInput();
        }

        private void DisableInput()
        {
            inputDisabled = true;
        }

        private void EnableInput()
        {
            inputDisabled = false;
        }

        public override void OnDeath()
        {
            DisableInput();
            base.OnDeath();
        }

        public override void OnRevival()
        {
            base.OnRevival();
            EnableInput();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SettingsManager.OnIAMConfigure -= OnIAMConfigure;
        }
    }
}