using RPGPlatformer.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class DialogueTrigger : MonoBehaviour, IDialogueTrigger
    {
        [SerializeField] List<DialogueTriggerData> dialogues = new();

        bool triggerEnabled = true;

        public event Action DialogueCancelRequested;
        public event Action<bool> DialogueBeganSuccessfully;
        public event Action DialogueEnded;
        public event Action<bool> TriggerEnabled;

        public static event Action<DialogueTrigger, DialogueTriggerData> DialogueTriggered;

        private void Start()
        {
            foreach (var d in dialogues)
            {
                d?.OnStart();
            }
        }

        //NOTE: if your trigger is attached to an interactable NPC, it may be better to use
        //NPC.SetCursorTypeAndPrimaryAction (it feels like a higher level script, which we should go to first)
        public void EnableTrigger(bool val)
        {
            if (val != triggerEnabled)
            {
                triggerEnabled = val;
                TriggerEnabled?.Invoke(val);
            }
        }

        public void RequestCancelDialogue()
        {
            DialogueCancelRequested?.Invoke();
        }

        public void TriggerDialogue(string dialogueName)
        {
            var data = dialogues.FirstOrDefault(x => x.DialogueSO.name == dialogueName);
            TriggerDialogue(data);
        }

        public void TriggerDialogue(int index)
        {
            if (index < 0 || index >= dialogues.Count) return;

            TriggerDialogue(dialogues[index]);
        }

        public void TriggerDialogue(DialogueTriggerData data)
        {
            if (!triggerEnabled)
            {
                //e.g. IntNPC calls TriggerDialogue
                //and will be waiting for this event to unsubscribe certain fcts
                DialogueBeganSuccessfully?.Invoke(false);
                return;
            }

            DialogueUI.DialogueBeganSuccessfully += DialogueBeganHandler;
            DialogueTriggered?.Invoke(this, data);

            void DialogueBeganHandler(bool val)
            {
                DialogueBeganSuccessfully?.Invoke(val);
                if (val)
                {
                    DialogueUI.DialogueEnded += DialogueEndedHandler;
                }
                DialogueUI.DialogueBeganSuccessfully -= DialogueBeganHandler;
            }

            void DialogueEndedHandler()
            {
                DialogueEnded?.Invoke();
                DialogueUI.DialogueEnded -= DialogueEndedHandler;
            }
        }

        private void OnDestroy()
        {
            DialogueCancelRequested = null;
            DialogueBeganSuccessfully = null;
            DialogueEnded = null;
            TriggerEnabled = null;
        }
    }
}