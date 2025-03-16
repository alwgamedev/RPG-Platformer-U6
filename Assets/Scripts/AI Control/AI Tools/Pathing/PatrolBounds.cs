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
            if (left != null && right != null)
            { 
                bounds = new Transform[] { left, right }; 
            }
        }
    }
}