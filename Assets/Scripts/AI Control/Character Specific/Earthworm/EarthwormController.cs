using UnityEngine;
using RPGPlatformer.Core;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(EarthwormDriver))]
    public class EarthwormController : StateDrivenController<EarthwormStateManager,
        EarthwormStateGraph, EarthwormStateMachine, EarthwormDriver>, IInputSource
    {
        //will add an update method at some point once I figure things out more
        //(i.e. who does the timer)

        bool AboveGround => stateManager.StateMachine.CurrentState == stateManager.StateGraph.aboveGround;

        protected override void InitializeStateManager()
        {
            stateManager = (EarthwormStateManager)Activator.CreateInstance(typeof(EarthwormStateManager),
                null, stateDriver, GetComponentInChildren<AnimationControl>());
        }

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.dormant.OnEntry += () => Debug.Log("controller received dormant entry");
            stateManager.StateGraph.aboveGround.OnEntry += () => Debug.Log("controller received above ground entry");
            stateManager.StateGraph.dormant.OnEntry += async () =>  await OnDormantEntry();
            stateManager.StateGraph.aboveGround.OnEntry += async () => await OnAboveGroundEntry();
            stateManager.StateGraph.aboveGround.OnExit += OnAboveGroundExit;
            stateManager.StateGraph.pursuit.OnEntry += async () => await OnPursuitEntry();
        }

        private async Task OnDormantEntry()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

            Debug.Log("going dormant");
            
            stateManager.StateGraph.dormant.OnExit += EarlyExitHandler;
            await stateDriver.Retreat(cts.Token);
            stateManager.StateGraph.dormant.OnExit -= EarlyExitHandler;

            Debug.Log("dormant");

            void EarlyExitHandler()
            {
                cts.Cancel();//rn the innermost task will catch the exception
                stateManager.StateGraph.dormant.OnExit -= EarlyExitHandler;
            }
        }

        private async Task OnAboveGroundEntry()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

            Debug.Log("emerging");

            stateManager.StateGraph.aboveGround.OnExit += EarlyExitHandler;
            await stateDriver.Emerge(cts.Token);
            stateManager.StateGraph.aboveGround.OnExit -= EarlyExitHandler;

            Debug.Log("emerged");            
            //then OnEmerged will be triggered in anim. event in the emerge animation
            //I think we DO  want invincibility, because it's really anticlimactic if it just dies while undergound
            //or emerging/retreating
            //-- add BLOCKED damage popups (so it doesn't feel like combat is glitching and not registering hits)

            void EarlyExitHandler()
            {
                cts.Cancel();
                stateManager.StateGraph.aboveGround.OnExit -= EarlyExitHandler;
            }

            //Debug.Log("emerging");
            //await stateDriver.Emerge(GlobalGameTools.Instance.TokenSource.Token);

            //Debug.Log("emerged");
        }

        //trigger in ANIM EVENT
        private void OnEmerged()
        {
            if (AboveGround)
            {
                //+ turn off invincibility
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

        private async Task OnPursuitEntry()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

            Debug.Log("retreating to pursue");

            stateManager.StateGraph.pursuit.OnExit += EarlyExitHandler;
            await stateDriver.Retreat(cts.Token);
            stateManager.StateGraph.pursuit.OnExit -= EarlyExitHandler;

            Debug.Log("ready to pursue");
            //then move to a new wormhole, and re-emerge

            void EarlyExitHandler()
            {
                cts.Cancel();
                stateManager.StateGraph.pursuit.OnExit -= EarlyExitHandler;
            }
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
