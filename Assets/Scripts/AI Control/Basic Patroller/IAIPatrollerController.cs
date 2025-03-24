using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public interface IAIPatrollerController
    {
        public bool Patrolling { get; }
        public IMovementController MovementController { get; }

        public void BeginDefaultPatrol();

        public void BeginPatrolRest();

        public void BeginPatrol(NavigationMode mode, object param);
    }
}