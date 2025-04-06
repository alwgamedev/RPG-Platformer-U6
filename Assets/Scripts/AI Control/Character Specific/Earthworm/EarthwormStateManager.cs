using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class EarthwormStateManager : StateManager<EarthwormStateGraph, EarthwormStateMachine, EarthwormDriver>
    {
        AnimationControl animationControl;

        public EarthwormStateManager(EarthwormStateMachine stateMachine, EarthwormDriver stateDriver, 
            AnimationControl animationControl)
            : base(stateMachine, stateDriver)
        {

        }
    }
}