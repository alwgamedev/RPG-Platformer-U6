using System;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public class ResponseChoiceData
    {
        public string choiceText;
        public string continuationID;

        public ResponseChoiceData(string choiceText = "", string continuationID = null)
        {
            this.choiceText = choiceText;
            this.continuationID = continuationID;
        }

        public void RemoveContinuation()
        {
            continuationID = null;
        }
    }
}