using UnityEngine;
using RPGPlatformer.Movement;
using RPGPlatformer.Core;
using System;
using System.Collections.Generic;

namespace RPGPlatformer.AIControl
{
    using T0 = HybridFlyerController;
    using T00 = HybridFlyer;
    using T01 = HybridFlyerStateGraph;
    using T02 = HybridFlyerStateMachine;
    using T03 = HybridFlyerStateManager;
    using T1 = ShuttleHawk;
    using T2 = ShuttleHawkStateGraph;
    using T3 = ShuttleHawkStateMachine;

    public class ShuttleHawkController : GenericAIPatrollerController<T0, T00, T01, T02, T03, T1, T2, T3,
        ShuttleHawkStateManager>
    {
        [SerializeField] PatrolPath shuttlePath;
        [SerializeField] PatrolPath returnPath;
        [SerializeField] Transform departurePoint;

        IInteractableNPC npc;

        Dictionary<string, Action> PatrolDestinationReachedHandler = new();

        public bool AwaitingDeparture 
            => stateManager.StateMachine.CurrentState == stateManager.StateGraph.awaitingDeparture;
        public bool Shuttling
            => stateManager.StateMachine.CurrentState == stateManager.StateGraph.shuttling;
        public bool ReturningToNest
            => stateManager.StateMachine.CurrentState == stateManager.StateGraph.returningToNest;

        public event Action<bool> DialogueEnabled;

        protected override void Awake()
        {
            base.Awake();

            npc = GetComponent<IInteractableNPC>();

            BuildPatrolDestinationReachedHandlerDict();
        }

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.patrol.OnExit += OnPatrolExit;
            stateManager.StateGraph.awaitingDeparture.OnEntry += OnAwaitingDepartureEntry;
            stateManager.StateGraph.shuttling.OnEntry += OnShuttlingEntry;
            stateManager.StateGraph.returningToNest.OnEntry += OnReturningToNestEntry;

            StateBehavior[stateManager.StateGraph.awaitingDeparture] = patroller.AwaitingDepartureBehavior;
            StateBehavior[stateManager.StateGraph.shuttling] = patroller.PatrolBehavior;
            StateBehavior[stateManager.StateGraph.returningToNest] = patroller.PatrolBehavior;
        }

        private void BuildPatrolDestinationReachedHandlerDict()
        {
            PatrolDestinationReachedHandler = new();

            PatrolDestinationReachedHandler[typeof(Patrol).Name] = base.OnDestinationReached;
            PatrolDestinationReachedHandler[typeof(AwaitingDeparture).Name] = AwaitingDepartureDestinationReached;
            PatrolDestinationReachedHandler[typeof(Shuttling).Name] = ShuttlingDestinationReached;
            PatrolDestinationReachedHandler[typeof(ReturningToNest).Name] = ReturningToNestDestinationReached; 
        }

        protected override void OnPatrolEntry()
        {
            base.OnPatrolEntry();

            DialogueEnabled?.Invoke(true);
        }

        private void OnPatrolExit()
        {
            DialogueEnabled?.Invoke(false);
        }

        private void OnAwaitingDepartureEntry()
        {
            patroller.HeadToDeparturePoint(departurePoint);
        }


        private void OnShuttlingEntry()
        {
            patroller.BeginFlightPath(shuttlePath);
        }

        private void OnReturningToNestEntry()
        {
            patroller.BeginFlightPath(returnPath);
        }

        protected override void OnDestinationReached()
        {
            if (PatrolDestinationReachedHandler.TryGetValue(stateManager.StateMachine.CurrentState.name, 
                out var action))
            {
                action?.Invoke();
            }
        }

        private void AwaitingDepartureDestinationReached()
        {
            //the hawk has been waiting at the departure point and the timer has run out
            if (patroller.PatrolNavigator.CurrentMode == NavigationMode.timedRest)
            {
                patroller.DepartureTimeOut();
            }
            else//when the hawk has first reached the departure point
            {
                patroller.ReadyForDeparture();
            }
        }

        private void ShuttlingDestinationReached()
        {
            if (patroller.PatrolNavigator.CurrentMode == NavigationMode.timedRest)
            {
                patroller.ReadyToReturnToNest();
            }
            else if (!patroller.PatrolNavigator.GetNextDestination())
            {
                patroller.ShuttleDestionationReached();
            }
        }

        private void ReturningToNestDestinationReached()
        {
            if (!patroller.PatrolNavigator.GetNextDestination())
            {
                patroller.OnReturnedToNest();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DialogueEnabled = null;
        }
    }
}
