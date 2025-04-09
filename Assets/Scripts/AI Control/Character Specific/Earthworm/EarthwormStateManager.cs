using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.AIControl
{

    public class EarthwormStateManager : StateManager<EarthwormStateGraph, EarthwormStateMachine, EarthwormDriver>
    {
        AnimationControl animationControl;

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
        }

        private void OnAboveGroundEntry()
        {
            animationControl.SetTrigger("emerge");
            animationControl.ResetTrigger("goDormant");
        }

        private void OnDormantEntry()
        {
            animationControl.SetTrigger("goDormant");
            animationControl.ResetTrigger("emerge");
        }
    }
}