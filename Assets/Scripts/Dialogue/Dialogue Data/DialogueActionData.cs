using System;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public struct DialogueActionData
    {
        [SerializeField] string actionName;
        [SerializeField] string[] parameters;

        public string ActionName => actionName;
        public string[] Parameters => parameters;
    }
}