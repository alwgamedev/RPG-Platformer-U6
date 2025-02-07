using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RPGPlatformer.Core
{
    public class InputActionsManager : MonoBehaviour, IInputSource
    {
        public InputActionMap actionMap = new();

        public static readonly string saveBindingPath = "<Keyboard>/n";
        public static readonly string loadBindingPath = "<Keyboard>/m";

        public static readonly string jumpBindingPath = "<Keyboard>/space";
        public static readonly string climbBindingPath = "<Keyboard>/shift";
        public static readonly string toggleRunBindingPath = "<Keyboard>/leftCtrl";


        //INPUT ACTIONS

        public InputAction SaveAction;
        public InputAction LoadAction;
        public InputAction LeftClickAction;
        public InputAction RightClickAction;
        public InputAction MoveLeftAction;
        public InputAction MoveRightAction;
        public InputAction ClimbAction;
        public InputAction ToggleRunAction;
        public InputAction SpacebarAction;
        public InputAction EscAction;
        public InputAction BackspaceAction;
        public Dictionary<int, InputAction> AbilityBarActions = new();


        ////BOOLS FOR KEY HELD DOWN
        public bool MoveLeftHeldDown { get; private set; }
        public bool MoveRightHeldDown { get; private set; }
        public bool MouseOverUI {  get; private set; }


        //ALERT CONTROLLER SYSTEMS WHEN CONFIGURED

        public event Action OnConfigure;

        private void Update()
        {
            MouseOverUI = EventSystem.current.IsPointerOverGameObject();
            //because it doesn't like when you check IsPointerOverGameObject() within event callbacks
        }

        public virtual void Configure()
        {
            if (SettingsManager.Instance == null)
            {
                Debug.LogError("IAM tried to configure, but SettingsManager does not have an Instance set yet");
                return;
            }

            var currentBindings = SettingsManager.Instance.InputSettings;
            if (currentBindings == null)
            {
                Debug.LogError("IAM tried to configure, but the SettingsManager's CurrentBindings are null");
            }

            actionMap?.Dispose();

            actionMap = new();

            SaveAction = actionMap.AddAction(name: "Save Button", type: InputActionType.Value, 
                binding: saveBindingPath);
            LoadAction = actionMap.AddAction(name: "Load Button", type: InputActionType.Value,
                binding: loadBindingPath);

            LeftClickAction = actionMap.AddAction(name: "Left Click", type: InputActionType.Value, 
                binding: "<Mouse>/leftButton");
            RightClickAction = actionMap.AddAction(name: "Right Click", type: InputActionType.Value, 
                binding: "<Mouse>/rightButton");

            MoveLeftAction = actionMap.AddAction(name: "Move Left", type: InputActionType.Value, 
                binding: currentBindings.moveLeftBindingPath);
            MoveLeftAction.started += (context) => { MoveLeftHeldDown = true; };
            MoveLeftAction.canceled += (context) => { MoveLeftHeldDown = false; };

            MoveRightAction = actionMap.AddAction(name: "Move Right", type: InputActionType.Value, 
                binding: currentBindings.moveRightBindingPath);
            MoveRightAction.started += (context) => { MoveRightHeldDown = true; };
            MoveRightAction.canceled += (context) => { MoveRightHeldDown = false; };

            ClimbAction = actionMap.AddAction(name: "Climb", type: InputActionType.Value, binding: climbBindingPath);

            ToggleRunAction = actionMap.AddAction(name: "Toggle Run", type: InputActionType.Value, 
                binding: toggleRunBindingPath);

            SpacebarAction = actionMap.AddAction(name: "Spacebar", type: InputActionType.Value, 
                binding: jumpBindingPath);

            EscAction = actionMap.AddAction(name: "Esc", type: InputActionType.Value, binding: "<Keyboard>/escape");

            BackspaceAction = actionMap.AddAction(name: "Backspace", type: InputActionType.Value, 
                binding: "<Keyboard>/backspace");

            foreach (var entry in currentBindings.abilityBarBindingPaths)
            {
                AbilityBarActions[entry.Key] = actionMap.AddAction(name: $"Ability bar {entry.Key}", 
                    type: InputActionType.Value, binding: entry.Value);
            }

            actionMap.Enable();

            OnConfigure?.Invoke();
        }

        public void EnableInput()
        {
            actionMap.Enable();
        }

        public void DisableInput()
        {
            actionMap.Disable();
        }

        private void OnDestroy()
        {
            OnConfigure = null;

            actionMap.Dispose();
        }
    }
}