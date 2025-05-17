using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public interface IAIPatroller
    {
        public IAIMovementController AIMovementController { get; }
    }
}