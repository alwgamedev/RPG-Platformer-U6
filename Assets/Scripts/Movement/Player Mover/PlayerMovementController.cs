using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.Movement
{
    public class PlayerMovementController : AdvancedMovementController, IPausable
    {
        bool inputDisabled;

        protected override void OnEnable()
        {
            base.OnEnable();

            SettingsManager.OnIAMConfigure += OnIAMConfigure;
        }

        void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;

            iam.ToggleRunAction.started += (context) =>
            {
                if (inputDisabled) return;
                mover.ToggleRun();
            };

            iam.MoveRightAction.started += (context) =>
            {
                if (inputDisabled) return;
                ComputeMoveInput();
            };
            iam.MoveRightAction.canceled += (context) => 
            { 
                if (inputDisabled) return; 
                ComputeMoveInput(); 
            };
            iam.MoveLeftAction.started += (context) => 
            { 
                
                if (inputDisabled) return; 
                ComputeMoveInput(); 
            };
            iam.MoveLeftAction.canceled += (context) => 
            { 
                if (inputDisabled) return; 
                ComputeMoveInput(); 
            };

            iam.SpacebarAction.started += (context) => 
            { 
                if (inputDisabled) return; 
                mover.Jump(); 
            };
        }

        private void ComputeMoveInput()
        {
            float val = 0;
            if (SettingsManager.Instance.IAM.MoveLeftHeldDown)
            {
                val -= 1;
            }
            if (SettingsManager.Instance.IAM.MoveRightHeldDown)
            {
                val += 1;
            }
            MoveInput = val;
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

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SettingsManager.OnIAMConfigure -= OnIAMConfigure;
        }
    }
}