using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RPGPlatformer.Core
{
    public class InputActionsManager : MonoBehaviour
    {
        InputActionMap actionMap = new();

        public const string saveBindingPath = "<Keyboard>/n";
        public const string loadBindingPath = "<Keyboard>/m";
        public const string leftClickBindingPath = "<Mouse>/leftButton";
        public const string rightClickBindingPath = "<Mouse>/rightButton";
        public const string jumpBindingPath = "<Keyboard>/space";
        public const string climbBindingPath = "<Keyboard>/shift";
        public const string toggleRunBindingPath = "<Keyboard>/leftCtrl";
        public const string escBindingPath = "<Keyboard>/escape";
        public const string backspaceBindingPath = "<Keyboard>/backspace";

        public const string saveActionName = "Save Button";
        public const string loadActionName = "Load Button";
        public const string leftClickActionName = "Left Click";
        public const string rightClickActionName = "Right Click";
        public const string moveLeftActionName = "Left Arrow";
        public const string moveRightActionName = "Right Arrow";
        public const string climbActionName = "Climb";//this will be like climb rope or ladder (not in use yet)
        public const string toggleRunActionName = "ToggleRun";
        public const string jumpActionName = "Spacebar";
        public const string escActionName = "Esc";
        public const string backspaceActionName = "Backspace";

        //INPUT ACTIONS

        //public InputAction SaveAction;
        //public InputAction LoadAction;
        //public InputAction LeftClickAction;
        //public InputAction RightClickAction;
        //public InputAction MoveLeftAction;
        //public InputAction MoveRightAction;
        //public InputAction ClimbAction;
        //public InputAction ToggleRunAction;
        //public InputAction SpacebarAction;
        //public InputAction EscAction;
        //public InputAction BackspaceAction;
        public Dictionary<int, InputAction> AbilityBarActions = new();

        //public bool IsInputEnabled => actionMap.enabled;

        //BOOLS FOR KEY HELD DOWN
        //public bool LeftClickDown { get; private set; }
        //public bool RightClickDown { get; private set; }
        //public bool MoveLeftHeldDown { get; private set; }
        //public bool MoveRightHeldDown { get; private set; }
        public bool MouseOverUI {  get; private set; }


        //ALERT CONTROLLER SYSTEMS WHEN CONFIGURED

        public event Action Configured;

        private void Update()
        {
            MouseOverUI = EventSystem.current.IsPointerOverGameObject();
            //because it doesn't like when you check IsPointerOverGameObject() within event callbacks
        }

        public InputAction InputAction(string name)
        {
            return actionMap[name];
        }

        //public InputAction AbilityBarInputAction(int i)
        //{
        //    return actionMap[$"Ability Bar {i}"];
        //}

        public bool HeldDown(string actionName)
        {
            return actionMap[actionName].inProgress;
            //^we'll see if this is the right thing
        }

        public void Configure()
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

            actionMap.AddAction(name: saveActionName, type: InputActionType.Value, 
                binding: saveBindingPath);
            actionMap.AddAction(name: loadActionName, type: InputActionType.Value,
                binding: loadBindingPath);
            actionMap.AddAction(name: leftClickActionName, type: InputActionType.Value, 
                binding: leftClickBindingPath);
            actionMap.AddAction(name: rightClickActionName, type: InputActionType.Value, 
                binding: rightClickBindingPath);
            actionMap.AddAction(name: moveLeftActionName, type: InputActionType.Value, 
                binding: currentBindings.moveLeftBindingPath);
            actionMap.AddAction(name: moveRightActionName, type: InputActionType.Value, 
                binding: currentBindings.moveRightBindingPath);
            actionMap.AddAction(name: climbActionName, type: InputActionType.Value, binding: climbBindingPath);
            actionMap.AddAction(name: toggleRunActionName, type: InputActionType.Value, 
                binding: toggleRunBindingPath);
            actionMap.AddAction(name: jumpActionName, type: InputActionType.Value, 
                binding: jumpBindingPath);
            actionMap.AddAction(name: escActionName, type: InputActionType.Value, binding: escBindingPath);
            actionMap.AddAction(name: backspaceActionName, type: InputActionType.Value, 
                binding: backspaceBindingPath);

            AbilityBarActions.Clear();
            foreach (var entry in currentBindings.abilityBarBindingPaths)
            {
                AbilityBarActions[entry.Key] = actionMap.AddAction(name: $"Ability Bar {entry.Key}", 
                    type: InputActionType.Value, binding: entry.Value);
            }

            actionMap.Enable();
            Configured?.Invoke();
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
            Configured = null;

            actionMap.Dispose();
        }
    }
}