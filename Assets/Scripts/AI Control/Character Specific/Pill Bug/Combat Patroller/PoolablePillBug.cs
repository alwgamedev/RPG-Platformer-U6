using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PoolablePillBug : PoolableAIPatroller
    {
        //PillBugMover mover;
        PillBugContainer container;

        protected override Vector3 Position => container.Mover.transform.position;

        protected override void Awake()
        {
            base.Awake();
            //mover = GetComponentInChildren<PillBugMover>();
            container = GetComponent<PillBugContainer>();
        }

        //public override void BeforeSetActive()
        //{
        //    base.BeforeSetActive();
        //    //container.PositionHealthBarCanvas();
        //}

        public override void SetPosition(Vector3 position)
        {
            container.Mover.SetPosition(position);
            container.PositionHealthBarCanvas();
        }
    }
}