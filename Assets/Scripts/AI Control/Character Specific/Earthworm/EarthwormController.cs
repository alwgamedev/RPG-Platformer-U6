using RPGPlatformer.Core;
using System;

namespace RPGPlatformer.AIControl
{
    public class EarthwormController : StateDrivenController<EarthwormStateManager,
        EarthwormStateGraph, EarthwormStateMachine, EarthwormDriver>, IInputSource
    {
        //will add an update method at some point once I figure things out more
        //(i.e. who does the timer)

        bool AboveGround => stateManager.StateMachine.CurrentState == stateManager.StateGraph.aboveGround;

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.dormant.OnEntry += OnDormantEntry;
            stateManager.StateGraph.aboveGround.OnEntry += OnAboveGroundEntry;
            stateManager.StateGraph.aboveGround.OnExit += OnAboveGroundExit;
            stateManager.StateGraph.pursuit.OnEntry += OnPursuitEntry;
        }

        private async void OnDormantEntry()
        {
            stateDriver.SetBodyAnchor(false);
            await stateDriver.MoveToAnchorPosition(GlobalGameTools.Instance.TokenSource.Token);
        }

        private async void OnAboveGroundEntry()
        {
            await stateDriver.Emerge(GlobalGameTools.Instance.TokenSource.Token);
            //then maybe wait 1 or 2 secs and start attacking
            //(OR TRIGGER START ATTACKING IN *ANIM. EVENT*)
            //(+ turn off invincibility)
            //I think we DO  want invincibility, because it's really anticlimactic if it just dies while undergound
            //or emerging/retreating
            //-- add BLOCKED damage popups (so it doesn't feel like combat is glitching and not registering hits)
            stateDriver.FacePlayer();
        }

        //trigger in ANIM EVENT
        private void OnEmerged()
        {
            if (AboveGround)
            {
                stateDriver.FacePlayer();
                stateDriver.StartAttacking();
            }
        }

        private void OnAboveGroundExit()
        {
            stateDriver.DisableIK();
            stateDriver.StopAttacking();
            //+turn on invincibility
            //INVINCIBILITY: just worm takes no damage
            //(to make life easy, bleeds will continue to hit, but deal 0 damage)
            //Let's also make sure the worm is immune to stuns
        }

        private async void OnPursuitEntry()
        {
            await stateDriver.Retreat(GlobalGameTools.Instance.TokenSource.Token);
            //then move to a new wormhole, and re-emerge (after a second or two)
        }

        //These will get called OnDeath/OnRevival from CombatController,
        //so think if there is anything else you should put here
        public void EnableInput()
        {
            stateManager.Unfreeze();
        }

        public void DisableInput()
        {
            stateManager.Freeze();
            stateManager.StateMachine.ForceCurrentState(stateManager.StateGraph.inactive);
        }
    }
}
