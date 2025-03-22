using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class DialogueActor : MonoBehaviour
    {
        [SerializeField] protected string actorName;

        Dictionary<string, Func<string[], int>> GetDecisionFunction;

        public virtual string ActorName => actorName;

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