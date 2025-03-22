using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class ResponseChoicesDialogueNode : DialogueNode
    {
        [SerializeField] List<ResponseChoiceData> responseChoices = new();

        public List<ResponseChoiceData> ResponseChoices()
        {
            return responseChoices;
        }

        public bool ValidResponse(int responseIndex)
        {
            return responseChoices != null && responseIndex >= 0 && responseIndex < responseChoices.Count;
        }

        public override string ContinuationID(int responseIndex)
        {
            if (responseIndex < 0 || responseIndex >= responseChoices.Count) return null;
            return responseChoices[responseIndex]?.continuationID;
        }

#if UNITY_EDITOR
        public void AddResponseChoice(string response = "")
        {
            responseChoices.Add(new(response));
            EditorUtility.SetDirty(this);
        }

        public void SetResponseChoiceText(int index, string response)
        {
            if(index < 0 || index >= responseChoices.Count) return;
            responseChoices[index] ??= new();
            responseChoices[index].choiceText = response;
            EditorUtility.SetDirty(this);
        }

        public void RemoveResponseChoice(int index)
        {
            responseChoices.RemoveAt(index);
            EditorUtility.SetDirty(this);
        }

        public override void SetContinuation(DialogueNode node, int responseIndex)
        {
            responseChoices[responseIndex].continuationID = node.UniqueID();
            EditorUtility.SetDirty(this);
        }

        public override void RemoveContinuation(int responseIndex)
        {
            responseChoices[responseIndex].RemoveContinuation();
            EditorUtility.SetDirty(this);
        }

        public override void EraseAnyOccurrencesOfChild(DialogueNode child)
        {
            foreach (var response in responseChoices)
            {
                if(response.continuationID == child.UniqueID())
                {
                    response.RemoveContinuation();
                }
            }
            EditorUtility.SetDirty(this);
        }
#endif
    }
}