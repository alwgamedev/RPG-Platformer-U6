using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public struct DialogueTriggerData
    {
        [SerializeField] bool allowPlayerToEnterCombatDuringDialogue;
        [SerializeField] DialogueSO dialogueSO;
        [SerializeField] List<Conversant> conversants;

        public bool AllowPlayerToEnterCombatDuringDialogue;
        public DialogueSO DialogueSO => dialogueSO;
        public List<Conversant> Conversants => conversants;

        public bool IsValid()
        {
            return dialogueSO != null && conversants != null && dialogueSO.NumConversants() == conversants.Count;
        }

        public string ConversantName(int i)
        {
            return conversants[i].ConversantName;
        }

        public string ConversantName(DialogueNode node)
        {
            return conversants[node.ConversantNumber()].ConversantName;
        }
    }
}
