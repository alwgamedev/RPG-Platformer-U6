using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public abstract class MbNavigationParameters : MonoBehaviour, NavigationParameters
    {
        public virtual object Content { get; }
    }
}