using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class AIPatrollerStateManager : AIPatrollerStateManager<AIPatrollerStateGraph, AIPatrollerStateMachine, AIPatroller>
    {
        public AIPatrollerStateManager(AIPatrollerStateMachine stateMachine, AIPatroller patroller) : base(stateMachine, patroller) { }
    }

    public class AIPatrollerStateManager<T0, T1, T2> : StateManager<T0, T1, T2>
        where T0 : AIPatrollerStateGraph
        where T1 : AIPatrollerStateMachine<T0>
        where T2 : AIPatroller
    {
        public AIPatrollerStateManager(T1 stateMachine, T2 patroller) : base(stateMachine, patroller) { }

        public override void Configure()
        {
            base.Configure();

            //StateGraph.pursuit.OnEntry += OnPursuitEntry;
            //StateGraph.pursuit.OnExit += OnPursuitExit;
        }

        //protected virtual void OnPursuitEntry()
        //{
        //    driver.movementController.SetRunning(true);
        //}

        //protected virtual void OnPursuitExit()
        //{
        //    driver.movementController.SetRunning(false);
        //    driver.movementController.MoveInput = 0;
        //}
    }
}