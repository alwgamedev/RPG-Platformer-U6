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

        public AINavigator PatrolNavigator { get; protected set; }
        public T MovementController { get; protected set; }

        protected virtual void Awake()
        {
            MovementController = GetComponent<T>();
            PatrolNavigator = GetComponent<AINavigator>();

            PatrolNavigator.BeginHangTime += MovementController.SoftStop;
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
            PatrolNavigator.BeginPatrol(mode, p, MovementController);
        }

        public virtual void PatrolBehavior()
        {
            PatrolNavigator.PatrolBehavior(MovementController);
        }

        //this gets called e.g. when you reach the end of a patrol path
        //public virtual void OnPatrolDestinationReached()
        //{
        //    if (!PatrolNavigator.GetNextDestionation())
        //    {
        //        BeginPatrol(PatrolMode.rest, null);
        //    }
        //}

        //protected virtual void TriggerPatrol()
        //{
        //    Trigger(typeof(Patrol).Name);
        //}

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnUpdate = null;
        }
    }
}