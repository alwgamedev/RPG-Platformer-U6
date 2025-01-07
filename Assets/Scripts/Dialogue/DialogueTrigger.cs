using RPGPlatformer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    [RequireComponent(typeof(InteractableGameObject))]
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] List<DialogueSO> dialogues = new();

        public string ConversantName { get; private set; }

        public static event Action<DialogueSO, string> DialogueTriggered;

        private void Start()
        {
            ConversantName = GetComponent<InteractableGameObject>().DisplayName;
        }

        public void TriggerDialogue(string dialogueName)
        {
            DialogueSO dialogue = dialogues.FirstOrDefault(x => x.name == dialogueName);
            TriggerDialogue(dialogue);
        }

        public void TriggerDialogue(int index)
        {
            if (index < 0 || index >= dialogues.Count) return;

            TriggerDialogue(dialogues[index]);
        }

        public void TriggerDialogue(DialogueSO dialogue)
        {
            if(dialogue)
            {
                DialogueTriggered?.Invoke(dialogue, ConversantName);
            }
        }
    }
}