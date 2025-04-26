using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class ClimberMovementStateManager : ClimberMovementStateManager<ClimberMovementStateGraph,
        ClimberMovementStateMachine, Climber>
    {
        public ClimberMovementStateManager(Climber climber, AnimationControl animationControl)
            : base(climber, animationControl) { }
    }

    public class ClimberMovementStateManager<T0, T1, T2> : AdvancedMovementStateManager<T0, T1, T2>
        where T0 : ClimberMovementStateGraph
        where T1 : ClimberMovementStateMachine<T0>
        where T2 : Climber
    {
        public ClimberMovementStateManager(T2 climber, AnimationControl animationControl)
            : base(climber, animationControl) { }

        public override void Configure()
        {
            base.Configure();

            StateGraph.climbing.OnEntry += OnClimbingEntry;
            StateGraph.climbing.OnExit += OnClimbingExit;
        }

        public void OnClimbingEntry()
        {
            animationControl.SetBool("climbing", true);
        }

        public void OnClimbingExit()
        {
            animationControl.SetBool("climbing", false);
        }

        public void AnimateClimbing(float moveInput)
        {
            animationControl.SetFloat("climbVelocity", moveInput);
        }

        public void AnimateSwinging(float swingSpeed)
        {
            animationControl.SetFloat("swingSpeed", swingSpeed, 1f, Time.deltaTime);
        }
    }
}