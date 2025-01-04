using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    //this will be a controller of a state system (with states patrol, pursue, attack)
    //-- can pursue any game object (or Health maybe), not just player, so maybe you could have some areas
    //where two factions of ai's are fighting each other, or their could be FRIENDLY ai and player companions
    //that help you in combat
    //it will also delegate to two sub-systems:
    //(*)Combat Controller - responsible for executing abilities through the Combatant
    //(*)Movement Controller - responsible for moving to destination through the Mover

    [RequireComponent(typeof(AIPatroller))]
    public class AIPatrollerController : MonoBehaviour, IInputSource
    {
        protected AIPatroller patroller;
        protected AIPatrollerStateManager stateManager;
        protected Health currentTarget;
        protected Action OnUpdate;

        protected string currentState;//for testing
        protected string storedState;

        protected Health CurrentTarget
        {
            get => currentTarget;
            set
            {
                currentTarget = value;
                patroller.combatController.currentTarget = value;
                if (value != null)
                {
                    patroller.BecomeSuspicious();
                }
                else
                {
                    stateManager.StateMachine.SetCurrentState(stateManager.StateGraph.patrol);
                }
            }
        }

        protected virtual void Awake()
        {
            patroller = GetComponent<AIPatroller>();

            stateManager = new(null, patroller);
            stateManager.Configure();
        }

        private void OnEnable()
        {
            stateManager.StateGraph.patrol.OnEntry += OnPatrolEntry;
            stateManager.StateGraph.suspicion.OnEntry += OnSuspicionEntry;
            stateManager.StateGraph.pursuit.OnEntry += OnPursuitEntry;
            stateManager.StateGraph.pursuit.OnExit += OnPursuitExit;
            stateManager.StateGraph.attack.OnEntry += OnAttackEntry;
            stateManager.StateGraph.attack.OnExit += OnAttackExit;

            stateManager.StateMachine.StateChange += (state) => currentState = state.GetType().Name;
            stateManager.StateMachine.StateStored += (state) => storedState = state.GetType().Name;
        }

        private void Start()
        {
            CurrentTarget = GameObject.Find("Player").GetComponent<Health>();
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void OnPatrolEntry()
        {
            //Debug.Log($"{gameObject.name} entering Patrol");
            CurrentTarget = null;
            patroller.movementController.SetRunning(false);
            OnUpdate = patroller.PatrolBehavior;
        }

        protected virtual void OnSuspicionEntry()
        {
            //Debug.Log($"{gameObject.name} entering Suspicion");
            OnUpdate = () => patroller.SuspicionBehavior(currentTarget);
        }

        protected virtual void OnPursuitEntry()
        {
            //Debug.Log($"{gameObject.name} entering Pursuit");
            OnUpdate = () => patroller.PursuitBehavior(currentTarget);
            patroller.movementController.SetRunning(true);
        }

        protected virtual void OnPursuitExit()
        {
            patroller.movementController.moveInput = 0;
        }

        protected virtual void OnAttackEntry()
        {
            //Debug.Log("Attack");
            OnUpdate = null;
            patroller.StartAttacking();
        }

        protected virtual void OnAttackExit()
        {
            patroller.StopAttacking();
        }

        protected virtual void OnDeath()
        {
            OnUpdate = null;
        }

        public void EnableInput()
        {
            //Debug.Log("Enabling input.");
            stateManager.Unfreeze();
        }

        public void DisableInput()
        {
            //Debug.Log("Disabling input.");
            OnUpdate = null;
            stateManager.Freeze();
            stateManager.StateMachine.ForceCurrentState(stateManager.StateGraph.inactive);
        }
    }
}