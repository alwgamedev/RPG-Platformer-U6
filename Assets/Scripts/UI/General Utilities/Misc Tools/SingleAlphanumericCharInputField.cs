using UnityEngine;
using TMPro;
using RPGPlatformer.Core;
using UnityEngine.InputSystem;

namespace RPGPlatformer.UI
{
    public class SingleAlphanumericCharInputField : MonoBehaviour
    {
        TMP_InputField inputField;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();

            Configure();

            //SettingsManager.OnIAMConfigure += OnIAMConfigure;
        }

        private void Update()
        {
            if (gameObject.activeSelf && inputField.isFocused)
            {
                inputField.DeactivateInputField();
                inputField.ActivateInputField();
                //(silly but makes sure all text is selected)
            }
        }

        private void Configure()
        {
            inputField.characterLimit = 1;
            inputField.contentType = TMP_InputField.ContentType.Alphanumeric;
            inputField.onFocusSelectAll = true;
            inputField.onValidateInput = ValidateInput;
        }

        private char ValidateInput(string input, int index, char c)
        {
            inputField.text = "";
            if (c == '-' || c == ' ')
            {
                return '\0';
            }
            return c;
        }

        //private void OnDestroy()
        //{
        //    SettingsManager.OnIAMConfigure -= OnIAMConfigure;
        //    if(SettingsManager.Instance != null & SettingsManager.Instance.IAM != null
        //        && SettingsManager.Instance.IAM.BackspaceAction != null)
        //    {
        //        SettingsManager.Instance.IAM.BackspaceAction.started -= ClearField;
        //    }
        //}
    }
}