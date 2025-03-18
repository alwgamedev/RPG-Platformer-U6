using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolBounds : PatrolParemeters
    {
        [SerializeField] Transform[] bounds;

        public override object Content => bounds;
    }
}