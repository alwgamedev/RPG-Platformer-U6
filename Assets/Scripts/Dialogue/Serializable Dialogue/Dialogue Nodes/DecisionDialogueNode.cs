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

        public List<string> Continuations()
        {
            return continuations;
        }

        public bool ValidContinuation(int index)
        {
            return index >= 0 && index < continuations.Count;
        }

#if UNITY_EDITOR
        public void SetDecisionActor(int index)
        {
            decisionFunctionData.SetActor(index);
            EditorUtility.SetDirty(this);
        }

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
            if (ValidContinuation(index))
            {
                continuations[index] = null;
                EditorUtility.SetDirty(this);
            }
        }

        public override void SetContinuation(DialogueNode node, int index)
        {
            if (ValidContinuation(index))
            {
                continuations[index] = node.UniqueID();
                EditorUtility.SetDirty(this);
            }
        }
    }
#endif
}