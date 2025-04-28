using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class ShuttleHawkStateManager : AIPatrollerStateManager<ShuttleHawkStateGraph, ShuttleHawkStateMachine,
        IHybridFlyerController, ShuttleHawk>
    {
        public ShuttleHawkStateManager(ShuttleHawkStateMachine stateMachine, ShuttleHawk patroller)
            : base(stateMachine, patroller) { }
    }
}