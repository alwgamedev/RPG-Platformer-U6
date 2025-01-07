using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class AutoContinuationDialogueNode : DialogueNode
    {
        [SerializeField] string continuationID;

        public override string ContinuationID(int responseIndex)
        {
            return continuationID;
        }

#if UNITY_EDITOR
        public override void SetContinuation(DialogueNode node, int responseIndex)
        {
            if(node)
            {
                continuationID = node.UniqueID();
                EditorUtility.SetDirty(this);
            }
        }

        public override void RemoveContinuation(int responseIndex)
        {
            continuationID = null;
            EditorUtility.SetDirty(this);
        }

        public override void EraseAnyOccurrencesOfChild(DialogueNode child)
        {
            if(continuationID == child.UniqueID())
            {
                continuationID = null;
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}