using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [RequireComponent(typeof(InputActionsManager))]
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; } = null;

        public InputActionsManager IAM { get; private set; }
        public InputBindingData CurrentBindings { get; private set; }

        public static event Action OnIAMConfigure;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;

                CurrentBindings = InputBindingData.DefaultBindings;

                IAM = GetComponent<InputActionsManager>();
                IAM.OnConfigure += IAMConfigureHandler;
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
                IAM.Configure();
            }
        }

        public InputBindingValidationResult TrySetInputBindings(InputBindingData data)
        {
            var result = data.Validate();
            if(result == InputBindingValidationResult.Valid)
            {
                CurrentBindings = data;
                IAM.Configure();
            }
            return result;
        }

        private void IAMConfigureHandler()
        {
            OnIAMConfigure?.Invoke();
            //IAM.OnConfigure -= IAMConfigureHandler;
        }

        private void OnDestroy()
        {
            if(Instance == this)
            {
                IAM.OnConfigure -= IAMConfigureHandler;
                OnIAMConfigure = null;
                Instance = null;
            }
        }
    }
}