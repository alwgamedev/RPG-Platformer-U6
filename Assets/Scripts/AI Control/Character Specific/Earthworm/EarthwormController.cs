using UnityEngine;
using RPGPlatformer.Core;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(EarthwormDriver))]
    public class EarthwormController : StateDrivenController<EarthwormStateManager,
        EarthwormStateGraph, EarthwormStateMachine, EarthwormDriver>, IInputSource
    {
        //will add an update method at some point once I figure things out more
        //(i.e. who does the timer)

        Action OnUpdate;
        Dictionary<State, Action> StateBehavior = new();

        bool AboveGround => stateManager.StateMachine.CurrentState == stateManager.StateGraph.aboveGround;

        protected override void Start()
        {
            base.Start();

            stateDriver.InitializeState();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        protected override void InitializeStateManager()
        {
            stateManager = (EarthwormStateManager)Activator.CreateInstance(typeof(EarthwormStateManager),
                null, stateDriver, GetComponentInChildren<AnimationControl>());
        }

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.dormant.OnEntry += async () =>  await OnDormantEntry();
            stateManager.StateGraph.aboveGround.OnEntry += async () => await OnAboveGroundEntry();
            stateManager.StateGraph.aboveGround.OnExit += OnAboveGroundExit;
            stateManager.StateGraph.pursuit.OnEntry += async () => await OnPursuitEntry();
        }


        //STATE BEHAVIOR


        //STATE TRANSITION HANDLERS

        private async Task OnDormantEntry()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

            try
            {
                stateManager.StateGraph.dormant.OnExit += EarlyExitHandler;
                await stateDriver.Retreat(cts.Token);
                stateManager.StateGraph.dormant.OnExit -= EarlyExitHandler;
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                stateManager.StateGraph.dormant.OnExit -= EarlyExitHandler;
            }

            void EarlyExitHandler()
            {
                cts.Cancel();
                stateManager.StateGraph.dormant.OnExit -= EarlyExitHandler;
            }
        }

        private async Task OnAboveGroundEntry()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

            try
            {
                stateDriver.FacePlayer();
                stateManager.StateGraph.aboveGround.OnExit += EarlyExitHandler;
                await stateDriver.Emerge(cts.Token);
                stateManager.StateGraph.aboveGround.OnExit -= EarlyExitHandler;
                //OnEmerged will be triggered in anim. event in the emerge animation
                //I think we DO  want invincibility, because it's really anticlimactic if it just dies while undergound
                //or emerging/retreating
                //-- add BLOCKED damage popups (so it doesn't feel like combat is glitching and not registering hits)
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                stateManager.StateGraph.aboveGround.OnExit -= EarlyExitHandler;
            }

            void EarlyExitHandler()
            {
                cts.Cancel();
                stateManager.StateGraph.aboveGround.OnExit -= EarlyExitHandler;
            }

            //Debug.Log("emerging");
            //await stateDriver.Emerge(GlobalGameTools.Instance.TokenSource.Token);

            //Debug.Log("emerged");
        }

        //triggered in anim event
        public void OnEmerged()
        {
            if (AboveGround)
            {
                stateDriver.SetAutoRetaliate(true);
                stateDriver.SetInvincible(false);
                stateDriver.StartAttacking();
            }
        }

        private void OnAboveGroundExit()
        {
            stateDriver.DisableIK();
            stateDriver.SetAutoRetaliate(false);
            stateDriver.SetInvincible(true);
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

            try
            {
                stateManager.StateGraph.pursuit.OnExit += EarlyExitHandler;
                await stateDriver.Retreat(cts.Token);
                stateManager.StateGraph.pursuit.OnExit -= EarlyExitHandler;
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                stateManager.StateGraph.pursuit.OnExit -= EarlyExitHandler;
            }

            void EarlyExitHandler()
            {
                cts.Cancel();
                stateManager.StateGraph.pursuit.OnExit -= EarlyExitHandler;
            }
        }


        //DISABLE/ENABLE INPUT

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
