using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class DialogueActor : MonoBehaviour
    {
        [SerializeField] protected string actorName;

        protected Dictionary<string, Func<string[], int>> DecisionFunction = new();

        public virtual string ActorName => actorName;

        protected virtual void Start()
        {
            BuildDecisionFunctionDict();
        }

        protected virtual void BuildDecisionFunctionDict() { }

        public virtual void OnBeginDialogue() { }

        public virtual void OnEndDialogue() { }

        public int MakeDecision(DialogueActionData data)
        {
            return DecisionFunction[data.ActionName](data.Parameters);
        }

        public bool TryMakeDecision(DialogueActionData data, out int decision)
        {
            if (DecisionFunction.TryGetValue(data.ActionName, out var func) && func != null)
            {
                decision = func(data.Parameters);
                return false;
            }

            decision = 0;
            return false;
        }
    }
}