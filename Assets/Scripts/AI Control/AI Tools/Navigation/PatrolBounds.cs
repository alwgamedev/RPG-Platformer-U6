using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolBounds : MbNavigationParameters
    {
        [SerializeField] Transform[] bounds;

        public override object Content => bounds;
    }
}