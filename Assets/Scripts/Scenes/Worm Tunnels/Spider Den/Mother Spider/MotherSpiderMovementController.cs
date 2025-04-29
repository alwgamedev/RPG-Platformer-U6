using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class MotherSpiderMovementController : AIMovementController
    {
        IKLegWalker[] legAnimators;

        protected override void Awake()
        {
            base.Awake();

            legAnimators = GetComponentsInChildren<IKLegWalker>(true);
        }

        public void PauseLegAnimators()
        {
            foreach (var l in legAnimators)
            {
                if (l)
                {
                    l.paused = true;
                }
            }
        }

        public void UnpauseLegAnimators()
        {
            foreach (var l in legAnimators)
            {
                if (l)
                {
                    l.paused = false;
                }
            }
        }
    }
}