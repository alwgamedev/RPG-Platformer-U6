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

    public class Jumping : Airborne { }

    public class AdvancedMovementStateGraph : MovementStateGraph
    {
        public readonly Jumping jumping;
        //public readonly Swimming swimming;

        public AdvancedMovementStateGraph() : base()
        {
            jumping = CreateNewVertex<Jumping>();
            //swimming = CreateNewVertex<Swimming>();

            AddEdgeBothWays((grounded, jumping));
            AddEdge((freefall, jumping));
            AddEdgeBothWays((jumping, swimming));
            //^you could go swimming to freefall e.g. if you fall off a waterfall
        }
    }
}