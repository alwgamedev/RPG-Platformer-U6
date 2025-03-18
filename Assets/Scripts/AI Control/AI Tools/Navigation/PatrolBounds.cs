using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolBounds : PatrolParemeters
    {
        [SerializeField] Transform left;
        [SerializeField] Transform right;

        Transform[] bounds;

        public override object[] Content => bounds;

        private void Awake()
        {
            bounds = new Transform[] { left, right }; 
        }
    }
}