using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class MovementStateMachine : MovementStateMachine<MovementStateGraph>
    {
        public MovementStateMachine() : base() { }
    }

    public class MovementStateMachine<T> : StateMachine<T> where T : MovementStateGraph
    {
        public MovementStateMachine() : base() { }
    }

    public abstract class MoveState : State { }
    public class Grounded : MoveState { }
    public class Airborne : MoveState { }
    public class Jumping : Airborne { }

    public class MovementStateGraph : StateGraph
    {
        public readonly Grounded grounded;
        public readonly Airborne airborne;
        public readonly Jumping jumping;

        public MovementStateGraph() : base()
        {
            grounded = CreateNewVertex<Grounded>();
            airborne = CreateNewVertex<Airborne>();
            jumping = CreateNewVertex<Jumping>();

            AddEdgeBothWays((grounded, airborne));
            AddEdgeBothWays((grounded, jumping));
            AddEdge((airborne, jumping));
        }
    }
}