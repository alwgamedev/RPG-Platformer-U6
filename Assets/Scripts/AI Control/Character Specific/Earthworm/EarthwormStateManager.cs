using JetBrains.Annotations;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class EarthwormStateManager : StateManager<EarthwormStateGraph, EarthwormStateMachine, EarthwormDriver>
    {
        AnimationControl animationControl;

        public EarthwormStateManager(EarthwormStateMachine stateMachine, EarthwormDriver stateDriver,
            AnimationControl animationControl) : base(stateMachine, stateDriver) { }

        public override void Configure()
        {
            StateGraph.aboveGround.OnEntry += OnAboveGroundEntry;
            StateGraph.aboveGround.OnExit += OnAboveGroundExit;
        }

        private void OnAboveGroundEntry()
        {
            animationControl.SetTrigger("emerge");
            animationControl.ResetTrigger("goDormant");
        }

        private void OnAboveGroundExit()
        {
            animationControl.SetTrigger("goDormant");
            animationControl.ResetTrigger("aboveGround");
        }
    }
}