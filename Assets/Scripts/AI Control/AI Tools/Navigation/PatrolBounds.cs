using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolBounds : MonoBehaviour, NavigationParameters
    {
        [SerializeField] Transform[] bounds;

        public object Content => bounds;
    }
}