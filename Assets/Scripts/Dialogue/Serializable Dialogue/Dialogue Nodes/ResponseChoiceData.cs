using System;
using System.Collections.Generic;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public class ResponseChoiceData
    {
        public string choiceText;
        public string continuationID;
        public List<DialogueActionData> responseActions = new();

        public ResponseChoiceData(string choiceText = "", string continuationID = null)
        {
            this.choiceText = choiceText;
            this.continuationID = continuationID;

        }

        public ResponseChoiceData(string choiceText, string continuationID, 
            List<DialogueActionData> responseActions) 
            : this(choiceText, continuationID)
        {
            this.responseActions = responseActions;
        }

        public void RemoveContinuation()
        {
            continuationID = null;
        }
    }
}