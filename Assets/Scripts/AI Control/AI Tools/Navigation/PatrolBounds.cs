using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolBounds : MBNavigationParameters
    {
        [SerializeField] Transform leftBound;
        [SerializeField] Transform rightBound;

        public override object Content => (leftBound, rightBound);
    }
}