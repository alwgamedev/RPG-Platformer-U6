using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class MotherSpiderMovementController : AIMovementController
    {
        //[SerializeField] IKLimbAnimator punchingArm;
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
            }
        }

        protected override void Awake()
        {
            base.Awake();

            legAnimators = GetComponentsInChildren<IKLimbAnimator>();
        }

        private void SetLegsReversed(bool reversed)
        {
            foreach (var l in legAnimators)
            {
                l.SetReversed(reversed);
            }
        }

        //we don't need to be computing speed every frame, bc it's not used for anything
        protected override void AnimateMovement() { }
    }
}