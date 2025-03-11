using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    public class CombatStateMachine : CombatStateMachine<CombatStateGraph>
    {
        public CombatStateMachine() : base() { }
    }

    public class CombatStateMachine<T> : StateMachine<T> where T : CombatStateGraph
    {
        public CombatStateMachine() : base() { }

        public override void SetCurrentState(State newState)
        {
            if (newState == stateGraph.dead)
            {
                Unfreeze();
            }

            base.SetCurrentState(newState);
        }
    }

    public abstract class CombatState : State { }
    public class NotInCombat : CombatState { }
    public class InCombat : CombatState { }
    public class Dead : CombatState { }

    public class CombatStateGraph : StateGraph
    {
        public readonly NotInCombat notInCombat;
        public readonly InCombat inCombat;
        public readonly Dead dead;

        public CombatStateGraph() : base()
        {
            notInCombat = CreateNewVertex<NotInCombat>();
            inCombat = CreateNewVertex<InCombat>();
            dead = CreateNewVertex<Dead>();

            AddEdgeBothWays((notInCombat, inCombat));
            AddEdgeBothWays((notInCombat, dead));
            AddEdge((inCombat, dead));
        }
    }
}