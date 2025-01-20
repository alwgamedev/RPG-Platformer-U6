using RPGPlatformer.Combat;
using RPGPlatformer.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    //[RequireComponent(typeof(InteractableGameObject))]
    public class DialogueTrigger : MonoBehaviour
    {
        //[SerializeField] List<DialogueSO> dialogues = new();
        [SerializeField] List<DialogueTriggerData> dialogues;

        public static event Action<DialogueTriggerData> DialogueTriggered;
        //signature (dialogue, conversantName, playerName)
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
            if (!data.AllowPlayerToEnterCombatDuringDialogue)
            {
                var player = FindAnyObjectByType<PlayerCombatController>();
                if (player != null)
                {
                    Debug.Log($"is the player in combat? {player.IsInCombat}");
                    if (player.IsInCombat)
                    {
                        CancelOnCombatEntry();
                        return;

                    }

                    player.CombatEntered += CancelOnCombatEntry;
                    DialogueCancelled += CancellationHandler;

                    void CancelOnCombatEntry()
                    {
                        GameLog.Log("You cannot participate in this dialogue while in combat.");
                        CancelDialogue();
                    }

                    void CancellationHandler()
                    {
                        player.CombatEntered -= CancelOnCombatEntry;
                        DialogueCancelled -= CancellationHandler;
                    }
                }
            }

            DialogueTriggered?.Invoke(data);
        }
    }
}