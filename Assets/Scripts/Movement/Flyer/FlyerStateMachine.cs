namespace RPGPlatformer.Movement
{
    public class FlyerStateMachine : FlyerStateMachine<FlyerStateGraph>
    {
        public FlyerStateMachine() : base() {}
    }

    public class FlyerStateMachine<T> : AdvancedMovementStateMachine<T> where T : FlyerStateGraph
    {
        public FlyerStateMachine() : base() {}
    }

    public class Flying : Airborne { }

    public class FlyerStateGraph : AdvancedMovementStateGraph
    {
        public readonly Flying flying;

        public FlyerStateGraph() : base()
        {
            flying = CreateNewVertex<Flying>();

            AddEdgeBothWays((grounded, flying));
            AddEdge((jumping, flying));
            AddEdge((freefall, flying));
        }
    }
}