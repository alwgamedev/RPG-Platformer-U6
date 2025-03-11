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
        [SerializeField] bool forgetCombatTargetOnPatrolEntry;

        protected AIPatroller patroller;
        protected AIPatrollerStateManager stateManager;
        protected AIPatrollerStateMachine stateMachine;
        protected Action OnUpdate;
        protected bool stateBehaviorSubscribed;

        protected Dictionary<State, Action> StateBehavior = new();

        //public IHealth CurrentTarget
        //{
        //    get => patroller.combatController.currentTarget;
        //    protected set
        //    {
        //        if (patroller.combatController != null)
        //        {
        //            patroller.combatController.currentTarget = value;
        //        }
        //        patroller.movementController.currentTarget = value;
        //    }
        //}

        protected virtual void Awake()
        {
            patroller = GetComponent<AIPatroller>();
        }

        private void Start()
        {
            InitializeStateManager();

            if (playerEnemy)
            {
                patroller.SetCombatTarget(GameObject.Find("Player").GetComponent<IHealth>());
            }
            else
            {
                patroller.SetCombatTarget(null);
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
            stateManager.StateGraph.suspicion.OnExit += OnSuspicionExit;
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

        protected virtual void OnPatrolEntry()
        {
            //if (forgetCombatTargetOnPatrolEntry && patroller.CombatTarget != null)
            //{
            //    patroller.SetCombatTarget(null);
            //    //???? but this triggers patrol again and you get stuck in a loop?
            //}

            patroller.MovementController.SetRunning(false);
        }

        protected virtual void OnSuspicionExit()
        {
            patroller.ResetSuspicionTimer();
        }

        protected virtual void OnPursuitEntry()
        {
            patroller.MovementController.SetRunning(true);
        }

        protected virtual void OnPursuitExit()
        {
            patroller.MovementController.SetRunning(false);
            patroller.MovementController.Stop();
        }

        protected virtual void OnAttackEntry()
        {
            patroller.StartAttacking();
        }

        protected virtual void OnAttackExit()
        {
            patroller.StopAttacking();
        }

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