using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AIPatroller))]
    public class AIPatrollerController : MonoBehaviour, IInputSource
    {
        [SerializeField] protected bool playerEnemy = true;

        protected AIPatroller patroller;
        protected AIPatrollerStateManager stateManager;
        protected AIPatrollerStateMachine stateMachine;
        protected Action OnUpdate;
        protected bool stateBehaviorSubscribed;

        protected Dictionary<State, Action> StateBehavior = new();

        public IHealth CurrentTarget
        {
            get => patroller.combatController.currentTarget;
            protected set
            { 
                patroller.combatController.currentTarget = value;
                patroller.movementController.currentTarget = value;
            }
        }

        protected virtual void Awake()
        {
            patroller = GetComponent<AIPatroller>();
        }

        private void Start()
        {
            InitializeStateManager();

            if (playerEnemy)
            {
                SetCurrentTarget(GameObject.Find("Player").GetComponent<IHealth>());
            }
            else
            {
                SetCurrentTarget(null);
            }

            SubscribeStateBehavior(true);
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void InitializeStateManager()
        {
            stateManager = new(null, patroller);
            stateManager.Configure();
            stateMachine = stateManager.StateMachine;

            stateManager.StateGraph.patrol.OnEntry += OnPatrolEntry;
            //stateManager.StateGraph.patrol.OnExit += OnPatrolExit;
            //stateManager.StateGraph.suspicion.OnEntry += OnSuspicionEntry;
            //stateManager.StateGraph.suspicion.OnExit += OnSuspicionExit;
            stateManager.StateGraph.pursuit.OnEntry += OnPursuitEntry;
            stateManager.StateGraph.pursuit.OnExit += OnPursuitExit;
            stateManager.StateGraph.attack.OnEntry += OnAttackEntry;
            stateManager.StateGraph.attack.OnExit += OnAttackExit;

            StateBehavior[stateManager.StateGraph.inactive] = null;
            StateBehavior[stateManager.StateGraph.suspicion] = patroller.SuspicionBehavior;
            StateBehavior[stateManager.StateGraph.patrol] = patroller.PatrolBehavior;
            StateBehavior[stateManager.StateGraph.pursuit] = patroller.PursuitBehavior;
            StateBehavior[stateManager.StateGraph.attack] = null;
        }

        public void PerformStateBehavior()
        {
            if (StateBehavior.TryGetValue(stateMachine.CurrentState, out var action))
            {
                action?.Invoke();
            }
        }

        public void SubscribeStateBehavior(bool val)
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

        protected virtual void SetCurrentTarget(IHealth targetHealth, bool beginPatrolIfNoTarget = true)
        {
            CurrentTarget = targetHealth;
            if (targetHealth != null)
            {
                patroller.TriggerSuspicion();
            }
            else
            {
                patroller.TriggerPatrol();
            }
        }

        protected virtual void OnPatrolEntry()
        {
            if (CurrentTarget != null)
            {
                SetCurrentTarget(null, false);
            }
            patroller.movementController.SetRunning(false);
            //StateBehavior = patroller.PatrolBehavior;
        }

        //protected virtual void OnPatrolExit()
        //{
        //    OnUpdate -= patroller.PatrolBehavior;
        //}

        //protected virtual void OnSuspicionEntry()
        //{
        //    StateBehavior = patroller.SuspicionBehavior;
        //}

        //protected virtual void OnSuspicionExit()
        //{
        //    OnUpdate -= patroller.SuspicionBehavior;
        //}

        protected virtual void OnPursuitEntry()
        {
            //OnUpdate += patroller.PursuitBehavior;
            //StateBehavior = patroller.PursuitBehavior;
            patroller.movementController.SetRunning(true);
        }

        protected virtual void OnPursuitExit()
        {
            patroller.movementController.SetRunning(false);
            patroller.movementController.Stop();
            //OnUpdate -= patroller.PursuitBehavior;
        }

        protected virtual void OnAttackEntry()
        {
            //patroller.movementController.EnableJumping(false);
            //StateBehavior = null;
            patroller.StartAttacking();
        }

        protected virtual void OnAttackExit()
        {
            patroller.StopAttacking();
            //patroller.movementController.EnableJumping(true);
            //OnUpdate -= patroller.MaintainMinimumCombatDistance;
        }

        //protected virtual void OnDeath()
        //{
        //    OnUpdate = null;
        //    //StateBehavior = null;
        //}

        public void EnableInput()
        {
            stateManager.Unfreeze();
            SubscribeStateBehavior(true);
        }

        public void DisableInput()
        {
            SubscribeStateBehavior(false);
            stateManager.Freeze();
            stateManager.StateMachine.ForceCurrentState(stateManager.StateGraph.inactive);
        }

        private void OnDestroy()
        {
            OnUpdate = null;
        }
    }
}