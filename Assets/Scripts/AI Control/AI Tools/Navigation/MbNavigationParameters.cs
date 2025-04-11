using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public abstract class MBNavigationParameters : MonoBehaviour, NavigationParameters
    {
        public virtual object Content { get; }
    }
}