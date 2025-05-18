using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(VisualCurveGuidePoint))]
    public class RopeNode : ClimbableObject
    {
        public float spacing2;

        private void FixedUpdate()
        {
            //maintain distance to previous
        }
    }
}