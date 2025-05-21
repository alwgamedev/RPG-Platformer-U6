using System;
using RPGPlatformer.Core;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Movement;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(MonoBehaviorInputConfigurer))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class GenericAIPatrollerController<T, T1, T2, T3, T4> 
        : StateDrivenController<T4, T2, T3, T1>, IInputSource, IAIPatrollerController, IPausable
        where T : IAIMovementController
        where T1 : GenericAIPatroller<T>
        where T2 : AIPatrollerStateGraph
        where T3 : AIPatrollerStateMachine<T2>
        where T4 : AIPatrollerStateManager<T2, T3, T, T1>
    {
        [SerializeField] protected NavigationMode defaultPatrolMode;
        [SerializeField] protected MBNavigationParameters defaultPatrolParameters;

        protected Action OnUpdate;
        protected bool stateBehaviorSubscribed;
        //protected object defaultPatrolParams;
        //in case you want to supply different default params (non-mb)

        protected Dictionary<State, Action> StateBehavior = new();

        public bool IsInputDisabled { get; protected set; }
        public bool Patrolling => stateManager.StateMachine.CurrentState == stateManager.StateGraph.patrol;
        public IAIPatroller Patroller => stateDriver;
        public object DefaultPatrolParams { get; set; }

        public event Action InputEnabled;
        public event Action InputDisabled;

        protected override void Awake()
        {
            base.Awake();

            if (defaultPatrolParameters)
            {
                DefaultPatrolParams = defaultPatrolParameters.Content;
            }
        }

        protected override void Start()
        {
            base.Start();

            BuildStateBehaviorDict();
            SubscribeStateBehaviorToUpdate(true);

            stateDriver.PatrolNavigator.DestinationReached += OnDestinationReached;

            stateDriver.InitializeState();
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        //public void SetDefaultPatrolParameters(MBNavigationParameters p)
        //{
        //    defaultPatrolParameters = p;
        //    DefaultPatrolParams = p.Content;
        //}

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.patrol.OnEntry += OnPatrolEntry;
            //stateManager.StateMachine.StateChange += 
            //    s => Debug.Log($"Patroller {gameObject.name}: {s.name}");//useful for testing
        }

        protected virtual void BuildStateBehaviorDict()
        {
            StateBehavior[stateManager.StateGraph.inactive] = null;
            StateBehavior[stateManager.StateGraph.patrol] = stateDriver.PatrolBehavior;
        }

        protected void PerformStateBehavior()
        {
            if (StateBehavior.TryGetValue(stateManager.StateMachine.CurrentState, out var action))
            {
                action?.Invoke();
            }
        }

        protected void SubscribeStateBehaviorToUpdate(bool val)
        {
            if (val == stateBehaviorSubscribed) return;

            if (val)
            {
                OnUpdate += PerformStateBehavior;
                stateBehaviorSubscribed = true;
            }
            else
            {
                OnUpdate -= PerformStateBehavior;
                stateBehaviorSubscribed = false;
            }
        }

        protected virtual void OnPatrolEntry()
        {
            stateDriver.MovementController.SetRunning(false);
            BeginDefaultPatrol();
        }

        public virtual void BeginDefaultPatrol()
        {
            BeginPatrol(defaultPatrolMode, DefaultPatrolParams);
        }

        public virtual void BeginPatrolRest()
        {
            BeginPatrol(NavigationMode.rest, null);
        }

        public virtual void BeginTimedPatrolRest(float time)
        {
            BeginPatrol(NavigationMode.timedRest, time);
        }

        public virtual void BeginPatrol(NavigationMode mode, object param)
        {
            stateDriver.BeginPatrol(mode, param);
        }

        protected virtual void OnDestinationReached()
        {
            if (!stateDriver.PatrolNavigator.GetNextDestination())
            {
                BeginPatrolRest();
            }
        }

        public void Pause()
        {
            DisableInput();
        }

        public void Unpause()
        {
            EnableInput();
        }

        public virtual void EnableInput()
        {
            stateManager.Unfreeze();
            SubscribeStateBehaviorToUpdate(true);
            IsInputDisabled = false;
            InputEnabled?.Invoke();
        }

        public virtual void DisableInput()
        {
            SubscribeStateBehaviorToUpdate(false);
            stateManager.Freeze();
            stateManager.StateMachine.ForceCurrentState(stateManager.StateGraph.inactive);
            IsInputDisabled = true;
            InputDisabled?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            OnUpdate = null;
            InputEnabled = null;
            InputDisabled = null;
        }
    }
}