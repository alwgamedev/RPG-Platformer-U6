using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Core
{
    using static InputTools;

    public enum InputBindingValidationResult
    {
        Valid, MoveLeftMissing, MoveRightMissing, BindingsNonDistinct
    }

    public class InputBindingData
    {
        public string moveLeftBindingPath;
        public string moveRightBindingPath;

        public Dictionary<int, string> abilityBarBindingPaths = new();

        public static InputBindingData DefaultBindings = new()
        {
            moveLeftBindingPath = "<Keyboard>/a",
            moveRightBindingPath = "<Keyboard>/d",

            abilityBarBindingPaths = new()
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
                [9] = "<Keyboard>/v",
                [10] = "<Keyboard>/1",
                [11] = "<Keyboard>/2",
                [12] = "<Keyboard>/3"
            }
        };

        public List<string> GetAllBindings()
        {
            var bindings = new List<string>(abilityBarBindingPaths.Values)
            {
                moveLeftBindingPath,
                moveRightBindingPath
            };
            return bindings;
        }

        public void LogBindings()//useful for catching bugs.
        {
            Debug.Log($"move left: {moveLeftBindingPath}");
            Debug.Log($"move right: {moveRightBindingPath}");

            foreach(var entry in abilityBarBindingPaths)
            {
                Debug.Log($"ability bar {entry.Key}: {entry.Value}");
            }
        }

        public bool ValidBinding(string binding)
        {
            return binding != null 
                && binding.StartsWith(keyboardBindingPrefix) 
                && binding.Length > keyboardBindingPrefix.Length;
        }

        public InputBindingValidationResult Validate()
        {
            List<string> all = GetAllBindings();

            if(!ValidBinding(moveLeftBindingPath))
            {
                return InputBindingValidationResult.MoveLeftMissing;
            }
            if(!ValidBinding(moveRightBindingPath))
            {
                return InputBindingValidationResult.MoveRightMissing;
            }
            if(all.Distinct().Count() != all.Count())
            {
                return InputBindingValidationResult.BindingsNonDistinct;
            }
            return InputBindingValidationResult.Valid;
        }
    }
}