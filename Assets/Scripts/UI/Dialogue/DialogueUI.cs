using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatformer.Dialogue;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class DialogueUI : HidableUI
    {
        [SerializeField] DialogueWindow dialogueWindowPrefab;

        //DialogueTriggerData activeDialogue;
        DialogueSO activeDialogue;
        List<string> conversantNames = new();
        DialogueNode currentNode;
        DialogueWindow activeWindow;
        //string conversantName;
        //string playerName = "Player";

        protected override void Awake()
        {
            base.Awake();

            DialogueTrigger.DialogueTriggered += StartDialogue;
            DialogueTrigger.DialogueCancelled += EndDialogue;

            //if (GlobalGameTools.Instance != null)
            //{
            //    playerName = GlobalGameTools.PlayerName;
            //}
            //else
            //{
            //    GlobalGameTools.InstanceReady += InitializePlayerName;
            //}
        }

        //protected void InitializePlayerName()
        //{
        //    playerName = GlobalGameTools.PlayerName;
        //    GlobalGameTools.InstanceReady -= InitializePlayerName;
        //}

        public void StartDialogue(DialogueTriggerData data/*DialogueSO dialogue, string conversantName, string playerName*/)
        {
            if (!data.IsValid())
            {
                Debug.Log($"Unable to start dialogue, because {nameof(DialogueTriggerData)} is invalid.");
                return;
            }

            activeDialogue = data.DialogueSO;
            currentNode = activeDialogue.RootNode();
            conversantNames = data.Conversants.Select(x => x.ConversantName).ToList();
            DisplayDialogueNode(currentNode);
            Show();
        }

        public void EndDialogue()
        {
            CloseActiveWindow();
            activeDialogue = null;
            currentNode = null;
            conversantNames = null;
            //conversantName = null;
            //playerName = null;
            Hide();
        }

        private void DisplayDialogueNode(DialogueNode dialogueNode/*, string conversantName*/)
        {
            CloseActiveWindow();

            currentNode = dialogueNode;
            string conversantName = conversantNames[dialogueNode.ConversantNumber()];

            activeWindow = Instantiate(dialogueWindowPrefab, transform);
            activeWindow.SetUpWindow(dialogueNode, conversantName/*, playerName*/);

            if(!activeDialogue.HasContinuation(dialogueNode))
            {
                activeWindow.NextButtonContainer.SetActive(false);
            }

            activeWindow.CloseButton.onClick.AddListener(CloseActiveWindow);
            activeWindow.ResponseSelected += DisplayContinuation;
            //(^even for a choice node where the responses have no continuation (i.e. the choices are just 
            //closing remarks) we should subscribe DisplayContinuation so that the dialogue closes
            //when that last choice is selected)
        }

        private void DisplayContinuation(int responseIndex)
        {
            if(!activeDialogue.TryGetContinuation(currentNode, responseIndex, out var continuation))
            {
                EndDialogue();
                return;
            }
            DisplayDialogueNode(continuation);
        }

        private void CloseActiveWindow()
        {
            if(activeWindow)
            {
                Destroy(activeWindow.gameObject);
                activeWindow = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DialogueTrigger.DialogueTriggered -= StartDialogue;
            DialogueTrigger.DialogueCancelled -= EndDialogue;
        }
    }
}
