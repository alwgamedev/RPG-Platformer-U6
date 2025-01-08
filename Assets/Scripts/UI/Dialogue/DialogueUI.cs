using RPGPlatformer.Dialogue;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class DialogueUI : HideableUI
    {
        [SerializeField] DialogueWindow dialogueWindowPrefab;

        DialogueSO activeDialogue;
        DialogueNode currentNode;
        DialogueWindow activeWindow;
        string conversantName;
        string playerName = "Player";

        protected override void Awake()
        {
            base.Awake();

            DialogueTrigger.DialogueTriggered += StartDialogue;
            DialogueTrigger.DialogueCancelled += EndDialogue;
        }

        public void StartDialogue(DialogueSO dialogue, string conversantName, string playerName)
        {
            activeDialogue = dialogue;
            currentNode = dialogue.RootNode();
            this.conversantName = conversantName;
            this.playerName = playerName;
            DisplayDialogueNode(currentNode);
            Show();
        }

        public void EndDialogue()
        {
            CloseActiveWindow();
            activeDialogue = null;
            currentNode = null;
            conversantName = null;
            playerName = null;
            Hide();
        }

        private void DisplayDialogueNode(DialogueNode dialogueNode)
        {
            CloseActiveWindow();

            currentNode = dialogueNode;

            activeWindow = Instantiate(dialogueWindowPrefab, transform);
            activeWindow.SetUpWindow(dialogueNode, conversantName, playerName);

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
