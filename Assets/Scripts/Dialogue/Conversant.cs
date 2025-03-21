using RPGPlatformer.AIControl;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RPGPlatformer.Dialogue
{
    public class Conversant : MonoBehaviour
    {
        [SerializeField] protected string conversantName;

        public virtual string ConversantName => conversantName;
    }
}