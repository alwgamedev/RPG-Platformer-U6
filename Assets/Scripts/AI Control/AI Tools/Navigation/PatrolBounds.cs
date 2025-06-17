using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolBounds : MBNavigationParameters
    {
        [SerializeField] RandomizableVector3 bounds;

        public override object Content => bounds;
    }
}