namespace RPGPlatformer.Movement
{
    public class AdvancedMovementStateMachine : AdvancedMovementStateMachine<AdvancedMovementStateGraph>
    {
        public AdvancedMovementStateMachine() : base() { }
    }

    public class AdvancedMovementStateMachine<T> : MovementStateMachine<T> where T : AdvancedMovementStateGraph
    {
        public AdvancedMovementStateMachine() : base() {}
    }

    public class Jumping : MoveState { }

    public class AdvancedMovementStateGraph : MovementStateGraph
    {
        public readonly Jumping jumping;

        public AdvancedMovementStateGraph() : base()
        {

            jumping = CreateNewVertex<Jumping>();

            AddEdgeBothWays((grounded, jumping));
            AddEdge((freefall, jumping));
        }
    }
}