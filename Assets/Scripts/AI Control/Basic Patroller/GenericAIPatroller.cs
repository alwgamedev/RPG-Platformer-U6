using System;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AINavigator))]
    public class GenericAIPatroller<T, T0, T1, T2, T3> : StateDriver
        where T : GenericAdvancedMovementController<T0, T1, T2, T3>
        where T0 : AdvancedMover
        where T1 : AdvancedMovementStateGraph
        where T2 : AdvancedMovementStateMachine<T1>
        where T3 : AdvancedMovementStateManager<T1, T2, T0>
    {
        protected Action OnUpdate;
        protected T movementController;

        public AINavigator PatrolNavigator { get; protected set; }
        public IMovementController MovementController => movementController;
        //public T MovementController { get; protected set; }

        protected virtual void Awake()
        {
            movementController = GetComponent<T>();
            PatrolNavigator = GetComponent<AINavigator>();

            PatrolNavigator.BeginHangTime += movementController.SoftStop;
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        public virtual void InitializeState()
        {
            Trigger(typeof(Patrol).Name);
        }

        public virtual void BeginPatrol(NavigationMode mode, NavigationParameters p)
        {
            BeginPatrol(mode, p?.Content);
        }

        public virtual void BeginPatrol(NavigationMode mode, object p)
        {
            PatrolNavigator.BeginPatrol(mode, p, movementController);
        }

        public virtual void PatrolBehavior()
        {
            PatrolNavigator.PatrolBehavior(movementController);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnUpdate = null;
        }
    }
}