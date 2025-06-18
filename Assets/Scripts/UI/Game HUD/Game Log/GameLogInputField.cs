using System;
using UnityEngine;
using TMPro;

namespace RPGPlatformer.UI
{
    public class GameLogInputField : MonoBehaviour
    {
        TMP_InputField inputField;

        public TMP_InputField InputField => inputField;

        public event Action OnReset;
        public event Action<object> InputSubmitted;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
        }

        public void ListenForNumericalInput()
        {
            SetContentType(TMP_InputField.ContentType.IntegerNumber);
            inputField.onSubmit.AddListener(SubmitNumericalInput);
        }

        public void ResetInputField()
        {
            inputField.onSubmit.RemoveAllListeners();
            inputField.contentType = TMP_InputField.ContentType.Standard;
            inputField.text = "";
            OnReset?.Invoke();
        }

        private void SetContentType(TMP_InputField.ContentType contentType)
        {
            inputField.contentType = contentType;
        }

        private int? ParseNumericalInput(string text)
        {
            if (Int32.TryParse(text, out int value))
            {
                return value;
            }
            return null;
        }

        private void SubmitNumericalInput(string text)
        {
            SubmitInput(ParseNumericalInput(text));
        }

        private void SubmitInput(object input)
        //this will also trigger when IF is deselected
        //(and in that case no input will be submitted)
        //(so you can cancel input attempt by clicking outside the input field, basically)
        {
            inputField.onSubmit.RemoveListener(SubmitNumericalInput);

            if (Input.GetKeyDown(KeyCode.Return))
            {
                InputSubmitted?.Invoke(input);
            }
            else
            {
                InputSubmitted?.Invoke(null);
            }
        }

        private void OnDestroy()
        {
            OnReset = null;
            InputSubmitted = null;
            inputField.onSubmit.RemoveAllListeners();
        }
    }
}