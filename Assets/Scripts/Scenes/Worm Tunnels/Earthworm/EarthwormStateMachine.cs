using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class EarthwormStateMachine : StateMachine<EarthwormStateGraph>
    {
        public EarthwormStateMachine() : base() { }
    }

    public abstract class EarthwormState : State { }
    public class EarthwormDormant : EarthwormState { }
    public class EarthwormAboveGround : EarthwormState { }
    public class EarthwormPursuit : EarthwormState { }
    public class EarthwormRetreat : EarthwormState { }

    public class EarthwormStateGraph : StateGraph
    {
        //public readonly Inactive inactive;
        public readonly EarthwormDormant dormant;
        public readonly EarthwormAboveGround aboveGround;
        public readonly EarthwormPursuit pursuit;
        public readonly EarthwormRetreat retreat;

        public EarthwormStateGraph() : base()
        {
            //inactive = CreateNewVertex<Inactive>();//for when input disabled (e.g. dead)
            dormant = CreateNewVertex<EarthwormDormant>();
            aboveGround = CreateNewVertex<EarthwormAboveGround>();
            pursuit = CreateNewVertex<EarthwormPursuit>();
            retreat = CreateNewVertex<EarthwormRetreat>();

            //AddEdgeBothWaysForAll(inactive);
            AddEdgeBothWaysForAll(dormant);
            AddEdgeBothWays((aboveGround, pursuit));
            AddEdgeBothWays((aboveGround, retreat));
        }
    }
}