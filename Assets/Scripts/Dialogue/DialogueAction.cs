using System;
using UnityEngine;
using UnityEngine.Events;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public class DialogueAction
    {
        [SerializeField] string nodeID;
        [SerializeField] string actionName;
        [SerializeField] UnityEvent<string[]> trigger = new();

        public string NodeID => nodeID;
        public string ActionName => actionName;
        public UnityEvent<string[]> Trigger => trigger;

        public DialogueAction(string nodeID, string actionName, UnityEvent<string[]> trigger)
        {
            this.nodeID = nodeID;
            this.actionName = actionName;
            this.trigger = trigger;
        }
    }
}