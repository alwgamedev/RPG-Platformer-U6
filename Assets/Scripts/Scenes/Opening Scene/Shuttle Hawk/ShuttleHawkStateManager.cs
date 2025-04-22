using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class ShuttleHawkStateManager : AIPatrollerStateManager<ShuttleHawkStateGraph, ShuttleHawkStateMachine,
        IHybridFlyerController, ShuttleHawk>
    //AIPatrollerStateManager<ShuttleHawkStateGraph, ShuttleHawkStateMachine, 
    //HybridFlyerController, HybridFlyer, HybridFlyerStateGraph, HybridFlyerStateMachine, HybridFlyerStateManager,
    //ShuttleHawk>
    {
        public ShuttleHawkStateManager(ShuttleHawkStateMachine stateMachine, ShuttleHawk patroller)
            : base(stateMachine, patroller) { }
    }
}