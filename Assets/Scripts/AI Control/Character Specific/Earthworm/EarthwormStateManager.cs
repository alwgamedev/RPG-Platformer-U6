using RPGPlatformer.Core;
using UnityEngine;

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

            firstEmerge = true;
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
            animationControl.SetTrigger("goDormant");
            animationControl.ResetTrigger("emerge");
            animationControl.ResetTrigger("quickEmerge");
        }
    }
}