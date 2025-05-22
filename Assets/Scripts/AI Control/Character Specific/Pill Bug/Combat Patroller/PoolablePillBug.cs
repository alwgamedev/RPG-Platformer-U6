using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PoolablePillBug : PoolableAIPatroller
    {
        PillBugContainer container;

        protected override Vector3 Position => container.Mover.transform.position;

        protected override void Awake()
        {
            base.Awake();
            container = GetComponent<PillBugContainer>();
        }

        public override void SetPosition(Vector3 position)
        {
            container.Mover.SetPosition(position);
            container.PositionHealthBarCanvas();
        }
    }
}