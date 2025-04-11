using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolBounds : MBNavigationParameters
    {
        [SerializeField] Transform[] bounds;

        public override object Content => bounds;
    }
}