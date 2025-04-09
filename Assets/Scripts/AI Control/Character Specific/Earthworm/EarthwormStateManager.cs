using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{

    public class EarthwormStateManager : StateManager<EarthwormStateGraph, EarthwormStateMachine, EarthwormDriver>
    {
        AnimationControl animationControl;
        
        public bool firstEmerge = true;

        public EarthwormStateManager(EarthwormStateMachine stateMachine,
            EarthwormDriver stateDriver, AnimationControl animationControl)
            : base(stateMachine, stateDriver)
        {
            this.animationControl = animationControl;
        }

        public override void Configure()
        {
            base.Configure();

            StateGraph.aboveGround.OnEntry += OnAboveGroundEntry;
            StateGraph.dormant.OnEntry += OnDormantEntry;
            StateGraph.pursuit.OnEntry += OnPursuitEntry;
            StateGraph.retreat.OnEntry += OnRetreatEntry;
        }

        private void OnAboveGroundEntry()
        {
            if (firstEmerge)
            {
                firstEmerge = false;
                animationControl.SetTrigger("emerge");
            }
            else
            {
                animationControl.SetTrigger("quickEmerge");
            }

            animationControl.ResetTrigger("goDormant");
        }

        private void OnDormantEntry()
        {
            firstEmerge = true;
            AnimateSubmerge();
        }

        private void OnPursuitEntry()
        {
            AnimateSubmerge();
        }

        private void OnRetreatEntry()
        {
            AnimateSubmerge();
        }

        private void AnimateSubmerge()
        {
            animationControl.SetTrigger("goDormant");
            animationControl.ResetTrigger("emerge");
            animationControl.ResetTrigger("quickEmerge");
        }
    }
}