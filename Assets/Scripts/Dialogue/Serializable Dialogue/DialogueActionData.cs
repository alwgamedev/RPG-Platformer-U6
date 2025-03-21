using System;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public struct DialogueActionData
    {
        [field: SerializeField]
        public string ActionName { get; private set; }
        [field: SerializeField]
        public string[] Parameters { get; private set; }
    }
}