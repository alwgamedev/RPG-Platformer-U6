using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGPlatformer.Core
{
    public class InputBindingManager : MonoBehaviour, IInputSource
    {
        public InputActionMap actionMap = new();

        //BINDING DATA

        public Dictionary<int, string> abilityBarBindingPaths = new()//keys will correspond to indices in the player's special ability book
                                                                     //(if an ability is not bound, or if there is no ability at that index, it won't cause errors)
        {
            [0] = "<Keyboard>/q",
            [1] = "<Keyboard>/w",
            [2] = "<Keyboard>/e",
            [3] = "<Keyboard>/s",
            [4] = "<Keyboard>/f",
            [5] = "<Keyboard>/g",
            [6] = "<Keyboard>/z",
            [7] = "<Keyboard>/x",
            [8] = "<Keyboard>/c",
        };

        public string saveBindingPath = "<Keyboard>/n";
        public string loadBindingPath = "<Keyboard>/m";

        public string moveLeftBindingPath = "<Keyboard>/a";
        public string moveRightBindingPath = "<Keyboard>/d";

        public string toggleRunBindingPath = "<Keyboard>/leftCtrl";


        //INPUT ACTIONS

        public InputAction SaveAction;
        public InputAction LoadAction;
        public InputAction LeftClickAction;
        public InputAction RightClickAction;
        public InputAction MoveLeftAction;
        public InputAction MoveRightAction;
        public InputAction ToggleRunAction;
        public InputAction SpacebarAction;
        public InputAction EscAction;
        public Dictionary<int, InputAction> AbilityBarActions = new();


        ////BOOLS FOR KEY HELD DOWN
        //public bool leftClickHeldDown;
        public bool moveLeftHeldDown;
        public bool moveRightHeldDown;


        //ALERT CONTROLLER SYSTEMS WHEN CONFIGURED

        public event Action OnConfigure;

        public void ClearAbilityBarBindings()
        {
            abilityBarBindingPaths.Clear();
            AbilityBarActions.Clear();
        }

        public virtual void Configure()
        {
            actionMap = new();

            SaveAction = actionMap.AddAction(name: "Save Button", type: InputActionType.Value, binding: saveBindingPath);
            LoadAction = actionMap.AddAction(name: "Load Button", type: InputActionType.Value, binding: loadBindingPath);

            LeftClickAction = actionMap.AddAction(name: "Left Click", type: InputActionType.Value, binding: "<Mouse>/leftButton");
            RightClickAction = actionMap.AddAction(name: "Right Click", type: InputActionType.Value, binding: "<Mouse>/rightButton");

            MoveLeftAction = actionMap.AddAction(name: "Move Left", type: InputActionType.Value, binding: moveLeftBindingPath);
            MoveLeftAction.started += (context) => { moveLeftHeldDown = true; };
            MoveLeftAction.canceled += (context) => { moveLeftHeldDown = false; };

            MoveRightAction = actionMap.AddAction(name: "Move Right", type: InputActionType.Value, binding: moveRightBindingPath);
            MoveRightAction.started += (context) => { moveRightHeldDown = true; };
            MoveRightAction.canceled += (context) => { moveRightHeldDown = false; };

            ToggleRunAction = actionMap.AddAction(name: "Toggle Run", type: InputActionType.Value, binding: toggleRunBindingPath);

            SpacebarAction = actionMap.AddAction(name: "Spacebar", type: InputActionType.Value, binding: "<Keyboard>/space");

            EscAction = actionMap.AddAction(name: "Esc", type: InputActionType.Value, binding: "<Keyboard>/escape");

            foreach (var entry in abilityBarBindingPaths)
            {
                try
                {
                    AbilityBarActions[entry.Key] = actionMap.AddAction(name: $"Ability bar {entry.Key}", type: InputActionType.Value, binding: entry.Value);
                }
                catch
                {
                    Debug.LogWarning($"Unable to bind {entry.Value} to action {entry.Key}");
                }
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

            actionMap.Disable();
        }
    }
}