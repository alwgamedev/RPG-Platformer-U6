using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    public class GenericClimberMovementController<T0, T1, T2, T3> 
        : GenericAdvancedMovementController<T0, T1, T2, T3>
        where T0 : Climber
        where T1 : ClimberMovementStateGraph
        where T2 : ClimberMovementStateMachine<T1>
        where T3 : ClimberMovementStateManager<T1, T2, T0>
    {
        [SerializeField] protected ClimbingMovementOptions climbingMovementOptions;

        public bool Climbing => stateManager.StateMachine.CurrentState == stateManager.StateGraph.climbing;

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.climbing.OnEntry += OnClimbingEntry;
        }

        //TO-DO: may also EndClimb when input is disabled? bc then getting stunned
        //would make you fall off any climbable object... if a scenario arises where that's possible we can
        //consider it

        protected override void UpdateMover()
        {
            if (!Climbing)
            {
                base.UpdateMover();
            }
            else//doing this in FixedUpdate causes flickering
            {
                stateDriver.UpdateClimb(MoveInput.x, climbingMovementOptions);
            }
        }

        protected override void HandleMoveInput()
        {
            if (!Climbing)
            {
                base.HandleMoveInput();
            }
            //else
            //{
            //    stateDriver.UpdateClimb(MoveInput.x, climbingMovementOptions);
            //    //if (Climbing && stateDriver.ClimberData.currentNode)
            //    //{
            //    //    FaceTarget(stateDriver.ClimberData.currentNode.transform.position);
            //    //}
            //}
        }

        protected virtual async void OnClimbingEntry()
        {
            FaceTarget(stateDriver.ClimberData.currentNode.transform);
            await MiscTools.DelayGameTime(0.1f, GlobalGameTools.Instance.TokenSource.Token);
            //so you still get the initial impact with the climbable (e.g. if it's a swinging rope)
            if (Climbing)
            {
                stateDriver.OnBeginClimb();
            }
        }

        protected virtual void OnClimbingExit()
        {
            //just to be extra sure rotation and rigidbody get reset
            stateDriver.EndClimb(false);
        }

        protected virtual void TryGrabOntoClimbableObject()
        {
            if (!Climbing)
            {
                stateDriver.TryGrabOntoClimbableObject(climbingMovementOptions);
            }
        }

        protected override void AnimateMovement()
        {
            if (!Climbing)
            {
                base.AnimateMovement();
            }
            else
            {
                stateManager.AnimateClimbing(MoveInput.x);
            }
        }

        public override void OnDeath()
        {
            if (Climbing)
            {
                stateDriver.EndClimb(true);
            }

            base.OnDeath();
        }
    }
}