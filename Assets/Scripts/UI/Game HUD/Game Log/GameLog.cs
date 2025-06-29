using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using RPGPlatformer.Core;
using System.Linq;

namespace RPGPlatformer.UI
{
    public class GameLog : MonoBehaviour
    {
        [SerializeField] int maxMessages = 100;
        [SerializeField] TextMeshProUGUI content;
        
        CollapsableUI collapsableUI;
        //Animation highlightAnim;
        GameLogInputField inputField;
        Queue<string> messages = new();

        public static GameLog Instance { get; private set; }
        public static GameLogInputField InputField => Instance.inputField;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                content.text = "";
                collapsableUI = GetComponent<CollapsableUI>();
                //highlightAnim = collapsableUI.CollapseButton.GetComponent<Animation>();
                if (SettingsManager.Instance && SettingsManager.Instance.IAMIsConfigured)
                {
                    OnIAMConfigure();
                }
                SettingsManager.IAMConfigured += OnIAMConfigure;//still subscribe to get settings updates
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
            inputField = GetComponentInChildren<GameLogInputField>(true);
            inputField.InputSubmitted += (input) => EnableInputField(false);
            EnableInputField(false);
        }

        private void OnIAMConfigure()
        {
            var iam = SettingsManager.Instance.IAM;
            iam.InputAction(InputActionsManager.leftClickActionName).started += DisableInputFieldOnMouseDown;
            iam.InputAction(InputActionsManager.rightClickActionName).started += DisableInputFieldOnMouseDown;
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
            //if (!collapsableUI.IsOpen)
            //{
            //    highlightAnim.Play();
            //    //or highlight flash here instead
            //}

            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.Log($"GameLog received an empty message. Was it null? {text == null}");
                return;
            }

            messages.Enqueue(text);

            if (messages.Count > maxMessages)
            {
                messages.Dequeue();
                content.text = string.Join('\n', messages.Where(x => !string.IsNullOrWhiteSpace(x)));
            }
            else if (messages.Count > 0 )
            {
                content.text += "\n" + text;
            }
            else
            {
                content.text = text;
            }
            //UpdateMessageCounterAndTrim(message);
        }

        private void EnableInputField(bool val)
        {
            if (collapsableUI && !collapsableUI.IsOpen)
            {
                collapsableUI.SetOpen(true);
            }
            inputField.ResetInputField();
            inputField.gameObject.SetActive(val);
            if (val)
            {
                inputField.InputField.Select();
            }
        }

        //private void UpdateMessageCounterAndTrim(string message)
        //{
        //    messageCharacterMarkers.Add(content.text.Length);
        //    if(messageCharacterMarkers.Count > maxMessages)
        //    {
        //        int lengthToDelete = messageCharacterMarkers[0];
        //        content.text = content.text.Remove(0, lengthToDelete);
        //        if (content.text[0] ==  '\n')
        //        {
        //            content.text = content.text.Remove(0, 1);
        //            lengthToDelete += 1;
        //        }
        //        messageCharacterMarkers.RemoveAt(0);
        //        for(int i = 0; i < messageCharacterMarkers.Count; i++)
        //        {
        //            messageCharacterMarkers[i] = messageCharacterMarkers[i] - lengthToDelete;
        //        }

        //        Canvas.ForceUpdateCanvases();//this is performance intensive too
        //    }
        //}

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

            SettingsManager.IAMConfigured -= OnIAMConfigure;
        }
    }
}
