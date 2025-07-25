using UnityEngine;
using RPGPlatformer.Core;
using System.Threading;
using System.Threading.Tasks;
using System;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(EarthwormDriver))]
    [RequireComponent(typeof(MonoBehaviorInputConfigurer))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class EarthwormController : StateDrivenController<EarthwormStateManager,
        EarthwormStateGraph, EarthwormStateMachine, EarthwormDriver>, IInputSource, IPausable
    {
        Action OnUpdate;

        bool AboveGround => stateManager.StateMachine.CurrentState == stateManager.StateGraph.aboveGround;

        public bool IsInputDisabled { get; private set; }

        public event Action InputEnabled;
        public event Action InputDisabled;

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
            stateManager.StateMachine.StateChange += DisableHitEffects;
        }


        //STATE TRANSITION HANDLERS

        private void DisableHitEffects(State state)
        {
            if (state != stateManager.StateGraph.aboveGround)
            {
                stateDriver.SetHitEffectsActive(false);
            }
        }

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
                stateDriver.FaceTarget();
                stateDriver.SetHitEffectsActive(true);
                await stateDriver.AboveGroundTimer(cts.Token);
                while (stateDriver.CombatController.ChannelingAbility)
                {
                    await Task.Yield();
                    if (GlobalGameTools.Instance.TokenSource.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                }
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
                OnUpdate = stateDriver.AboveGroundBehavior;
                stateDriver.SetAutoRetaliate(true);
                stateDriver.SetInvincible(false);
                stateDriver.EnableContactColliders();
                stateDriver.StartAttacking();
            }
        }

        private void OnAboveGroundExit()
        {
            OnUpdate = null;
            stateDriver.DisableAllIK();
            stateDriver.SetAutoRetaliate(false);
            stateDriver.SetInvincible(true);
            stateDriver.DisableContactColliders();//so that he doesn't drag the player underground
            stateDriver.StopAttacking();
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
                await stateDriver.TunnelTowardsWormhole(cts.Token);
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

        public void Pause()
        {
            DisableInput();
        }

        public void Unpause()
        {
            EnableInput();
        }

        public void EnableInput()
        {
            stateManager.Unfreeze();
            IsInputDisabled = false;
            InputEnabled?.Invoke();
        }

        public void DisableInput()
        {
            OnUpdate = null;
            stateManager.Freeze();
            IsInputDisabled = true;
            InputDisabled?.Invoke();
        }

        private void OnDestroy()
        {
            InputEnabled = null;
            InputDisabled = null;
        }
    }
}
