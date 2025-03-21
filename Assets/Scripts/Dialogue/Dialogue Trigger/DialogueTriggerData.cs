using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public class DialogueTriggerData : ISerializationCallbackReceiver
    {
        [SerializeField] bool allowPlayerToEnterCombatDuringDialogue;
        [SerializeField] DialogueSO dialogueSO;
        [SerializeField] Conversant[] conversants;
        [SerializeField] DialogueAction[] entryActions;
        [SerializeField] DialogueAction[] exitActions;

        Dictionary<int, Conversant> Conversants = new();
        Dictionary<string, Dictionary<string, UnityEvent<string[]>>> EntryActions = new();
        Dictionary<string, Dictionary<string, UnityEvent<string[]>>> ExitActions = new();

        public bool AllowPlayerToEnterCombatDuringDialogue => allowPlayerToEnterCombatDuringDialogue;
        public DialogueSO DialogueSO => dialogueSO;

        //public string ConversantName(int i)
        //{
        //    return conversants[i].ConversantName;
        //}

        public string ConversantName(DialogueNode node)
        {
            if (Conversants.TryGetValue(node.ConversantNumber(), out var n) && n != null)
            {
                return n.ConversantName;
            }
            return "";
        }

        public void ExecuteEntryActions(DialogueNode dialogueNode)
        {
            if (dialogueNode == null || dialogueNode.EntryActions() == null
                || !EntryActions.TryGetValue(dialogueNode.UniqueID(), out var dict)
                || dict == null) return;

            foreach (var actionData in dialogueNode.EntryActions())
            {
                if (dict.TryGetValue(actionData.ActionName, out var action))
                {
                    action?.Invoke(actionData.Parameters);
                }
            }
        }

        public void ExecuteExitActions(DialogueNode dialogueNode)
        {
            if (dialogueNode == null || dialogueNode.ExitActions() == null 
                || !ExitActions.TryGetValue(dialogueNode.UniqueID(), out var dict)
                || dict == null) return;

            foreach (var actionData in dialogueNode.ExitActions())
            {
                if (dict.TryGetValue(actionData.ActionName, out var action))
                {
                    action?.Invoke(actionData.Parameters);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            BeforeSerializeConversants();
            BeforeSerializeEntryActions();
            BeforeSerializeExitActions();
        }

        public void OnAfterDeserialize()
        {
            AfterDeserializeConversants();
            AfterDeserializeEntryActions();
            AfterDeserializeExitActions();
        }

        private void BeforeSerializeConversants()
        {
            if (dialogueSO == null || dialogueSO.ConversantNames() == null)
            {
                conversants = new Conversant[0];
                return;
            }

            conversants = new Conversant[dialogueSO.ConversantNames().Count];

            for (int i = 0; i < conversants.Length; i++)
            {
                if (Conversants.TryGetValue(i, out var c))
                {
                    conversants[i] = c;
                }
            }
        }

        private void BeforeSerializeEntryActions()
        {
            if (dialogueSO == null || dialogueSO.Nodes() == null)
            {
                entryActions = new DialogueAction[0];
                return;
            }

            var count = 0;

            foreach (var node in dialogueSO.Nodes())
            {
                if (node == null || node.EntryActions() == null)
                {
                    continue;
                }
                count += node.EntryActions().Count;
            }

            entryActions = new DialogueAction[count];

            count = 0;

            foreach (var node in dialogueSO.Nodes())
            {
                if (node == null || node.EntryActions() == null)
                {
                    continue;
                }

                var id = node.UniqueID();

                if (!EntryActions.ContainsKey(id))
                {
                    EntryActions[id] = new();
                }

                var dict = EntryActions[id];

                foreach (var action in node.EntryActions())
                {
                    if (!dict.ContainsKey(action.ActionName))
                    {
                        dict[action.ActionName] = new();
                    }

                    entryActions[count] = new(id, action.ActionName, dict[action.ActionName]);
                    count++;
                }
            }
        }

        private void BeforeSerializeExitActions()
        {
            if (dialogueSO == null || dialogueSO.Nodes() == null)
            {
                exitActions = new DialogueAction[0];
                return;
            }

            var count = 0;

            foreach (var node in dialogueSO.Nodes())
            {
                if (node == null || node.ExitActions() == null)
                {
                    continue;
                }
                count += node.ExitActions().Count;
            }

            exitActions = new DialogueAction[count];

            count = 0;

            foreach (var node in dialogueSO.Nodes())
            {
                if (node == null || node.ExitActions() == null)
                {
                    continue;
                }

                var id = node.UniqueID();

                if (!ExitActions.ContainsKey(id))
                {
                    ExitActions[id] = new();
                }

                var dict = ExitActions[id];

                foreach (var action in node.ExitActions())
                {
                    if (!dict.ContainsKey(action.ActionName))
                    {
                        dict[action.ActionName] = new();
                    }

                    exitActions[count] = new(id, action.ActionName, dict[action.ActionName]);
                    count++;
                }
            }
        }

        private void AfterDeserializeConversants()
        {
            Conversants = new();

            if (conversants == null) return;

            for (int i = 0; i < conversants.Length; i++)
            {
                Conversants[i] = conversants[i];
            }
        }

        private void AfterDeserializeEntryActions()
        {
            EntryActions = new();

            if (entryActions == null) return;

            for (int i = 0; i < entryActions.Length; i++)
            {
                var d = entryActions[i];
                if (!EntryActions.ContainsKey(d.NodeID))
                {
                    EntryActions[d.NodeID] = new();
                }
                EntryActions[d.NodeID][d.ActionName] = d.Trigger;
            }
        }

        private void AfterDeserializeExitActions()
        {
            ExitActions = new();

            if (exitActions == null) return;

            for (int i = 0; i < exitActions.Length; i++)
            {
                var d = exitActions[i];
                if (!ExitActions.ContainsKey(d.NodeID))
                {
                    ExitActions[d.NodeID] = new();
                }
                ExitActions[d.NodeID][d.ActionName] = d.Trigger;
            }
        }
    }
}
