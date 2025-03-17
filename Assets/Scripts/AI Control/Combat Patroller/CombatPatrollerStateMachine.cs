namespace RPGPlatformer.AIControl
{
    public class CombatPatrollerStateMachine : CombatPatrollerStateMachine<CombatPatrollerStateGraph>
    {
        public CombatPatrollerStateMachine() : base() { }
    }

    public class CombatPatrollerStateMachine<T> : AIPatrollerStateMachine<T> where T : CombatPatrollerStateGraph
    {
        public CombatPatrollerStateMachine() : base() { }
    }

    public class Suspicion : AIPatrollerState { }//Suspicion = there is a target in the area,
                                                 //and I will be actively checking whether I am in range to pursue or attack
    public class Pursuit : AIPatrollerState { }
    public class Attack : AIPatrollerState { }

    public class CombatPatrollerStateGraph : AIPatrollerStateGraph
    {
        public readonly Suspicion suspicion;
        public readonly Pursuit pursuit;
        public readonly Attack attack;

        public CombatPatrollerStateGraph() : base()
        {
            suspicion = CreateNewVertex<Suspicion>();
            pursuit = CreateNewVertex<Pursuit>();
            attack = CreateNewVertex<Attack>();

            AddEdgeBothWays((patrol, suspicion));
            AddEdgeBothWays((patrol, pursuit));
            AddEdgeBothWays((patrol, attack));
            //AddEdgeBothWays((patrol, inactive));
            AddEdgeBothWays((suspicion, pursuit));
            AddEdgeBothWays((suspicion, attack));
            AddEdgeBothWays((suspicion, inactive));
            AddEdgeBothWays((pursuit, attack));
            AddEdgeBothWays((pursuit, inactive));
            AddEdgeBothWays((attack, inactive));
        }
    }
}