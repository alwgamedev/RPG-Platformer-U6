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
    public class Freefall : MoveState { }

    public class MovementStateGraph : StateGraph
    {
        public readonly Grounded grounded;
        public readonly Freefall freefall;
        //public readonly Jumping jumping;

        public MovementStateGraph() : base()
        {
            grounded = CreateNewVertex<Grounded>();
            freefall = CreateNewVertex<Freefall>();
            //jumping = CreateNewVertex<Jumping>();

            AddEdgeBothWays((grounded, freefall));
            //AddEdgeBothWays((grounded, jumping));
            //AddEdge((freefall, jumping));
        }
    }
}