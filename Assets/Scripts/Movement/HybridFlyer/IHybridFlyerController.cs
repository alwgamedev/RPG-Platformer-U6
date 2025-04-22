using System;

namespace RPGPlatformer.Movement
{
    public interface IHybridFlyerController : IAIMovementController
    {
        public event Action OnFlightEntry;
        public event Action OnFlightExit;

        public void BeginFlying();
    }
}