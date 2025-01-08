using RPGPlatformer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    //[RequireComponent(typeof(InteractableGameObject))]
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] List<DialogueSO> dialogues = new();

        public static event Action<DialogueSO, string, string> DialogueTriggered;
        //signature (dialogue, conversantName, playerName)
        public static event Action DialogueCancelled;

        //private void Start()
        //{
        //    var igo = GetComponent<InteractableGameObject>();
        //    if (igo)
        //    {
        //        igo.PlayerOutOfRange += () => DialogueCancelled?.Invoke();
        //    }
        //}

        public void CancelDialogue()
        {
            DialogueCancelled?.Invoke();
        }

        public void TriggerDialogue(string dialogueName, string conversantName, string playerName)
        {
            DialogueSO dialogue = dialogues.FirstOrDefault(x => x.name == dialogueName);
            TriggerDialogue(dialogue, conversantName, playerName);
        }

        public void TriggerDialogue(int index, string conversantName, string playerName)
        {
            if (index < 0 || index >= dialogues.Count) return;

            TriggerDialogue(dialogues[index], conversantName, playerName);
        }

        public void TriggerDialogue(DialogueSO dialogue, string conversantName, string playerName)
        {
            if(dialogue)
            {
                DialogueTriggered?.Invoke(dialogue, conversantName, playerName);
            }
        }
    }
}