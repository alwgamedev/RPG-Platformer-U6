using RPGPlatformer.Core;

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

            //will also subscribe animateclimbing(true) here (to set an initial trigger)
            //just see what's done in base movement state manager
        }

        //animate climbing(bool val) (on climbing entry, set a trigger)
        //animating climbing movement(moveInput) (if no moveinput trigger back to idle climbing pose)
    }
}