using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class MovementStateManager : MovementStateManager<MovementStateGraph, MovementStateMachine, Mover>
    {
        public MovementStateManager(MovementStateMachine stateMachine = null, Mover mover = null) : base(stateMachine, mover) { }
    }

    public class MovementStateManager<T0, T1, T2> : StateManager<T0, T1, T2>
        where T0 : MovementStateGraph
        where T1 : StateMachine<T0>
        where T2 : Mover
    {
        public MovementStateManager(T1 stateMachine = null, T2 mover = null) : base(stateMachine, mover) { }

        public override void Configure()
        {
            base.Configure();
        }
    }
}