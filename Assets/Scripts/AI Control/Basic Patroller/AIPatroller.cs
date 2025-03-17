using System;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AIMovementController))]
    [RequireComponent(typeof(PatrolNavigator))]
    public class AIPatroller : StateDriver
    {
        protected Action OnUpdate;

        public PatrolNavigator PatrolNavigator { get; protected set; }
        public AIMovementController MovementController { get; protected set; }

        protected virtual void Awake()
        {
            MovementController = GetComponent<AIMovementController>();
            PatrolNavigator = GetComponent<PatrolNavigator>();

            PatrolNavigator.PatrolComplete += OnPatrolComplete;
            PatrolNavigator.BeginHangTime += MovementController.SoftStop;
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        public virtual void BeginPatrol(PatrolMode mode, PatrolParemeters p)
        {
            PatrolNavigator.BeginPatrol(mode, p, MovementController);
        }

        public virtual void PatrolBehavior()
        {
            PatrolNavigator.PatrolBehavior(MovementController);
        }

        //this gets called e.g. when you reach the end of a patrol path
        public virtual void OnPatrolComplete()
        {
            PatrolNavigator.BeginRest(MovementController);
        }

        public void TriggerPatrol()
        {
            Trigger(typeof(Patrol).Name);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnUpdate = null;
        }
    }
}
