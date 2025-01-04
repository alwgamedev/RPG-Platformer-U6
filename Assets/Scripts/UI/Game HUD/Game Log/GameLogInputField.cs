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
            inputField.onSubmit.AddListener((text) => SubmitInput(ParseNumericalInput(text)));
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

        private void SubmitInput(object input)
        //hopefully this will also trigger when IF is deselected
        //(and in that case no input will be submitted)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Input submitted");
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