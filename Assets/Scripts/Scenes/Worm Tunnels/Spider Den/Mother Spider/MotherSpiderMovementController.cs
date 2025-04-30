using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class MotherSpiderMovementController : AIMovementController
    {
        IKLegWalker[] legAnimators;

        //public override Vector3 MoveInput
        //{
        //    get => base.MoveInput;
        //    set
        //    {
        //        base.MoveInput = value;
        //        if (!legAnimators[0].Reversed && MoveInput.z < 0)
        //        {
        //            SetLegsReversed(true);
        //        }
        //        else if (legAnimators[0].Reversed && MoveInput.z > 0)
        //        {
        //            SetLegsReversed(false);
        //        }
        //    }
        //}

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

        public override void SoftStop()
        {
            base.SoftStop();

            ReturnLegsToInitialPositions(false);
        }

        private void ReturnLegsToInitialPositions(bool snapToPosition)
        {
            Debug.Log("returning legs to initial position");
            foreach (var l in legAnimators)
            {
                l.InitializeFootPosition(snapToPosition);
            }
        }

        //public void SetLegsReversed(bool reversed)
        //{
        //    foreach (var l in legAnimators)
        //    {
        //        l.Reversed = reversed;
        //    }
        //}
    }
}