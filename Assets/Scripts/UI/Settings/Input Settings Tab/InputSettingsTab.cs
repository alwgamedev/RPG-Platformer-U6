using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using Unity.VisualScripting;
using System.Linq;

namespace RPGPlatformer.UI
{
    using static InputTools;

    public class InputSettingsTab : SettingsTab
    {
        [SerializeField] VerticalLayoutGroup movementGroup;
        [SerializeField] VerticalLayoutGroup abilityBarGroup;
        [SerializeField] GameObject settingFieldPrefab;

        TMP_InputField moveLeftInputField;
        TMP_InputField moveRightInputField;
        List<TMP_InputField> abilityBarInputFields = new();
        List<TMP_InputField> allInputFields = new();

        public override void Redraw()
        {
            Clear();

            moveLeftInputField = AddField("Move Left:", movementGroup);
            moveRightInputField = AddField("Move Right:", movementGroup);

            for(int i = 1; i <= AbilityBar.playerAbilityBarLength; i++)
            {
                abilityBarInputFields.Add(AddField($"Ability Bar {i}:", abilityBarGroup));
            }

            PopulateFields(SettingsManager.Instance.InputSettings);

            HasUnsavedChanges = false;

        }

        public override void LoadDefaultSettings()
        {
            PopulateFields(InputBindingData.DefaultBindings);
        }

        public override bool TrySaveTab(out string resultMessage)
        {
            resultMessage = null;
            if(SettingsManager.Instance == null)
            {
                return false;
            }

            var result = SettingsManager.Instance.TrySetInputBindings(GetBindings());

            if (result == InputBindingValidationResult.Valid)
            {
                HasUnsavedChanges = false;
                return true;
            }
            else if (result == InputBindingValidationResult.MoveLeftMissing)
            {
                resultMessage = "Move Left binding cannot be empty.";
                return false;
            }
            else if (result == InputBindingValidationResult.MoveRightMissing)
            {
                resultMessage = "Move Right binding cannot be empty.";
                return false;
            }
            else if (result == InputBindingValidationResult.BindingsNonDistinct)
            {
                resultMessage = "You cannot use the same keybinding for more than one action.";
                return false;
            }
            else
            {
                resultMessage = "Input bindings are invalid.";
                return false;
            }
        }

        public void Clear()
        {
            moveLeftInputField = null;
            moveRightInputField = null;
            abilityBarInputFields.Clear();
            allInputFields.Clear();

            //Debug.Log($"movement group non-null? movementGroup != null)

            foreach (var child in movementGroup.GetComponentsInChildren<TMP_InputField>())
            {
                Destroy(child.transform.parent.gameObject);
            }

            foreach (var child in abilityBarGroup.GetComponentsInChildren<TMP_InputField>())
            {
                Destroy(child.transform.parent.gameObject);
            }
        }

        public void PopulateFields(InputBindingData data)
        {
            moveLeftInputField.text = KeyName(data.moveLeftBindingPath);
            moveRightInputField.text = KeyName(data.moveRightBindingPath);

            for (int i = 0; i < AbilityBar.playerAbilityBarLength; i++)
            {
                if (data.abilityBarBindingPaths.ContainsKey(i))
                {
                    abilityBarInputFields[i].text = KeyName(data.abilityBarBindingPaths[i]);
                }
                else
                {
                    abilityBarInputFields[i].text = "";
                }
            }
        }

        public InputBindingData GetBindings()
        {
            Dictionary<int, string> abilityBarBindings = GetAbilityBarBindings();

            var result = new InputBindingData()
            {
                moveLeftBindingPath = ToBindingPath(moveLeftInputField.text.Trim().ToLower()),
                moveRightBindingPath = ToBindingPath(moveRightInputField.text.Trim().ToLower()),
                abilityBarBindingPaths = abilityBarBindings
            };

            return result;
        }

        public Dictionary<int, string> GetAbilityBarBindings()
        {
            Dictionary<int, string> abilityBarBindings = new();
            for (int i = 0; i < abilityBarInputFields.Count; i++)
            {
                if (!string.IsNullOrEmpty(abilityBarInputFields[i].text.Trim()))
                {
                    abilityBarBindings[i] = ToBindingPath(abilityBarInputFields[i].text.Trim().ToLower());
                }
            }

            return abilityBarBindings;
        }

        private TMP_InputField AddField(string name, VerticalLayoutGroup parent)
        {
            var instance = Instantiate(settingFieldPrefab, parent.transform);
            var label = instance.GetComponentInChildren<TextMeshProUGUI>();
            label.text = name;
            var inputField = instance.GetComponentInChildren<TMP_InputField>();
            inputField.onValueChanged.AddListener((text) => RemoveDuplicateBindings(inputField));
            inputField.onValueChanged.AddListener((text) => HasUnsavedChanges = true);
            allInputFields.Add(inputField);
            return inputField;
        }

        private void RemoveDuplicateBindings(TMP_InputField inputField)
        {
            if (string.IsNullOrEmpty(inputField.text)) return;
            foreach(var field in allInputFields)
            {
                if (field == inputField) continue;
                if(field.text.ToLower() == inputField.text.ToLower())
                {
                    field.text = "";
                }
            }
        }
    }
}