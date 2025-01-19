using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AIMovementController : AdvancedMovementController
    {
        [SerializeField] bool dontMoveOverDropOffs;

        protected override void GroundedMoveAction(float input)
            //I am doing the drop off check here (rather than at the point where MoveInput is set)
            //so that orientation is accurate (without having to do an unecessary SetOrientation every time we
            //set MoveInput)
        {
            SetOrientation(input);

            if (dontMoveOverDropOffs && mover.DropOffInFront())
            {
                MoveInput = 0;//we may not want to do this because it triggers an event
                //but for now it's harmless
                return;
            }

            mover.MoveGrounded();
        }
    }
}