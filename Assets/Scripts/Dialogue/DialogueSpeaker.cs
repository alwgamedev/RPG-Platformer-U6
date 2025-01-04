using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public abstract class DialogueSpeaker : MonoBehaviour
    {
        [SerializeField] string speakerName;

        public virtual string SpeakerName => speakerName;
    }
}