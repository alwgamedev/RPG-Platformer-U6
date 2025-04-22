using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class PillBugMovementStateMachine : StateMachine<PillBugMovementStateGraph>
    {
        public PillBugMovementStateMachine() : base() { }
    }

    public abstract class PillBugMoveState : MoveState { }
    public class PillBugUncurled : PillBugMoveState { }
    public class PillBugCurled : PillBugMoveState { }

    public class PillBugMovementStateGraph : StateGraph
    {
        public readonly PillBugUncurled uncurled;
        public readonly PillBugCurled curled;

        public PillBugMovementStateGraph() : base()
        {
            uncurled = CreateNewVertex<PillBugUncurled>();
            curled = CreateNewVertex<PillBugCurled>();

            AddEdgeBothWays((uncurled, curled));
        }
    }
}