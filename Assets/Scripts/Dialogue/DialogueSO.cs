using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Dialogue 
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/New Dialogue")]
    public class DialogueSO : ScriptableObject
    {
        [SerializeField] DialogueNode rootNode;
        [SerializeField] List<DialogueNode> nodes = new();
        
        Dictionary<string, DialogueNode> NodeLookup = new();

        private void OnValidate()
        {
            RebuildNodeLookup();
        }

        private void RebuildNodeLookup()
        {
            NodeLookup.Clear();
            foreach (DialogueNode node in nodes)
            {
                NodeLookup[node.UniqueID()] = node;
            }
        }

        public DialogueNode RootNode()
        {
            return rootNode;
        }

        public IEnumerable<DialogueNode> Nodes()
        {
            return nodes;
        }

        public bool HasContinuation(DialogueNode node)
        {
            if (node is ChoicesDialogueNode choicesNode)
            {
                for (int i = 0; i < choicesNode.ResponseChoices().Count; i++)
                {
                    if (TryGetContinuation(choicesNode, i, out _))
                    {
                        return true;
                    }
                }
                return false;
            }
            return TryGetContinuation(node, 0, out _);
        }

        public bool TryGetContinuation(DialogueNode node, int responseIndex, out DialogueNode continuation)
        {
            continuation = null;
            string id = node.ContinuationID(responseIndex);
            if (id != null)
            {
                if(NodeLookup == null || NodeLookup.Count == 0)
                {
                    RebuildNodeLookup();
                }
                if (NodeLookup.TryGetValue(id, out continuation))
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        public void SetRootNode(DialogueNode dialogueNode)
        {
            rootNode = dialogueNode;
            AssetDatabase.SaveAssets();
        }

        public DialogueNode CreateNode<T>(Vector2 position = default) where T : DialogueNode
        {
            DialogueNode newNode = ScriptableObject.CreateInstance<T>();
            newNode.Initialize(position);
            nodes.Add(newNode);
            OnValidate();

            AssetDatabase.AddObjectToAsset(newNode, this);
            AssetDatabase.SaveAssets();

            return newNode;
        }

        public void DeleteNode(DialogueNode node)
        {
            nodes.Remove(node);
            EraseAnyOccurencesOfChild(node);
            OnValidate();

            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();

            Undo.DestroyObjectImmediate(node);
        }

        public void EraseAnyOccurencesOfChild(DialogueNode nodeToRemove)
        {
            foreach(DialogueNode node in nodes)
            {
                if (node != null)
                {
                    node.EraseAnyOccurrencesOfChild(nodeToRemove);
                }
            }
        }

        public void SetContinuation(DialogueNode parent, DialogueNode child, int responseIndex)
        {
            if (parent != null && child != null)
            {
                parent.SetContinuation(child, responseIndex);
            }
        }

        public void RemoveChild(DialogueNode parent, DialogueNode child, int responseIndex)
        {
            if (parent != null && child != null)
            {
                parent.RemoveContinuation(responseIndex);
            }
        }
#endif
    }
}
