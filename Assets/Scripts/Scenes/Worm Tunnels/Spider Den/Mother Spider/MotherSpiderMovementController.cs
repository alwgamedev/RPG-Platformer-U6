using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class MotherSpiderMovementController : AIMovementController
    {
        IKLimbAnimator[] legAnimators;

        public override Vector3 MoveInput
        {
            get => base.MoveInput;
            protected set
            {
                base.MoveInput = value;
                if (!legAnimators[0].Reversed && Moving && MoveInput.z < 0)
                {
                    SetLegsReversed(true);
                }
                else if (legAnimators[0].Reversed && Moving && MoveInput.z >= 0)
                {
                    SetLegsReversed(false);
                }

                //if (legAnimators[0].steppingDisabled && Moving)
                //{
                //    DisableStepping(false);
                //}
                //else if (!legAnimators[0].steppingDisabled && !Moving)
                //{
                //    DisableStepping(true);
                //}
            }
        }

        protected override void Awake()
        {
            base.Awake();

            legAnimators = GetComponentsInChildren<IKLimbAnimator>();
        }

        public void PauseLegAnimators()
        {
            foreach (var l in legAnimators)
            {
                l.paused = true;
            }
        }

        public void UnpauseLegAnimators()
        {
            foreach (var l in legAnimators)
            {
                l.paused = false;
            }
        }

        public void ResetWalkAnimation(bool snapToPosition)
        {
            foreach (var l in legAnimators)
            {
                l.ResetWalkAnimation(snapToPosition);
            }
        }

        private void SetLegsReversed(bool reversed/*, bool restoreInitialPosition = false*/)
        {
            foreach (var l in legAnimators)
            {
                l.SetReversed(reversed/*, restoreInitialPosition*/);
            }
        }

        //was using this when stopping to make sure new steps aren't started immediately after resetting initial pos
        //but it doesn't seem necessary anymore
        private void DisableStepping(bool disable)
        {
            foreach (var l in legAnimators)
            {
                l.steppingDisabled = disable;
            }
        }
    }
}