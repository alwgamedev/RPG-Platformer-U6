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
    public abstract class Airborne : MoveState { }
    public class Grounded : MoveState { }
    public class Freefall : Airborne { }
    public class Swimming : MoveState { }

    public class MovementStateGraph : StateGraph
    {
        public readonly Grounded grounded;
        public readonly Freefall freefall;
        public readonly Swimming swimming;

        public MovementStateGraph() : base()
        {
            grounded = CreateNewVertex<Grounded>();
            freefall = CreateNewVertex<Freefall>();
            swimming = CreateNewVertex<Swimming>();

            AddEdgeBothWays((grounded, freefall));
            AddEdgeBothWaysForAll(swimming);
        }
    }
}