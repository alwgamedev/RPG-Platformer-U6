using System;
using RPGPlatformer.Core;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class GenericAIPatrollerController<T0, T00, T01, T02, T03, T1, T2, T3, T4> 
        : StateDrivenController<T4, T2, T3, T1>, IInputSource, IAIPatrollerController
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

        protected Action OnUpdate;
        protected bool stateBehaviorSubscribed;
        protected object defaultPatrolParams;
        //in case you want to supply different default params (non-mb)

        protected Dictionary<State, Action> StateBehavior = new();

        public bool Patrolling => stateManager.StateMachine.CurrentState == stateManager.StateGraph.patrol;
        public IMovementController MovementController => stateDriver.MovementController;

        protected override void Awake()
        {
            base.Awake();

            if (defaultPatrolParameters)
            {
                defaultPatrolParams = defaultPatrolParameters.Content;
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

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.patrol.OnEntry += OnPatrolEntry;
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
            BeginPatrol(defaultPatrolMode, defaultPatrolParams);
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