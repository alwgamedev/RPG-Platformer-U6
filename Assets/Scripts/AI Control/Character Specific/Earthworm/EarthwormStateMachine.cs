using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class EarthwormStateMachine : StateMachine<EarthwormStateGraph>
    {
        AnimationControl animationControl;

        public EarthwormStateMachine(AnimationControl animationControl) : base()
        {
            this.animationControl = animationControl;
        }
    }

    public abstract class EarthwormState : State { }
    public class EarthwormDormant : EarthwormState { }
    public class EarthwormAboveGround : EarthwormState { }
    public class EarthwormPursuit : EarthwormState { }

    public class EarthwormStateGraph : StateGraph
    {
        public readonly EarthwormDormant dormant;
        public readonly EarthwormAboveGround aboveGround;
        public readonly EarthwormPursuit pursuit;

        public EarthwormStateGraph() : base()
        {
            dormant = CreateNewVertex<EarthwormDormant>();
            aboveGround = CreateNewVertex<EarthwormAboveGround>();
            pursuit = CreateNewVertex<EarthwormPursuit>();
        }
    }
}