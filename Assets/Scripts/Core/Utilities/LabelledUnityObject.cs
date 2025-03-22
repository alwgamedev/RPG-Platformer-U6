using UnityEngine;
using System;

namespace RPGPlatformer.Core
{
    //this is just so we can get list views with custom element labels (instead of Element 0, Element 1, ...)
    //for the DialogueActorData inspector
    [Serializable]
    public struct LabelledUnityObject<T> where T : UnityEngine.Object
    {
        public string label;
        public T labelledObject;

        public LabelledUnityObject(string label, T labelledObject)
        {
            this.label = label;
            this.labelledObject = labelledObject;
        }
    }
}
