using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class DialogueActor : MonoBehaviour
    {
        [SerializeField] protected string actorName;

        protected Dictionary<string, Func<string[], int>> GetDecisionFunction = new();
        //^here we can store methods from components that don't usually take string[] parameters
        //(rather than crowding up the component class with overloads of those methods)

        public virtual string ActorName => actorName;

        protected virtual void Start()
        {
            BuildDecisionFunctionDict();
        }

        //keys need to match the function name used in the Dialogue SO exactly!
        protected virtual void BuildDecisionFunctionDict() { }

        public virtual void OnBeginDialogue() { }

        public virtual void OnEndDialogue() { }

        public int MakeDecision(DialogueActionData data)
        {
            return GetDecisionFunction[data.ActionName](data.Parameters);
        }

        public bool TryMakeDecision(DialogueActionData data, out int decision)
        {
            if (GetDecisionFunction.TryGetValue(data.ActionName, out var func) && func != null)
            {
                decision = func(data.Parameters);
                return false;
            }

            decision = 0;
            return false;
        }
    }
}