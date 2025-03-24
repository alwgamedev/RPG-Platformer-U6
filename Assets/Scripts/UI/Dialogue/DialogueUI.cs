using UnityEngine;
using RPGPlatformer.Dialogue;
using System;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class DialogueUI : HidableUI
    {
        [SerializeField] DialogueWindow dialogueWindowPrefab;

        DialogueTriggerData activeDialogue;
        DialogueNode currentNode;
        DialogueWindow activeWindow;

        public static event Action<bool> DialogueBeganSuccessfully;//bool is whether it began successfully
        public static event Action DialogueEnded;

        protected override void Awake()
        {
            base.Awake();

            DialogueTrigger.DialogueTriggered += HandleDialogueTrigger;
            //DialogueTrigger.DialogueCancelled += EndDialogue;
        }

        public void StartDialogue(DialogueTriggerData data)
        {
            activeDialogue = data;
            currentNode = activeDialogue.DialogueSO.RootNode();
            foreach (var actor in data.Actors())
            {
                if (actor)
                {
                    actor.OnBeginDialogue();
                }
            }
            DisplayDialogueNode(currentNode);
            Show();

            DialogueBeganSuccessfully?.Invoke(true);
        }

        public void EndDialogue()
        {
            CloseActiveWindow();
            Hide();

            if (activeDialogue != null)
            {
                foreach (var actor in activeDialogue.Actors())
                {
                    if (actor)
                    {
                        actor.OnEndDialogue();
                    }
                }
            }

            DialogueEnded?.Invoke();

            activeDialogue = null;
            currentNode = null;
        }

        private void HandleDialogueTrigger(DialogueTrigger trigger, DialogueTriggerData data)
        {
            if (activeDialogue != null)
            {
                GameLog.Log("Exit the current dialogue before starting another.");
                DialogueBeganSuccessfully?.Invoke(false);
                return;
            }

            if (!data.AllowPlayerToEnterCombatDuringDialogue)
            {
                var player = GlobalGameTools.Player;

                if (player != null)
                {
                    if (player.IsInCombat)
                    {
                        GameLog.Log("You cannot participate in this dialogue while in combat.");
                        DialogueBeganSuccessfully?.Invoke(false);
                        return;
                    }

                    player.CombatEntered += CancelOnCombatEntry;
                    DialogueEnded += DialogueEndCombatantHandler;

                    void CancelOnCombatEntry()
                    {
                        GameLog.Log("You cannot participate in this dialogue while in combat.");
                        EndDialogue();
                    }

                    void DialogueEndCombatantHandler()
                    {
                        if (player != null)
                        {
                            player.CombatEntered -= CancelOnCombatEntry;
                        }
                        DialogueEnded -= DialogueEndCombatantHandler;
                    }
                }

                trigger.DialogueCancelRequested += EndDialogue;
                DialogueEnded += DialogueEndTriggerHandler;

                void DialogueEndTriggerHandler()
                {
                    if (trigger != null)
                    {
                        trigger.DialogueCancelRequested -= EndDialogue;
                    }

                    DialogueEnded -= DialogueEndTriggerHandler;
                }

                StartDialogue(data);
            }
        }

        private void DisplayDialogueNode(DialogueNode dialogueNode)
        {
            CloseActiveWindow();

            currentNode = dialogueNode;

            //allows you to use decision nodes with no text to navigate the dialogue tree
            if (currentNode is DecisionDialogueNode decisionNode && decisionNode.TextSegments().Count == 0)
            {
                activeDialogue.ExecuteEntryActions(currentNode);
                DisplayContinuation(0);
                //display continuation will execute the decision function and pick the appropriate continuation
                return;
            }

            var conversantName = activeDialogue.SpeakerName(dialogueNode);
            activeWindow = Instantiate(dialogueWindowPrefab, transform);
            activeWindow.SetUpWindow(dialogueNode, conversantName);

            if(!activeDialogue.DialogueSO.HasContinuation(dialogueNode))
            {
                activeWindow.NextButtonContainer.SetActive(false);
            }

            activeWindow.CloseButton.onClick.AddListener(EndDialogue);
            activeWindow.RequestContinuation += DisplayContinuation;
            //(^even for a choice node where the responses have no continuation (i.e. the choices are just 
            //closing remarks) we should subscribe DisplayContinuation so that the dialogue closes
            //when that last choice is selected)

            activeDialogue.ExecuteEntryActions(currentNode);
        }

        private void DisplayContinuation(int index)
        {
            if (currentNode is ResponseChoicesDialogueNode c)
            {
                activeDialogue.ExecuteResponseActions(c, index);
            }
            else if (currentNode is DecisionDialogueNode d)
            {
                index = activeDialogue.DecideContinuation(d);
            }

            activeDialogue.ExecuteExitActions(currentNode);

            if(!activeDialogue.DialogueSO.TryGetContinuation(currentNode, index, out var continuation))
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

            DialogueTrigger.DialogueTriggered -= HandleDialogueTrigger;
            //DialogueTrigger.DialogueCancelled -= EndDialogue;

            DialogueBeganSuccessfully = null;
            DialogueEnded = null;
        }
    }
}
