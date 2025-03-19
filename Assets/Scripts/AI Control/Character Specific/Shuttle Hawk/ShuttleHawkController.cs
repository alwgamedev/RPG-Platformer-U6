using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    using T0 = HybridFlyerController;
    using T00 = HybridFlyer;
    using T01 = HybridFlyerStateGraph;
    using T02 = HybridFlyerStateMachine;
    using T03 = HybridFlyerStateManager;
    using T1 = HybridFlyerPatroller;
    using T2 = AIPatrollerStateGraph;
    using T3 = AIPatrollerStateMachine;

    public class ShuttleHawkController : GenericAIPatrollerController<T0, T00, T01, T02, T03, T1, T2, T3,
        AIPatrollerStateManager<T2, T3, T0, T00, T01, T02, T03, T1>>
    {
        [SerializeField] PatrolPath flightPath;

        int boundedIterations;

        //walk back and forth between bounds 3 times, then fly along flightPath
        protected override void OnPatrolDestinationReached()
        {
            if (patroller.PatrolNavigator.CurrentMode == PatrolMode.bounded)
            {
                boundedIterations++;

                if (boundedIterations > 2)
                {
                    boundedIterations = 0;
                    patroller.BeginPatrol(PatrolMode.pathForwards, flightPath);
                    patroller.MovementController.SoftStop();
                    patroller.MovementController.BeginFlying();
                }
                else
                {
                    patroller.PatrolNavigator.GetNextDestination();
                }
            }
            else
            {
                if (!patroller.PatrolNavigator.GetNextDestination())
                {
                    patroller.BeginPatrol(defaultPatrolMode, defaultPatrolParameters);
                }
            }
        }
    }
}
