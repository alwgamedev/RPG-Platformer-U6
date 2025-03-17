using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class CombatPatrollerStateMachine : CombatPatrollerStateMachine<CombatPatrollerStateGraph>
    {
        public CombatPatrollerStateMachine() : base() { }
    }

    public class CombatPatrollerStateMachine<T> : StateMachine<T> where T : CombatPatrollerStateGraph
    {
        public CombatPatrollerStateMachine() : base() { }
    }

    public abstract class AIPatrollerState : State { }
    public class Inactive : AIPatrollerState { }
    public class Patrol : AIPatrollerState { }
    public class Suspicion : AIPatrollerState { }//Suspicion = there is a target in the area,
                                                 //and I will be actively checking whether I am in range to pursue or attack
    public class Pursuit : Suspicion { }
    public class Attack : Suspicion { }

    public class CombatPatrollerStateGraph : StateGraph
    {
        public readonly Inactive inactive;
        public readonly Patrol patrol;
        public readonly Suspicion suspicion;
        public readonly Pursuit pursuit;
        public readonly Attack attack;

        public CombatPatrollerStateGraph() : base()
        {
            inactive = CreateNewVertex<Inactive>();
            patrol = CreateNewVertex<Patrol>();
            suspicion = CreateNewVertex<Suspicion>();
            pursuit = CreateNewVertex<Pursuit>();
            attack = CreateNewVertex<Attack>();

            AddEdgeBothWays((patrol, suspicion));
            AddEdgeBothWays((patrol, pursuit));
            AddEdgeBothWays((patrol, attack));
            AddEdgeBothWays((patrol, inactive));
            AddEdgeBothWays((suspicion, pursuit));
            AddEdgeBothWays((suspicion, attack));
            AddEdgeBothWays((suspicion, inactive));
            AddEdgeBothWays((pursuit, attack));
            AddEdgeBothWays((pursuit, inactive));
            AddEdgeBothWays((attack, inactive));
        }
    }
}