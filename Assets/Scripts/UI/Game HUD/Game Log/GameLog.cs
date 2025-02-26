using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class GameLog : MonoBehaviour
    {
        [SerializeField] int maxMessages = 100;
        [SerializeField] TextMeshProUGUI content;

        GameLogInputField inputField;

        List<int> messageCharacterMarkers = new();//[i] -> index in content.text of the last character of the i-th message

        public static GameLog Instance { get; private set; }
        public static GameLogInputField InputField => Instance.inputField;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                messageCharacterMarkers = new();
                content.text = "";
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if(Instance == this)
            {
                Configure();
            }
        }

        public void Configure()
        {
            inputField = GetComponentInChildren<GameLogInputField>();
            inputField.InputSubmitted += (input) => EnableInputField(false);
            EnableInputField(false);

            if (SettingsManager.Instance && SettingsManager.Instance.IAM.actionMap != null)
            {
                OnIAMConfigure();
            }
            SettingsManager.OnIAMConfigure += OnIAMConfigure;
            //still subscribe in case action map gets rebuilt due to input bindings change or something
        }

        private void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;
            iam.LeftClickAction.started += DisableInputFieldOnMouseDown;
            iam.RightClickAction.started += DisableInputFieldOnMouseDown;
        }

        public static void Log(string text)
        {
            if (Instance)
            {
                Instance.PrivateLog(text);
            }
        }

        public static void ListenForNumericalInput()
        {
            Instance.EnableInputField(true);
            Instance.inputField.ListenForNumericalInput();
            Log("Enter the desired quantity:");
        }

        private void PrivateLog(string text)
        {
            if (text.Length == 0) return;

            string message = content.text == "" ? text : "\n" + text;
            content.text += message;
            UpdateMessageCounterAndTrim(message);
        }

        private void EnableInputField(bool val)
        {
            inputField.ResetInputField();
            inputField.gameObject.SetActive(val);
            if (val)
            {
                inputField.InputField.Select();
            }
        }

        private void UpdateMessageCounterAndTrim(string message)
        {
            messageCharacterMarkers.Add(content.text.Length);
            if(messageCharacterMarkers.Count > maxMessages)
            {
                int lengthToDelete = messageCharacterMarkers[0];
                content.text = content.text.Remove(0, lengthToDelete);
                if (content.text[0] ==  '\n')
                {
                    content.text = content.text.Remove(0, 1);
                    lengthToDelete += 1;
                }
                messageCharacterMarkers.RemoveAt(0);
                for(int i = 0; i < messageCharacterMarkers.Count; i++)
                {
                    messageCharacterMarkers[i] = messageCharacterMarkers[i] - lengthToDelete;
                }

                Canvas.ForceUpdateCanvases();
            }
        }

        private void DisableInputFieldOnMouseDown(InputAction.CallbackContext ctx)
        {
            if (inputField.gameObject.activeSelf)
            {
                EnableInputField(false);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
