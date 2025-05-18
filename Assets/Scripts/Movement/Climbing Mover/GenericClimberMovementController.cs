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
                stateDriver.UpdateClimb(InSwingMode() ? 0 : MoveInput.x, climbingMovementOptions);
            }
        }

        protected override void HandleMoveInput()
        {
            if (!Climbing)
            {
                base.HandleMoveInput();
            }
            else if (MoveInput.x != 0 && InSwingMode() && stateDriver.ClimberData.currentNode)
            {
                stateDriver.ClimberData.currentNode.ApplyAcceleration(climbingMovementOptions.SwingPushAcceleration
                    * Mathf.Sign(MoveInput.x) * transform.right);
            }
        }

        protected virtual bool InSwingMode()
        {
            return false;
        }

        protected virtual async void OnClimbingEntry()
        {
            await MiscTools.DelayGameTime(0.1f, GlobalGameTools.Instance.TokenSource.Token);
            //^delay so you still get the initial impact with the climbable (e.g. if it's a swinging rope)
            if (Climbing)
            {
                stateDriver.OnBeginClimb();
            }
        }

        protected virtual void OnClimbingExit()
        {
            //just to be extra sure rotation, rigidbody, and collider get reset
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
                stateManager.AnimateSwinging(SwingSpeedFraction());

                if (!InSwingMode())
                {
                    stateManager.AnimateClimbing(MoveInput.x);
                }
            }
        }

        protected float SwingSpeedFraction()
        {
            if (!stateDriver.ClimberData.currentNode || !stateDriver.ClimberData.currentNode.Rigidbody) 
                return 0;

            return stateDriver.ClimberData.currentNode.Rigidbody.linearVelocity.magnitude
                / 5 * stateDriver.RunSpeed;
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