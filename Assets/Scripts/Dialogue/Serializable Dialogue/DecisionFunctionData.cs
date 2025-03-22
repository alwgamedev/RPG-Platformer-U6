using System;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public struct DecisionFunctionData
    {
        [SerializeField] int actorIndex;//for now we'll assume decision always made by one of the dialogue actors
        [SerializeField] DialogueActionData functionData;

        public int ActorIndex => actorIndex;
        public DialogueActionData FunctionData => functionData;
    }
}