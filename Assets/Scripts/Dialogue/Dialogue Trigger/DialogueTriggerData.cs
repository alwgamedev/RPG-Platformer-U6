using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Events;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public class DialogueTriggerData : ISerializationCallbackReceiver
    {
        [SerializeField] bool allowPlayerToEnterCombatDuringDialogue;
        [SerializeField] DialogueSO dialogueSO;
        [SerializeField] DialogueActorData actors;
        [SerializeField] DialogueAction[] entryActions;
        [SerializeField] DialogueAction[] exitActions;
        [SerializeField] DialogueResponseAction[] responseActions;

        Dictionary<string, Dictionary<string, UnityEvent<string[]>>> EntryActions = new();
        Dictionary<string, Dictionary<string, UnityEvent<string[]>>> ExitActions = new();
        Dictionary<string, Dictionary<int, Dictionary<string, UnityEvent<string[]>>>> ResponseActions = new();
        //nodeId => (responseIndex => (actionName => event))

        public bool AllowPlayerToEnterCombatDuringDialogue => allowPlayerToEnterCombatDuringDialogue;
        public DialogueSO DialogueSO => dialogueSO;

        //NOTE: it might seem useful to be able to "find actor by name,"
        //e.g. to execute an action on one of the dialogue actor scripts,
        //but those actions would be stored in the DialogueSO
        //where the actors are identified by their speaker index,
        //hence for now there is no use for "find actor by name"

        public string SpeakerName(DialogueNode node)
        {
            return actors[node.SpeakerIndex()].labelledObject.ActorName;
        }

        public IEnumerable<DialogueActor> Actors()
        {
            return actors.GetActors();
        }

        public DialogueActor Actor(int index)
        {
            return actors[index].labelledObject;
        }

        public int DecideContinuation(DecisionDialogueNode decisionNode)
        {
            var decisionFunction = decisionNode.DecisionFunctionData();
            return Actor(decisionFunction.ActorIndex).MakeDecision(decisionFunction.FunctionData);
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

        public void ExecuteResponseActions(ResponseChoicesDialogueNode choicesNode, int responseIndex)
        {
            //all this crap probably unnecessary
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
            actors.OnBeforeSerialize(dialogueSO);
            BeforeSerializeEntryActions();
            BeforeSerializeExitActions();
            BeforeSerializeResponseActions();
        }

        public void OnAfterDeserialize()
        {
            actors.OnAfterDeserialize();
            AfterDeserializeEntryActions();
            AfterDeserializeExitActions();
            AfterDeserializeResponseActions();
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
                var dActions = node.EntryActions();

                if (dActions == null || dActions.Count == 0)
                {
                    continue;
                }

                var id = node.UniqueID();

                if (!EntryActions.ContainsKey(id))
                {
                    EntryActions[id] = new();
                }

                var nodeDict = EntryActions[id];

                foreach (var action in dActions)
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
                var dActions = node.ExitActions();

                if (dActions == null || dActions.Count == 0)
                {
                    continue;
                }

                var id = node.UniqueID();

                if (!ExitActions.ContainsKey(id))
                {
                    ExitActions[id] = new();
                }

                var nodeDict = ExitActions[id];

                foreach (var action in dActions)
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
                var choicesNode = node as ResponseChoicesDialogueNode;

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

        private void AfterDeserializeEntryActions()
        {
            EntryActions = new();

            if (entryActions == null) return;

            for (int i = 0; i < entryActions.Length; i++)
            {
                var action = entryActions[i];
                if (!EntryActions.ContainsKey(action.NodeID))
                {
                    EntryActions[action.NodeID] = new();
                }
                EntryActions[action.NodeID][action.ActionName] = action.Trigger;
            }
        }

        private void AfterDeserializeExitActions()
        {
            ExitActions = new();

            if (exitActions == null) return;

            for (int i = 0; i < exitActions.Length; i++)
            {
                var action = exitActions[i];
                if (!ExitActions.ContainsKey(action.NodeID))
                {
                    ExitActions[action.NodeID] = new();
                }
                ExitActions[action.NodeID][action.ActionName] = action.Trigger;
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
