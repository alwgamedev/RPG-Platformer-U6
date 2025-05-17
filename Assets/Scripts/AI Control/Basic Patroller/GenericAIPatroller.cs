using System;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AINavigator))]
    public class GenericAIPatroller<T> : StateDriver, IInputDependent, IAIPatroller
        where T : IAIMovementController
    {
        protected Action OnUpdate;

        public AINavigator PatrolNavigator { get; protected set; }
        public T MovementController { get; protected set; }
        public IAIMovementController AIMovementController => MovementController;
        public IInputSource InputSource { get; protected set; }

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
        public virtual void InitializeInputSource()
        {
            InputSource = GetComponent<IInputSource>();
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

        public virtual void OnInputEnabled() { }

        public virtual void OnInputDisabled() { }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnUpdate = null;
        }
    }
}