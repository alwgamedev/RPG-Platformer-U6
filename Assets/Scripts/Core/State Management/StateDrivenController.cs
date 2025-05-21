using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class StateDrivenController<T0, T1, T2, T3> : MonoBehaviour
        where T0 : StateManager<T1, T2, T3>
        where T1 : StateGraph
        where T2 : StateMachine<T1>
        where T3 : StateDriver
    {
        protected T0 stateManager;
        protected T3 stateDriver;

        protected virtual void Awake()
        {
            InitializeDriver();
            //InitializeStateManager();
        }

        private void OnEnable()
        {
            if (stateManager == null)
            {
                InitializeStateManager();
            }
            //we only do this once, but I'm just making sure it gets called after anything else in Awake
        }

        protected virtual void Start()
        {
            ConfigureStateManager();
        }

        protected virtual void InitializeDriver()
        {
            stateDriver = GetComponent<T3>();
        }

        protected virtual void InitializeStateManager()
        {
            stateManager = (T0)Activator.CreateInstance(typeof(T0), null, stateDriver);
        }

        protected virtual void ConfigureStateManager()
        {
            stateManager.Configure();

            //+subscribe state entry/exit functions here
        }
    }
}