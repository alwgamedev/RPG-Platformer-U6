using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public abstract class StateDriver : MonoBehaviour
    {
        public event Action<string> TriggerEvent;

        protected void Trigger(string stateName)
        {
            TriggerEvent?.Invoke(stateName);
        }

        protected virtual void OnDestroy()
        {
            TriggerEvent = null;
        }
    }
}