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

        Action OnUpdate;

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
            stateManager = new EarthwormStateManager(null, stateDriver, 
                GetComponentInChildren<AnimationControl>());
        }

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.dormant.OnEntry += async () =>  await OnDormantEntry();
            stateManager.StateGraph.aboveGround.OnEntry += async () => await OnAboveGroundEntry();
            stateManager.StateGraph.aboveGround.OnExit += OnAboveGroundExit;
            stateManager.StateGraph.pursuit.OnEntry += async () => await OnPursuitEntry();
            stateManager.StateGraph.retreat.OnEntry += async () => await OnRetreatEntry();
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
                await stateDriver.Submerge(cts.Token);
                stateDriver.EnableWormholeTrigger(true);
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
                stateDriver.FaceTarget();
                stateManager.StateGraph.aboveGround.OnExit += EarlyExitHandler;
                await stateDriver.Emerge(cts.Token);
                await stateDriver.AboveGroundTimer(cts.Token);
                stateManager.StateGraph.aboveGround.OnExit -= EarlyExitHandler;

                stateDriver.AboveGroundTimerComplete();
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
        }

        //triggered in anim event
        public void OnEmerged()
        {
            if (AboveGround)
            {
                Debug.Log("on emerged");
                OnUpdate = stateDriver.AboveGroundBehavior;
                stateDriver.SetAutoRetaliate(true);
                stateDriver.SetInvincible(false);
                stateDriver.StartAttacking();
            }
        }

        private void OnAboveGroundExit()
        {
            OnUpdate = null;
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
                await stateDriver.Submerge(cts.Token);
                await stateDriver.PursueTarget(cts.Token);
                stateManager.StateGraph.pursuit.OnExit -= EarlyExitHandler;

                stateDriver.PursuitComplete();
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

        private async Task OnRetreatEntry()
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);

            try
            {
                stateManager.StateGraph.retreat.OnExit += EarlyExitHandler;
                await stateDriver.Submerge(cts.Token);
                stateDriver.ChooseRandomWormholePosition();
                stateDriver.GoToWormhole();
                stateDriver.EnableWormholeTrigger(true);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                stateManager.StateGraph.retreat.OnExit -= EarlyExitHandler;
            }

            void EarlyExitHandler()
            {
                cts.Cancel();
                stateManager.StateGraph.retreat.OnExit -= EarlyExitHandler;
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
