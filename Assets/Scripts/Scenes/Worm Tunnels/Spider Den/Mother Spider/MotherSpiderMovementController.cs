using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class MotherSpiderMovementController : AIMovementController
    {
        IKLegWalker[] legAnimators;

        public override Vector3 MoveInput
        {
            get => base.MoveInput;
            set
            {
                base.MoveInput = value;
                if (!legAnimators[0].Reversed && MoveInput.z < 0)
                {
                    SetLegsReversed(true);
                }
                else if (legAnimators[0].Reversed && MoveInput.z >= 0)
                {
                    SetLegsReversed(false);
                }

                if (legAnimators[0].steppingDisabled && Moving)
                {
                    DisableStepping(false);
                }
                else if (!Moving)
                {
                    ReturnLegsToInitialPositions(false);
                    DisableStepping(true);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            legAnimators = GetComponentsInChildren<IKLegWalker>(true);
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

        //public override void SoftStop()
        //{
        //    SetLegsReversed(false);

        //    base.SoftStop();

        //    ReturnLegsToInitialPositions(false);
        //    DisableStepping(true);
        //}

        private void ReturnLegsToInitialPositions(bool snapToPosition)
        {
            foreach (var l in legAnimators)
            {
                l.InitializeFootPosition(snapToPosition);
            }
        }

        private void SetLegsReversed(bool reversed)
        {
            Debug.Log($"setting legs reversed: {reversed}");
            foreach (var l in legAnimators)
            {
                l.Reversed = reversed;
            }
        }

        private void DisableStepping(bool disable)
        {
            foreach (var l in legAnimators)
            {
                l.steppingDisabled = disable;
            }
        }
    }
}