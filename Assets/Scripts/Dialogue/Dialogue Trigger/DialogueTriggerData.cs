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
        [SerializeField] DialogueResponseAction[] responseActions;

        Dictionary<int, Conversant> Conversants = new();
        Dictionary<string, Dictionary<string, UnityEvent<string[]>>> EntryActions = new();
        Dictionary<string, Dictionary<string, UnityEvent<string[]>>> ExitActions = new();
        Dictionary<string, Dictionary<int, Dictionary<string, UnityEvent<string[]>>>> ResponseActions = new();
        //nodeId => (responseIndex => (actionName => event))

        public bool AllowPlayerToEnterCombatDuringDialogue => allowPlayerToEnterCombatDuringDialogue;
        public DialogueSO DialogueSO => dialogueSO;

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

            foreach (var action in dialogueNode.EntryActions())
            {
                if (dict.TryGetValue(action.ActionName, out var trigger))
                {
                    trigger?.Invoke(action.Parameters);
                }
            }
        }

        public void ExecuteExitActions(DialogueNode dialogueNode)
        {
            if (dialogueNode == null || dialogueNode.ExitActions() == null 
                || !ExitActions.TryGetValue(dialogueNode.UniqueID(), out var dict)
                || dict == null) return;

            foreach (var action in dialogueNode.ExitActions())
            {
                if (dict.TryGetValue(action.ActionName, out var trigger))
                {
                    trigger?.Invoke(action.Parameters);
                }
            }
        }

        public void ExecuteResponseActions(ChoicesDialogueNode choicesNode, int responseIndex)
        {
            if (choicesNode == null || !choicesNode.ValidResponse(responseIndex)
                || choicesNode.ResponseChoices()[responseIndex].responseActions == null
                || !ResponseActions.TryGetValue(choicesNode.UniqueID(), out var nodeDict)
                || nodeDict == null || !nodeDict.TryGetValue(responseIndex, out var responseDict)
                || responseDict == null)
                return;

            foreach (var action in choicesNode.ResponseChoices()[responseIndex].responseActions)
            {
                if (responseDict.TryGetValue(action.ActionName, out var trigger))
                {
                    trigger?.Invoke(action.Parameters);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            BeforeSerializeConversants();
            BeforeSerializeEntryActions();
            BeforeSerializeExitActions();
            BeforeSerializeResponseActions();
        }

        public void OnAfterDeserialize()
        {
            AfterDeserializeConversants();
            AfterDeserializeEntryActions();
            AfterDeserializeExitActions();
            AfterDeserializeResponseActions();
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

            HashSet<(string, string)> keys = new();

            foreach (var node in dialogueSO.Nodes())
            {
                if (node == null || node.EntryActions() == null)
                    continue;

                var id = node.UniqueID();

                if (!EntryActions.ContainsKey(id))
                {
                    EntryActions[id] = new();
                }

                var nodeDict = EntryActions[id];
                
                foreach (var action in node.EntryActions())
                {
                    if (!nodeDict.ContainsKey(action.ActionName))
                    {
                        nodeDict[action.ActionName] = new();
                    }

                    keys.Add((id, action.ActionName));
                }
            }

            entryActions = new DialogueAction[keys.Count];

            int j = 0;

            foreach (var key in keys)
            {
                entryActions[j] = new DialogueAction(key.Item1, key.Item2,
                    EntryActions[key.Item1][key.Item2]);
                j++;
            }
        }

        private void BeforeSerializeExitActions()
        {
            if (dialogueSO == null || dialogueSO.Nodes() == null)
            {
                exitActions = new DialogueAction[0];
                return;
            }

            HashSet<(string, string)> keys = new();

            foreach (var node in dialogueSO.Nodes())
            {
                if (node == null || node.ExitActions() == null)
                    continue;

                var id = node.UniqueID();

                if (!ExitActions.ContainsKey(id))
                {
                    ExitActions[id] = new();
                }

                var nodeDict = ExitActions[id];

                foreach (var action in node.ExitActions())
                {
                    if (!nodeDict.ContainsKey(action.ActionName))
                    {
                        nodeDict[action.ActionName] = new();
                    }

                    keys.Add((id, action.ActionName));
                }
            }

            exitActions = new DialogueAction[keys.Count];

            int j = 0;

            foreach (var key in keys)
            {
                exitActions[j] = new DialogueAction(key.Item1, key.Item2,
                    ExitActions[key.Item1][key.Item2]);
                j++;
            }
        }

        private void BeforeSerializeResponseActions()
        {
            if (dialogueSO == null || dialogueSO.Nodes() == null)
            {
                exitActions = new DialogueAction[0];
                return;
            }

            HashSet<(string, int, string)> keys = new();

            foreach (var node in dialogueSO.Nodes())
            {
                var choicesNode = node as ChoicesDialogueNode;

                if (choicesNode == null || choicesNode.ResponseChoices() == null)
                    continue;

                var id = choicesNode.UniqueID();

                if (!ResponseActions.ContainsKey(id))
                {
                    ResponseActions[id] = new();
                }

                var choices = choicesNode.ResponseChoices();
                var nodeDict = ResponseActions[id];
                
                for (int i = 0; i < choices.Count; i++)
                {
                    if (choices[i]?.responseActions == null) continue;

                    if (!nodeDict.ContainsKey(i))
                    {
                        nodeDict[i] = new();
                    }

                    var choiceDict = nodeDict[i];
                    //Dictionary<action, UnityEvent> -- actions for i-th response choice

                    foreach (var action in choices[i].responseActions)
                    {
                        if (!choiceDict.ContainsKey(action.ActionName))
                        {
                            choiceDict[action.ActionName] = new();
                        }

                        keys.Add((id, i, action.ActionName));
                    }
                }
            }

            responseActions = new DialogueResponseAction[keys.Count];

            int j = 0;

            foreach (var key in keys)
            {
                responseActions[j] = new DialogueResponseAction(key.Item1, key.Item2, key.Item3,
                    ResponseActions[key.Item1][key.Item2][key.Item3]);
                j++;
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

        private void AfterDeserializeResponseActions()
        {
            ResponseActions = new();

            if (responseActions == null) return;

            for (int i = 0; i < responseActions.Length; i++)
            {
                var d = responseActions[i];

                if (!ResponseActions.ContainsKey(d.NodeID))
                {
                    ResponseActions[d.NodeID] = new();
                }
                if (!ResponseActions[d.NodeID].ContainsKey(d.ResponseIndex))
                {
                    ResponseActions[d.NodeID][d.ResponseIndex] = new();
                }

                ResponseActions[d.NodeID][d.ResponseIndex][d.ActionName] = d.Trigger;
            }
        }
    }
}
