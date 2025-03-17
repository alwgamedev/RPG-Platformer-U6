namespace RPGPlatformer.Movement
{
    public class HybridFlyerStateMachine : HybridFlyerStateMachine<HybridFlyerStateGraph>
    {
        public HybridFlyerStateMachine() : base() {}
    }

    public class HybridFlyerStateMachine<T> : AdvancedMovementStateMachine<T> where T : HybridFlyerStateGraph
    {
        public HybridFlyerStateMachine() : base() {}
    }

    public class Flying : Airborne { }

    public class HybridFlyerStateGraph : AdvancedMovementStateGraph
    {
        public readonly Flying flying;

        public HybridFlyerStateGraph() : base()
        {
            flying = CreateNewVertex<Flying>();

            AddEdgeBothWays((grounded, flying));
            AddEdge((jumping, flying));
            AddEdge((freefall, flying));
        }
    }
}