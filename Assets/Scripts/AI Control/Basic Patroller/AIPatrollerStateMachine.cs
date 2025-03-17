using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class AIPatrollerStateMachine : AIPatrollerStateMachine<AIPatrollerStateGraph>
    {
        public AIPatrollerStateMachine() : base() { }
    }

    public class AIPatrollerStateMachine<T> : StateMachine<T> where T : AIPatrollerStateGraph
    {
        public AIPatrollerStateMachine() : base() { }
    }

    public abstract class AIPatrollerState : State { }
    public class Inactive : AIPatrollerState { }
    public class Patrol : AIPatrollerState { }

    public class AIPatrollerStateGraph : StateGraph
    {
        public readonly Inactive inactive;
        public readonly Patrol patrol;

        public AIPatrollerStateGraph() : base()
        {
            inactive = CreateNewVertex<Inactive>();
            patrol = CreateNewVertex<Patrol>();

            AddEdgeBothWays((patrol, inactive));
        }
    }
}