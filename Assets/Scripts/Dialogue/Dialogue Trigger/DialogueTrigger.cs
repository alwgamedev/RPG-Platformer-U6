using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] List<DialogueTriggerData> dialogues;

        public static event Action<DialogueTriggerData> DialogueTriggered;
        public static event Action DialogueCancelled;

        public void CancelDialogue()
        {
            DialogueCancelled?.Invoke();
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
            DialogueTriggered?.Invoke(data);
        }
    }
}