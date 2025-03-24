using UnityEngine;
using RPGPlatformer.Movement;
using UnityEditor;

namespace RPGPlatformer.AIControl
{
    using T0 = HybridFlyerController;
    using T00 = HybridFlyer;
    using T01 = HybridFlyerStateGraph;
    using T02 = HybridFlyerStateMachine;
    using T03 = HybridFlyerStateManager;
    using T1 = ShuttleHawk;
    using T2 = ShuttleHawkStateGraph;
    using T3 = ShuttleHawkStateMachine;

    public class ShuttleHawkController : GenericAIPatrollerController<T0, T00, T01, T02, T03, T1, T2, T3,
        ShuttleHawkStateManager>
    {
        [SerializeField] PatrolPath flightPath;
        [SerializeField] Transform departurePoint;

        public bool AwaitingDeparture 
            => stateManager.StateMachine.CurrentState == stateManager.StateGraph.awaitingDeparture;

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.awaitingDeparture.OnEntry += OnAwaitingDepartureEntry;

            StateBehavior[stateManager.StateGraph.awaitingDeparture] = patroller.AwaitingDepartureBehavior;
            StateBehavior[stateManager.StateGraph.shuttling] = patroller.PatrolBehavior;
            StateBehavior[stateManager.StateGraph.returningToNest] = patroller.PatrolBehavior;
            //later patroller may have more specific functions
        }

        private void OnAwaitingDepartureEntry()
        {
            patroller.HeadToDeparturePoint(departurePoint);
        }

        protected override void OnPatrolDestinationReached()
        {
            base.OnPatrolDestinationReached();
            
            if (AwaitingDeparture)//this is when hawk reaches departure point and is waiting for departure
            {
                patroller.ReadyForDeparture();
            }
        }
    }
}
