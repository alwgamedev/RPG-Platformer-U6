using UnityEngine;
using RPGPlatformer.Core;
using System.Threading.Tasks;
using System.Threading;

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
            stateManager.StateGraph.climbing.OnExit += OnClimbingExit;
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
            else//doing this in FixedUpdate causes flickering (and it's not physics dependent anyway)
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
            else if (MoveInput.x != 0 && InSwingMode() && stateDriver.ClimberData.currentNode != null)
            {
                stateDriver.ClimberData.currentNode.ApplyAcceleration(climbingMovementOptions.SwingPushAcceleration
                    * Mathf.Sign(MoveInput.x) * transform.right);
            }
        }

        protected virtual bool InSwingMode()
        {
            return false;
        }

        protected virtual /*async*/ void OnClimbingEntry()
        {
            //await MiscTools.DelayGameTime(0.1f, GlobalGameTools.Instance.TokenSource.Token);
            //^delay so you still get the initial impact with the climbable (e.g. if it's a swinging rope)
            //^nvmd was stupid and looked buggy, plus we are happy to avoid async overhead when we can
            if (Climbing)
            {
                stateDriver.OnBeginClimb();
            }
        }

        protected virtual async void OnClimbingExit()
        {
            stateDriver.EndClimb();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

            try
            {
                stateManager.StateGraph.climbing.OnEntry += cts.Cancel;
                await MiscTools.DelayGameTime(1, cts.Token);
                stateDriver.EnableCollisionWithClimbables(true);
            }
            catch(TaskCanceledException)
            {
                return;
            }
            finally
            {
                stateManager.StateGraph.climbing.OnEntry -= cts.Cancel;
            }
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
            if (stateDriver.ClimberData.currentNode == null) 
                return 0;

            return 0.2f * stateDriver.ClimberData.currentNode.Speed * stateDriver.RunSpeed;
        }

        protected override bool CanBeginWallCling()
        {
            return !Climbing && base.CanBeginWallCling();
        }

        protected override bool CanAnimateWallScramble()
        {
            return !Climbing && base.CanAnimateWallScramble();
        }

        public override void OnDeath()
        {
            if (Climbing)
            {
                stateDriver.FallOffClimbable();
            }

            base.OnDeath();
        }
    }
}