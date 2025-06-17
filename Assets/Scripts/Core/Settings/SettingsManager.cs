using RPGPlatformer.Combat;
using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [RequireComponent(typeof(InputActionsManager))]
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; } = null;

        public bool IAMIsConfigured { get; private set; }
        public InputActionsManager IAM { get; private set; }
        public InputBindingData InputSettings { get; private set; }
        public SerializableCharacterAbilityBarData PlayerAbilityBars { get; private set; }
        //NOTE: it needs to be the serializable version so that we can save it (if it's an actual ability bar
        //then we would have to serialize attack abilities and way too much information; this way we basically
        //just store the combat style and ability name)

        public static event Action IAMConfigured;//also gets called when new InputSettings are set
        public static event Action<SerializableCharacterAbilityBarData> NewAbilityBarSettings;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;

                InputSettings = InputBindingData.DefaultBindings;

                IAM = GetComponent<InputActionsManager>();
                IAM.Configured += BroadcastIAMConfigure;

                PlayerAbilityBars = SerializableCharacterAbilityBarData.DefaultAbilityBarData(); 
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

        //INPUT SETTINGS

        public InputBindingValidationResult TrySetInputBindings(InputBindingData data)
        {
            var result = data.Validate();
            if(result == InputBindingValidationResult.Valid)
            {
                InputSettings = data;
                IAM.Configure();
            }
            return result;
        }

        private void BroadcastIAMConfigure()
        {
            IAMIsConfigured = true;
            IAMConfigured?.Invoke();
        }


        //ABILITY BAR SETTINGS

        public void SetPlayerAbilityBars(SerializableCharacterAbilityBarData newData)
        {
            PlayerAbilityBars = newData;
            NewAbilityBarSettings?.Invoke(PlayerAbilityBars);
        }

        private void OnDestroy()
        {
            if(Instance == this)
            {
                IAM.Configured -= BroadcastIAMConfigure;
                IAMConfigured = null;
                Instance = null;
            }
        }
    }
}