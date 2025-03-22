using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using RPGPlatformer.Dialogue;
using RPGPlatformer.Combat;
using System;

namespace RPGPlatformer.UI
{
    public class DialogueUI : HidableUI
    {
        [SerializeField] DialogueWindow dialogueWindowPrefab;

        DialogueTriggerData activeDialogue;
        DialogueNode currentNode;
        DialogueWindow activeWindow;

        public event Action DialogueEnded;

        protected override void Awake()
        {
            base.Awake();

            DialogueTrigger.DialogueTriggered += HandleDialogueTrigger;
            DialogueTrigger.DialogueCancelled += EndDialogue;
        }

        public void StartDialogue(DialogueTriggerData data)
        {
            activeDialogue = data;
            currentNode = activeDialogue.DialogueSO.RootNode();
            DisplayDialogueNode(currentNode);
            Show();
        }

        public void EndDialogue()
        {
            CloseActiveWindow();
            activeDialogue = null;
            currentNode = null;
            DialogueEnded?.Invoke();
            Hide();
        }

        private void HandleDialogueTrigger(DialogueTriggerData data)
        {
            if (activeDialogue != null)
            {
                GameLog.Log("Exit the current dialogue before starting another.");
                return;
            }

            //if (!data.IsValid())
            //{
            //    Debug.Log($"Unable to start dialogue, because {nameof(DialogueTriggerData)} is invalid.");
            //    return;
            //}

            if (!data.AllowPlayerToEnterCombatDuringDialogue)
            {
                var player = FindAnyObjectByType<PlayerCombatController>();

                if (player != null)
                {
                    if (player.IsInCombat)
                    {
                        CancelOnCombatEntry();
                        return;
                    }

                    player.CombatEntered += CancelOnCombatEntry;
                    DialogueEnded += DialogueEndedHandler;

                    void CancelOnCombatEntry()
                    {
                        GameLog.Log("You cannot participate in this dialogue while in combat.");
                        EndDialogue();
                    }

                    void DialogueEndedHandler()
                    {
                        player.CombatEntered -= CancelOnCombatEntry;
                        DialogueEnded -= DialogueEndedHandler;
                        //because we still need to unsubscribe when the dialogue ends naturally
                    }
                }

                StartDialogue(data);
            }
        }

        private void DisplayDialogueNode(DialogueNode dialogueNode)
        {
            CloseActiveWindow();

            currentNode = dialogueNode;
            var conversantName = activeDialogue.SpeakerName(dialogueNode);

            activeWindow = Instantiate(dialogueWindowPrefab, transform);
            activeWindow.SetUpWindow(dialogueNode, conversantName);

            if(!activeDialogue.DialogueSO.HasContinuation(dialogueNode))
            {
                activeWindow.NextButtonContainer.SetActive(false);
            }

            activeWindow.CloseButton.onClick.AddListener(EndDialogue);
            activeWindow.ResponseSelected += DisplayContinuation;
            //(^even for a choice node where the responses have no continuation (i.e. the choices are just 
            //closing remarks) we should subscribe DisplayContinuation so that the dialogue closes
            //when that last choice is selected)

            activeDialogue.ExecuteEntryActions(currentNode);
        }

        private void DisplayContinuation(int responseIndex)
        {
            if (currentNode is ChoicesDialogueNode c)
            {
                activeDialogue.ExecuteResponseActions(c, responseIndex);
            }

            activeDialogue.ExecuteExitActions(currentNode);

            if(!activeDialogue.DialogueSO.TryGetContinuation(currentNode, responseIndex, out var continuation))
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
            DialogueTrigger.DialogueCancelled -= EndDialogue;
        }
    }
}
