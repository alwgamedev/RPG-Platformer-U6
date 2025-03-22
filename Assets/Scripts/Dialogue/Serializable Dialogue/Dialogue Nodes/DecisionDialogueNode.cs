using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class DecisionDialogueNode : DialogueNode
    {
        [SerializeField] DecisionFunctionData decisionFunctionData;
        [SerializeField] List<string> continuations = new();

        public DecisionFunctionData DecisionFunctionData()
        {
            return decisionFunctionData;
        }

        public override string ContinuationID(int index)
        {
            return continuations[index];
        }

#if UNITY_EDITOR
        public override void EraseAnyOccurrencesOfChild(DialogueNode child)
        {
            for (int i = 0; i < continuations.Count; i++)
            {
                if (continuations[i] == child.UniqueID())
                {
                    continuations[i] = null;
                }
            }
            EditorUtility.SetDirty(this);
        }

        public override void RemoveContinuation(int index)
        {
            continuations[index] = null;
            EditorUtility.SetDirty(this);
        }

        public override void SetContinuation(DialogueNode node, int index)
        {
            continuations[index] = node.UniqueID();
            EditorUtility.SetDirty(this);
        }
    }
#endif
}