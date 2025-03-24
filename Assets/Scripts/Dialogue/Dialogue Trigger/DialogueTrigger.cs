using RPGPlatformer.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] List<DialogueTriggerData> dialogues;

        public event Action DialogueCancelRequested;
        public event Action<bool> DialogueBeganSuccessfully;
        public event Action DialogueEnded;

        public static event Action<DialogueTrigger, DialogueTriggerData> DialogueTriggered;

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
        }
    }
}