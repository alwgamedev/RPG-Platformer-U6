using UnityEngine;
using UnityEngine.Events;

namespace RPGPlatformer.Dialogue
{
    public class DialogueResponseAction : DialogueAction
    {
        [SerializeField] int responseIndex;

        public int ResponseIndex => responseIndex;

        public DialogueResponseAction(string nodeID, int responseIndex, string actionName, UnityEvent<string[]> trigger)
            : base(nodeID, actionName, trigger)
        {
            this.responseIndex = responseIndex;
        }
    }
}