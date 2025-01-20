using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AIPatroller))]
    public class AIPatrollerController : MonoBehaviour, IInputSource
    {
        [SerializeField] protected bool playerEnemy = true;

        protected AIPatroller patroller;
        protected AIPatrollerStateManager stateManager;
        protected Action OnUpdate;

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
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void InitializeStateManager()
        {
            stateManager = new(null, patroller);
            stateManager.Configure();

            stateManager.StateGraph.patrol.OnEntry += OnPatrolEntry;
            stateManager.StateGraph.patrol.OnExit += OnPatrolExit;
            stateManager.StateGraph.suspicion.OnEntry += OnSuspicionEntry;
            stateManager.StateGraph.suspicion.OnExit += OnSuspicionExit;
            stateManager.StateGraph.pursuit.OnEntry += OnPursuitEntry;
            stateManager.StateGraph.pursuit.OnExit += OnPursuitExit;
            stateManager.StateGraph.attack.OnEntry += OnAttackEntry;
            stateManager.StateGraph.attack.OnExit += OnAttackExit;
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
            OnUpdate += patroller.PatrolBehavior;
        }

        protected virtual void OnPatrolExit()
        {
            OnUpdate -= patroller.PatrolBehavior;
        }

        protected virtual void OnSuspicionEntry()
        {
            OnUpdate += patroller.SuspicionBehavior;
        }

        protected virtual void OnSuspicionExit()
        {
            OnUpdate -= patroller.SuspicionBehavior;
        }

        protected virtual void OnPursuitEntry()
        {
            OnUpdate += patroller.PursuitBehavior;
            patroller.movementController.SetRunning(true);
        }

        protected virtual void OnPursuitExit()
        {
            patroller.movementController.SetRunning(false);
            patroller.movementController.MoveInput = 0;
            OnUpdate -= patroller.PursuitBehavior;
        }

        protected virtual void OnAttackEntry()
        {
            patroller.movementController.EnableJumping(false);
            patroller.StartAttacking();
        }

        protected virtual void OnAttackExit()
        {
            patroller.StopAttacking();
            patroller.movementController.EnableJumping(true);
            //OnUpdate -= patroller.MaintainMinimumCombatDistance;
        }

        protected virtual void OnDeath()
        {
            OnUpdate = null;
        }

        public void EnableInput()
        {
            stateManager.Unfreeze();
        }

        public void DisableInput()
        {
            OnUpdate = null;
            stateManager.Freeze();
            stateManager.StateMachine.ForceCurrentState(stateManager.StateGraph.inactive);
        }

        private void OnDestroy()
        {
            OnUpdate = null;
        }
    }
}