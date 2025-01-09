using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [RequireComponent(typeof(InputBindingManager))]
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; } = null;
        public InputBindingManager IBM { get; private set; }

        public static event Action OnIBMConfigure;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                IBM = GetComponent<InputBindingManager>();
                IBM.OnConfigure += IBMConfigureHandler;
            }
            else
            {
                Debug.Log("Settings manager already has an Instance set.");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (Instance == this)
            {
                IBM.Configure();
            }
        }

        private void IBMConfigureHandler()
        {
            OnIBMConfigure?.Invoke();
            IBM.OnConfigure -= IBMConfigureHandler;
        }

        private void OnDestroy()
        {
            if(Instance == this)
            {
                OnIBMConfigure = null;
                Instance = null;
            }
        }
    }
}