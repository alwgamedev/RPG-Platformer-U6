namespace RPGPlatformer.AIControl
{
    public class ShuttleHawkStateMachine : AIPatrollerStateMachine<ShuttleHawkStateGraph>
    {
        public ShuttleHawkStateMachine() : base() { }
    }

    public abstract class ShuttleHawkState : AIPatrollerState { }
    public class AwaitingDeparture : ShuttleHawkState { }
    public class Shuttling : ShuttleHawkState { }
    public class ReturningToNest : ShuttleHawkState { }

    public class ShuttleHawkStateGraph : AIPatrollerStateGraph
    {
        //make sure everything has edge out of inactive (so that we can transition out of pause)

        public readonly AwaitingDeparture awaitingDeparture;
        public readonly Shuttling shuttling;
        public readonly ReturningToNest returningToNest;

        public ShuttleHawkStateGraph() : base()
        {
            awaitingDeparture = CreateNewVertex<AwaitingDeparture>();
            shuttling = CreateNewVertex<Shuttling>();
            returningToNest = CreateNewVertex<ReturningToNest>();

            AddEdgeBothWays((patrol, awaitingDeparture));//can return to patrol if departure times out
            AddEdge((awaitingDeparture, shuttling));
            AddEdge((shuttling, returningToNest));
            AddEdge((returningToNest, patrol));

            AddEdgeBothWaysForAll(inactive);
        }
    }
}