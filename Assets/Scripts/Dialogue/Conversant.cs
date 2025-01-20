using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public class Conversant : MonoBehaviour
    {
        [SerializeField] string conversantName;

        public virtual string ConversantName => conversantName;
    }
}