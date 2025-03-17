using System;
using RPGPlatformer.Core;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class GenericAIPatrollerController<T0, T1, T2, T3> : MonoBehaviour, IInputSource
        where T0 : AIPatroller
        where T1 : AIPatrollerStateGraph
        where T2 : AIPatrollerStateMachine<T1>
        where T3 : AIPatrollerStateManager<T1, T2, T0>
    {
        [SerializeField] protected PatrolMode defaultPatrolMode;
        [SerializeField] protected PatrolParemeters defaultPatrolParameters;

        protected T0 patroller;
        protected T3 stateManager;

        protected Action OnUpdate;
        protected bool stateBehaviorSubscribed;

        protected Dictionary<State, Action> StateBehavior = new();

        protected virtual void Awake()
        {
            patroller = GetComponent<T0>();
        }

        protected virtual void Start()
        {
            InitializeStateManager();
            ConfigureStateManager();
            SubscribeStateBehaviorToUpdate(true);
            InitializeState();
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void InitializeStateManager()
        {
            stateManager = (T3)Activator.CreateInstance(typeof(T3), null, patroller);
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
            patroller.TriggerPatrol();
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