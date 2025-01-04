using RPGPlatformer.Core;


namespace RPGPlatformer.Movement
{
    public class PlayerMovementController : AdvancedMovementController
    {
        InputBindingManager ibm;

        protected override void Awake()
        {
            base.Awake();

        }

        protected override void OnEnable()
        {
            base.OnEnable();

            SettingsManager.OnIBMConfigure += OnIBMConfigure;
        }

        void OnIBMConfigure()
        {
            ibm = SettingsManager.Instance.IBM;

            ibm.ToggleRunAction.started += (context) => mover.ToggleRun();

            ibm.MoveRightAction.started += (context) => ComputeMoveInput();
            ibm.MoveRightAction.canceled += (context) => ComputeMoveInput();
            ibm.MoveLeftAction.started += (context) => ComputeMoveInput();
            ibm.MoveLeftAction.canceled += (context) => ComputeMoveInput();

            ibm.SpacebarAction.started += (context) => mover.Jump();

            SettingsManager.OnIBMConfigure -= OnIBMConfigure;
        }

        private void ComputeMoveInput()
        {
            float val = 0;
            if (ibm.moveLeftHeldDown)
            {
                val -= 1;
            }
            if (ibm.moveRightHeldDown)
            {
                val += 1;
            }
            moveInput = val;
        }
    }
}