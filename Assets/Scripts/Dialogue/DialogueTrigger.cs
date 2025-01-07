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

        public static event Action<DialogueSO, string, string> DialogueTriggered;
        //signature (dialogue, conversantName, playerName)

        private void Start()
        {
            ConversantName = GetComponent<InteractableGameObject>().DisplayName;
        }

        public void TriggerDialogue(string dialogueName, string playerName)
        {
            DialogueSO dialogue = dialogues.FirstOrDefault(x => x.name == dialogueName);
            TriggerDialogue(dialogue, playerName);
        }

        public void TriggerDialogue(int index, string playerName)
        {
            if (index < 0 || index >= dialogues.Count) return;

            TriggerDialogue(dialogues[index], playerName);
        }

        public void TriggerDialogue(DialogueSO dialogue, string playerName)
        {
            if(dialogue)
            {
                DialogueTriggered?.Invoke(dialogue, ConversantName, playerName);
            }
        }
    }
}