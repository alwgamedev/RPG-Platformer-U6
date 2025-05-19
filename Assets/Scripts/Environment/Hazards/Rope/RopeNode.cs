using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(VisualCurveGuidePoint))]
    public class RopeNode : GenericClimbNode<RopeNode>
    {
        public VisualCurveGuidePoint CurveGuidePoint { get; private set; }
        public CircleCollider2D CircleCollider { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            CurveGuidePoint = GetComponent<VisualCurveGuidePoint>();
            CircleCollider = GetComponent<CircleCollider2D>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateTangentDirection();
        }

        private void UpdateTangentDirection()
        {
            var p = Higher ? Higher.transform.position : transform.position;
            var q = Lower ? Lower.transform.position : transform.position;

            CurveGuidePoint.SetTangentDir(q - p);
        }
    }
}