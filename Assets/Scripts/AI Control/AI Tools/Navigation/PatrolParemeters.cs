using UnityEngine;

namespace RPGPlatformer.AIControl
{
    //could be an interface instead; doesn't really matter rn
    public abstract class PatrolParemeters : MonoBehaviour
    {
        public abstract object[] Content { get; }
    }
}