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
    //public class Climbing : Grounded { }

    public class AdvancedMovementStateGraph : MovementStateGraph
    {
        public readonly Jumping jumping;
        //public readonly Climbing climbing;

        public AdvancedMovementStateGraph() : base()
        {
            jumping = CreateNewVertex<Jumping>();
            //climbing = CreateNewVertex<Climbing>();

            AddEdgeBothWays((grounded, jumping));
            AddEdge((freefall, jumping));
            //AddEdgeBothWaysForAll(climbing);
        }
    }
}