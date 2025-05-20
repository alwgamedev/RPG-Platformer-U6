using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolBounds : MBNavigationParameters
    {
        //[SerializeField] Transform leftBound;
        //[SerializeField] Transform rightBound;
        [SerializeField] RandomizableVector3 bounds;

        public override object Content => bounds;//(leftBound, rightBound);
    }
}