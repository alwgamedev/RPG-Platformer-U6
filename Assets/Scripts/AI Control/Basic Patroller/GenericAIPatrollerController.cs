using System;
using RPGPlatformer.Core;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class GenericAIPatrollerController<T0, T00, T01, T02, T03, T1, T2, T3, T4> : MonoBehaviour, IInputSource
        where T0 : GenericAdvancedMovementController<T00, T01, T02, T03>
        where T00 : AdvancedMover
        where T01 : AdvancedMovementStateGraph
        where T02 : AdvancedMovementStateMachine<T01>
        where T03 : AdvancedMovementStateManager<T01, T02, T00>
        where T1 : GenericAIPatroller<T0, T00, T01, T02, T03>
        where T2 : AIPatrollerStateGraph
        where T3 : AIPatrollerStateMachine<T2>
        where T4 : AIPatrollerStateManager<T2, T3, T0, T00, T01, T02, T03, T1>
    {
        [SerializeField] protected NavigationMode defaultPatrolMode;
        [SerializeField] protected MbNavigationParameters defaultPatrolParameters;

        protected T1 patroller;
        protected T4 stateManager;

        protected Action OnUpdate;
        protected bool stateBehaviorSubscribed;
        protected object defaultPatrolParams;//in case you want to supply different default params (non-mb)

        protected Dictionary<State, Action> StateBehavior = new();

        public bool Patrolling => stateManager.StateMachine.CurrentState == stateManager.StateGraph.patrol;

        protected virtual void Awake()
        {
            patroller = GetComponent<T1>();
            if (defaultPatrolParameters)
            {
                defaultPatrolParams = defaultPatrolParameters.Content;
            }
        }

        protected virtual void Start()
        {
            InitializeStateManager();
            ConfigureStateManager();
            SubscribeStateBehaviorToUpdate(true);

            patroller.PatrolNavigator.DestinationReached += OnPatrolDestinationReached;

            InitializeState();
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void InitializeStateManager()
        {
            stateManager = (T4)Activator.CreateInstance(typeof(T4), null, patroller);
        }

        protected virtual void ConfigureStateManager()
        {
            stateManager.Configure();

            stateManager.StateGraph.patrol.OnEntry += OnPatrolEntry;

            StateBehavior[stateManager.StateGraph.inactive] = null;
            StateBehavior[stateManager.StateGraph.patrol] = patroller.PatrolBehavior;
        }

        protected virtual void InitializeState()
        {
            BeginDefaultPatrol();
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
            patroller.MovementController.SetRunning(false);
            patroller.BeginPatrol(defaultPatrolMode, defaultPatrolParameters);
        }

        protected virtual void BeginDefaultPatrol()
        {
            patroller.BeginPatrol(defaultPatrolMode, defaultPatrolParams);
        }

        protected virtual void OnPatrolDestinationReached()
        {
            if (Patrolling && !patroller.PatrolNavigator.GetNextDestination())
            {
                patroller.BeginPatrol(NavigationMode.rest, null);
            }
        }

        public virtual void EnableInput()
        {
            stateManager.Unfreeze();
            SubscribeStateBehaviorToUpdate(true);
        }

        public virtual void DisableInput()
        {
            SubscribeStateBehaviorToUpdate(false);
            stateManager.Freeze();
            stateManager.StateMachine.ForceCurrentState(stateManager.StateGraph.inactive);
        }

        protected virtual void OnDestroy()
        {
            OnUpdate = null;
        }
    }
}